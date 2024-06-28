using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx.Logging;
using Gameplay;
using Gameplay.Abilities;
using Gameplay.Items.Data;
using Gameplay.Items.Settings;
using Gameplay.Values;
using Platforms;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Metalify : IMod
{
    public string Name => "ItemsPlus";
    private List<GameObject> trackedItems = new List<GameObject>(); // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
    private CoroutineHelper coroutineHelper; // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
    private string Custom; // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING

    public void OnLoad()
    {
        ModLoader.Logger.LogInfo("Items+ Mod has been loaded!");

        WoodMagnetCreate(); // Item creation Function
        LongSwordCreate(); // Item Creation Function
    
        GameObject helperObject = new GameObject("CoroutineHelper"); // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
        coroutineHelper = helperObject.AddComponent<CoroutineHelper>(); // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
        coroutineHelper.Initialize(this); // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING

        coroutineHelper.StartCoroutine(WaitForItemsToSpawn()); // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
        coroutineHelper.StartCoroutine(CheckForNewItems()); // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
    }

    private IEnumerator WaitForItemsToSpawn() // DONT TOUCH 
    {
        
        while (!FindAndTrackItems())
        {
            yield return new WaitForSeconds(1f);
        }

        coroutineHelper.StartCoroutine(MonitorTrackedItems());
    } // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
    private bool FindAndTrackItems() // DONT TOUCH 
    {
       
        
        var items = GameObject.FindObjectsOfType<MonoBehaviour>().Where(x => x.GetType().ToString() == "Gameplay.Items.PickupItem").Select(x => x.gameObject).ToList();

        if (items.Count == 0)
        {
            return false;
        }

        foreach (var item in items)
        {
            if (!trackedItems.Contains(item))
            {
                Gameplay.Items.PickupItem pickupItem = item.GetComponent<Gameplay.Items.PickupItem>();
                if (pickupItem)
                {
                    Type type = pickupItem.GetType();
                    FieldInfo dataField = type.GetField("Data", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (dataField != null)
                    {
                        PickupItemData pickupItemData = (PickupItemData)dataField.GetValue(pickupItem);
                        if (pickupItemData != null)
                        {
                            PickupItemSetting pickupItemSetting = pickupItemData.Setting;
                            if (pickupItemSetting != null)
                            {
                                ItemMaterialSetting itemMaterialSetting = pickupItemSetting.Material;
                                if (itemMaterialSetting != null)
                                {
                                    string custom = itemMaterialSetting.Name.PlaceHolderString;

                                    if (custom == Custom)
                                    {
                                        ModLoader.Logger.LogInfo("Found GameObject with custom string: " + pickupItem.gameObject.name);
                                        
                                        FieldInfo spriteField = type.GetField("_spriteRenderer", BindingFlags.NonPublic | BindingFlags.Instance);
                                        if (spriteField == null)
                                        {
                                            Debug.LogError("[MOD] Field '_spriteRenderer' not found in type.");
                                        }

                                        SpriteRenderer itemRenderer = (SpriteRenderer)spriteField.GetValue(pickupItem);
                                        if (itemRenderer == null)
                                        {
                                            Debug.LogError("[MOD] '_spriteRenderer' field value is null.");
                                        }

                                        FieldInfo scalefield = type.GetField("_scale", BindingFlags.NonPublic | BindingFlags.Instance);
                                        Vector3 scale = (Vector3)scalefield.GetValue(pickupItem);
                                        scale = new Vector3(0.4f, 0.4f, 0.4f);
                                        scalefield.SetValue(pickupItem, scale);

                                        Bounds bounds = itemRenderer.sprite.bounds;

                                        FieldInfo colliderField = type.GetField("MainCollider", BindingFlags.NonPublic | BindingFlags.Instance);
                                        if (colliderField == null)
                                        {
                                            Debug.LogError("[MOD] Field 'MainCollider' not found in type.");
                                        }

                                        Collider2D collider = (Collider2D)colliderField.GetValue(pickupItem);
                                        if (collider == null)
                                        {
                                            Debug.LogError("[MOD] 'MainCollider' field value is null.");
                                        }

                                        collider.enabled = false;

                                        BoxCollider2D boxCollider = pickupItem.gameObject.AddComponent<BoxCollider2D>();
                                        if (boxCollider != null)
                                        {
                                            Vector2 newSize = new Vector2(bounds.size.x, bounds.size.y);
                                            boxCollider.size = newSize;
                                            colliderField.SetValue(pickupItem, boxCollider);

                                            Debug.Log("[MOD] Setup BoxCollider on Custom Object");
                                        }
                                        else
                                        {
                                            Debug.LogError("[MOD] Failed to add BoxCollider2D to pickupItem.");
                                            collider.enabled = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                trackedItems.Add(item);
            }
        }

        ModLoader.Logger.LogInfo($"Found and tracked {trackedItems.Count} items.");
        return true;
    } // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
    
    private IEnumerator MonitorTrackedItems() // DONT TOUCH 
    {
        while (true)
        {
            trackedItems.RemoveAll(item => item == null);

            if (trackedItems.Count == 0)
            {
                ModLoader.Logger.LogInfo("All tracked items have been destroyed. Starting search again.");
                coroutineHelper.StartCoroutine(WaitForItemsToSpawn());
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }
    } // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING

    private IEnumerator CheckForNewItems() // DONT TOUCH 
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            FindAndTrackItems();
        }
    } // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
    
    private PickupItemData WoodMagnetCreate()
    {

        string str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ""); // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
        Texture2D texture2D = LoadTextureFromFile(str + "/mod_star.png"); // This is the icon that shows in the item overview and tooltip in the bottom left
        Sprite image = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), Vector2.one * 0.5f); // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
        
        Texture2D texture2D2 = LoadTextureFromFile(str + "/wood_glue.png"); // This is what your sprite actually looks like
        Sprite image2 = Sprite.Create(texture2D2, new Rect(0f, 0f, (float)texture2D2.width, (float)texture2D2.height), Vector2.one * 0.5f);  // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
       
        PickupItemSetting pickupItemSetting = ScriptableObject.CreateInstance<PickupItemSetting>(); // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING

        pickupItemSetting.Name.PlaceHolderString = "Wood Magnet"; // Your item name
        pickupItemSetting.Description.PlaceHolderString = "All in the name."; // Description
        pickupItemSetting.Image = image2; // what your sprite actually looks like, check line 182
        pickupItemSetting.Type = 0; // leave as 0
        pickupItemSetting.Group = 0; // leave as 0
        pickupItemSetting.Rarity = (EItemRarity)1; // what rarity you want the item to be. 1 = Normal 2 = Rare and 3 = DontDrop
        pickupItemSetting.Properties = 0; // leave as 0
        pickupItemSetting.CanBeUpgraded = false; // If the item can be upgraded
        pickupItemSetting.HasNewDescription = true; // leave as true

        EItemTag item = (EItemTag)6; // leave as is
        pickupItemSetting.Tags.Add(item);
        // Item effects start // 
        ProximityEffect WoodGlue = new ProximityEffect();
        WoodGlue.Magnetic.Enabled = true;
        int layerNumber = 15;
        int layerMask = 1 << layerNumber;
        WoodGlue.Magnetic.Layer = layerMask;
        Gameplay.Values.Number Sreefhf = new Number();
        Sreefhf.Value = 10.0f;
        WoodGlue.Magnetic.Strength = Sreefhf;
        Gameplay.Values.Number Disscale = new Number();
        Disscale.Value = 5.0f;
        WoodGlue.Magnetic.DistanceScale = Disscale;
        WoodGlue.Magnetic.ForceMode = EffectorForceMode2D.InverseSquared;
        WoodGlue.Magnetic.Radius = Disscale;
        
        WoodGlue.ConsumeItems.Enabled = false;
        WoodGlue.ItemEffect.Enabled = false;
        WoodGlue.ItemReplacement.Enabled = false;
        // item effects end // 
        pickupItemSetting.ProximityEffects.Add(WoodGlue);
        pickupItemSetting.Variants = null;
        pickupItemSetting.CustomColor = false;
        pickupItemSetting.IsStackable = true;
        pickupItemSetting.MachineEffects = null;
        pickupItemSetting.PerkAbilities = null;
        pickupItemSetting.PetAbilities = null;
        pickupItemSetting.RepeatAbilities = false;
        pickupItemSetting.UpgradeDescription = null;
        pickupItemSetting.UpgradeValues = null;
        pickupItemSetting.HasCircleCollider = false;
        pickupItemSetting.HasImageVariants = false;
        pickupItemSetting.HasNewDescription = false;
        pickupItemSetting.HasPetAbilities = false;
        pickupItemSetting.RepeatPetAbilities = false;
        pickupItemSetting.SpecialPerkEffects = null;
        ModLoader.Logger.LogInfo("Assigned basic properties to itemSetting.");
        pickupItemSetting.Material = ScriptableObject.CreateInstance<ItemMaterialSetting>();
        pickupItemSetting.Material.Name.PlaceHolderString = "- Custom Item -"; // DO NOT TOUCH OTHERWISE YOUR ITEM WILL NOT WORK
        pickupItemSetting.Material.Description.PlaceHolderString = "Wood"; // Whatever u want here rlly
        pickupItemSetting.Material.Image = image;
        pickupItemSetting.Material.TextColor = Color.cyan;
        pickupItemSetting.Material.Density = 2f;
        pickupItemSetting.Material.Layer = 15; // layer of the item. 15 is wood and 14 is metal. if you want anymore reverse the game.
        ModLoader.Logger.LogInfo("Assigned material properties to itemSetting.");
        // DONT TOUCH START //
        GameConfiguration configuration = Runtime.Configuration;
        ModLoader.Logger.LogInfo("Searched for Gameplay.GameConfiguration component.");
        if (configuration != null)
        {
            ModLoader.Logger.LogInfo("Found Gameplay.GameConfiguration component!");
            PickupItemSetting[] array = new PickupItemSetting[configuration.Items.Length + 1];
            for (int i = 0; i < configuration.Items.Length; i++)
            {
                array[i] = configuration.Items[i];
            }
            array[array.Length - 1] = pickupItemSetting;
            ModLoader.Logger.LogInfo(string.Format("Items count before adding: {0}", configuration.Items.Length));
            configuration.Items = array;
            ModLoader.Logger.LogInfo("Added new item to gameConfig.Items.");
            ModLoader.Logger.LogInfo(string.Format("Items count after adding: {0}", configuration.Items.Length));
        }
        else
        {
            ModLoader.Logger.LogInfo("Gameplay.GameConfiguration component not found.");
        }
        StartItem startItem;
        startItem.Item = pickupItemSetting;
        startItem.Amount = 1;
        startItem.Upgraded = false;
        PickupItemData pickupItemData = pickupItemSetting.GenerateData(false);

        Custom = pickupItemSetting.Material.Name.PlaceHolderString;

        pickupItemSetting.GetName(pickupItemData);
        ModLoader.Logger.LogInfo("Generated item data.");
        string name = pickupItemSetting.GetName(pickupItemData);
        string description = pickupItemSetting.GetDescription(pickupItemData);
        ModLoader.Logger.LogInfo("Retrieved item properties.");
        ModLoader.Logger.LogInfo("Item Name: " + name);
        ModLoader.Logger.LogInfo("Item Description: " + description);
        return pickupItemData;
        // DONT TOUCH END //
    } // Item with properties that effect other items
    private PickupItemData LongSwordCreate()
    {
        
        // I cant be bothered to comment all of this one, its pretty much the same as the woodmagnet. Scroll down and read my comments
        
        ModLoader.Logger.LogInfo("Starting itemcreate method.");
        string str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "");
        Texture2D texture2D = LoadTextureFromFile(str + "/mod_star.png");
        Sprite image = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), Vector2.one * 0.5f);
        Texture2D texture2D2 = LoadTextureFromFile(str + "/long_sword.png");
        Sprite image2 = Sprite.Create(texture2D2, new Rect(0f, 0f, (float)texture2D2.width, (float)texture2D2.height), Vector2.one * 0.5f);
        ModLoader.Logger.LogInfo("Created brownSprite.");
        PickupItemSetting pickupItemSetting = ScriptableObject.CreateInstance<PickupItemSetting>();
        ModLoader.Logger.LogInfo("Created itemSetting instance.");
        pickupItemSetting.Name.PlaceHolderString = "Long Sword";
        pickupItemSetting.Description.PlaceHolderString = "Deals 50 Damage but also 30 Damage to yourself.";
        pickupItemSetting.Image = image2;
        pickupItemSetting.Type = EPickupItemType.Combat;
        pickupItemSetting.Group = EItemGroup.None;
        pickupItemSetting.Rarity = (EItemRarity)1;
        pickupItemSetting.Properties = EItemProperty.None;
        pickupItemSetting.CanBeUpgraded = false;
        pickupItemSetting.HasNewDescription = true;
        BaseValue baseValue = new BaseValue();
        
        Ability LongSword = new Ability();
        Ability LongSwordInflict = new Ability();
        Gameplay.Values.Number DMGammount = new Number();
        DMGammount.Value = 50.0f; // The damage the sword does to enemies
        Gameplay.Values.Number InflictAmmount = new Number();
        InflictAmmount.Value = 30.0f; // damage the sword does to the player
        
        baseValue.Name = "DamageAmount"; // dont change
        baseValue.Persistent = false; // dont change
        baseValue.Value = 50.0; // change to whatever you want the damage to enemies to be
        pickupItemSetting.StartValues.Add(baseValue);
        
        
        LongSword.Damage.Enabled = true; // dont change
        LongSword.Damage.DamageAmount = DMGammount; // dont change
        LongSword.Damage.Target = Ability.EAbilityTarget.FirstTarget; // dont change 
        
        LongSwordInflict.Damage.Enabled = true; // set to false if you dont want to attack yourself
        LongSwordInflict.Damage.DamageAmount = InflictAmmount;
        LongSwordInflict.Damage.Target = Ability.EAbilityTarget.Self;
        
        pickupItemSetting.Abilities.Add(LongSword);
        pickupItemSetting.Abilities.Add(LongSwordInflict);
        
        EItemTag damage = EItemTag.Damage;
        EItemTag selfinflic = EItemTag.SelfDamage;
        pickupItemSetting.Tags.Add(damage);
        pickupItemSetting.Tags.Add(selfinflic);
        pickupItemSetting.Variants = null;
        pickupItemSetting.CustomColor = false;
        pickupItemSetting.IsStackable = true;
        pickupItemSetting.MachineEffects = null;
        pickupItemSetting.PerkAbilities = null;
        pickupItemSetting.PetAbilities = null;
        pickupItemSetting.RepeatAbilities = false;
        pickupItemSetting.UpgradeDescription = null;
        pickupItemSetting.UpgradeValues.Add(baseValue);
        pickupItemSetting.HasCircleCollider = false;
        pickupItemSetting.HasImageVariants = false;
        pickupItemSetting.HasNewDescription = false;
        pickupItemSetting.HasPetAbilities = false;
        pickupItemSetting.RepeatPetAbilities = false;
        pickupItemSetting.SpecialPerkEffects = null;
        ModLoader.Logger.LogInfo("Assigned basic properties to itemSetting.");
        pickupItemSetting.Material = ScriptableObject.CreateInstance<ItemMaterialSetting>();
        pickupItemSetting.Material.Name.PlaceHolderString = "- Custom Item -"; // DONT TOUCH OTHERWISE YOULL BREAK THE GAME
        pickupItemSetting.Material.Description.PlaceHolderString = "Metal";
        pickupItemSetting.Material.Image = image;
        pickupItemSetting.Material.TextColor = Color.cyan;
        pickupItemSetting.Material.Density = 2f;
        pickupItemSetting.Material.Layer = 14;
        ModLoader.Logger.LogInfo("Assigned material properties to itemSetting.");
        GameConfiguration configuration = Runtime.Configuration;
        ModLoader.Logger.LogInfo("Searched for Gameplay.GameConfiguration component.");
        if (configuration != null)
        {
            ModLoader.Logger.LogInfo("Found Gameplay.GameConfiguration component!");
            PickupItemSetting[] array = new PickupItemSetting[configuration.Items.Length + 1];
            for (int i = 0; i < configuration.Items.Length; i++)
            {
                array[i] = configuration.Items[i];
            }
            array[array.Length - 1] = pickupItemSetting;
            ModLoader.Logger.LogInfo(string.Format("Items count before adding: {0}", configuration.Items.Length));
            configuration.Items = array;
            ModLoader.Logger.LogInfo("Added new item to gameConfig.Items.");
            ModLoader.Logger.LogInfo(string.Format("Items count after adding: {0}", configuration.Items.Length));
        }
        else
        {
            ModLoader.Logger.LogInfo("Gameplay.GameConfiguration component not found.");
        }
        StartItem startItem;
        startItem.Item = pickupItemSetting;
        startItem.Amount = 1;
        startItem.Upgraded = false;
        PickupItemData pickupItemData = pickupItemSetting.GenerateData(false);

        Custom = pickupItemSetting.Material.Name.PlaceHolderString;

        pickupItemSetting.GetName(pickupItemData);
        ModLoader.Logger.LogInfo("Generated item data.");
        string name = pickupItemSetting.GetName(pickupItemData);
        string description = pickupItemSetting.GetDescription(pickupItemData);
        ModLoader.Logger.LogInfo("Retrieved item properties.");
        ModLoader.Logger.LogInfo("Item Name: " + name);
        ModLoader.Logger.LogInfo("Item Description: " + description);
        return pickupItemData;
    } // Basic weapon that deals damage

    private Texture2D LoadTextureFromFile(string filePath)
    {
        Texture2D texture2D = null;
        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(fileData);
        }
        else
        {
            ModLoader.Logger.LogError($"Texture file not found at path: {filePath}");
        }
        return texture2D;
    }  // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
    
}

public class CoroutineHelper : MonoBehaviour
{
    private Metalify metalify;

    public void Initialize(Metalify metalify)
    {
        this.metalify = metalify;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        metalify.OnLoad();
    }
    
} // DO NOT TOUCH UNLESS YOU KNOW WHAT YOUR DOING
