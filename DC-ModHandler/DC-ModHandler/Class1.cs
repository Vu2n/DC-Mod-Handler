using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[BepInPlugin("com.vu2n.modloader", "Vu2ns Mod Handler", "1.0.0")]
public class ModLoader : BaseUnityPlugin
{
    private const string ModsDirectory = "Mods";
    private List<ModInfo> modList = new List<ModInfo>();
    private GameObject modOverviewScreen;
    public static ManualLogSource Logger;
    private Texture2D ModStar;
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo("Mod Loader initialized");

        if (!Directory.Exists(ModsDirectory))
        {
            Directory.CreateDirectory(ModsDirectory);
            Logger.LogInfo($"Created mods directory at {ModsDirectory}");
        }
    }
    Texture2D LoadTextureFromFile(string filePath)
    {
        Texture2D texture = null;
        byte[] fileData;

        if (System.IO.File.Exists(filePath))
        {
            fileData = System.IO.File.ReadAllBytes(filePath);
            texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }

        return texture;
    }
    private void Start()
    {
        LoadModsFromDirectory();
        string filePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "mod_star.png");
        ModStar = LoadTextureFromFile(filePath);
        
        
        CreateModList(); // After finishing game, need to recall this method. Will add later.
        ExecuteMods();
    }

    private void LoadModsFromDirectory()
    {
        string[] modDirectories = Directory.GetDirectories(ModsDirectory);

        foreach (string modDir in modDirectories)
        {
            ModInfo modInfo = ParseModInfo(modDir);
            if (modInfo != null)
            {
                LoadMod(modInfo, modDir);
                modList.Add(modInfo);
            }
        }
    }

    private ModInfo ParseModInfo(string modDirectory)
    {
        string manifestPath = Path.Combine(modDirectory, "Manifest.txt");
        if (!File.Exists(manifestPath))
        {
            Logger.LogError($"Manifest file not found for mod at: {modDirectory}");
            return null;
        }

        ModInfo modInfo = new ModInfo();

        try
        {
            string[] manifestLines = File.ReadAllLines(manifestPath);

            foreach (string line in manifestLines)
            {
                if (line.StartsWith("name="))
                {
                    modInfo.Name = line.Substring("name=".Length).Trim('"');
                }
                else if (line.StartsWith("description="))
                {
                    modInfo.Description = line.Substring("description=".Length).Trim('"');
                }
                else if (line.StartsWith("version="))
                {
                    modInfo.Version = line.Substring("version=".Length).Trim('"');
                }
                else if (line.StartsWith("author="))
                {
                    modInfo.Author = line.Substring("author=".Length).Trim('"');
                }
                else if (line.StartsWith("mod="))
                {
                    modInfo.ModDllFileName = line.Substring("mod=".Length).Trim('"');
                }
            }

            if (string.IsNullOrEmpty(modInfo.Name) || string.IsNullOrEmpty(modInfo.ModDllFileName))
            {
                Logger.LogError($"Invalid Manifest.txt for mod at: {modDirectory}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error parsing Manifest.txt for mod at: {modDirectory}\nException: {ex}");
            return null;
        }

        return modInfo;
    }

    private void LoadMod(ModInfo modInfo, string modDirectory)
    {
        try
        {
            string modPath = Path.Combine(modDirectory, modInfo.ModDllFileName);
            if (!Path.IsPathRooted(modPath))
            {
                modPath = Path.GetFullPath(modPath);
            }
            Assembly modAssembly = Assembly.LoadFrom(modPath);
            Type modType = modAssembly.GetTypes().FirstOrDefault(t => typeof(IMod).IsAssignableFrom(t) && !t.IsInterface);

            if (modType != null)
            {
                modInfo.ModInstance = (IMod)Activator.CreateInstance(modType);
                modInfo.ModInstance.OnLoad();
                Logger.LogInfo($"Loaded mod: {modInfo.Name}");
            }
            else
            {
                Logger.LogError($"No valid mod class found in: {modPath}");
            }
        }
        catch (ReflectionTypeLoadException rtle)
        {
            Logger.LogError($"ReflectionTypeLoadException while loading mod from {modDirectory}: {rtle.Message}");
            foreach (var loaderException in rtle.LoaderExceptions)
            {
                Logger.LogError($"LoaderException: {loaderException.Message}");
            }
        }
        catch (TypeLoadException tle)
        {
            Logger.LogError($"TypeLoadException while loading mod from {modDirectory}: {tle.Message}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load mod from {modDirectory}: {ex.Message}");
        }
    }

    private void ExecuteMods()
    {
        foreach (ModInfo mod in modList)
        {
            // mod.ModInstance?.Execute();
        }
    }

    private void CreateModList()
    {
        GameObject discordButton = GameObject.Find("DiscordButton");
        if (discordButton == null)
        {
            Logger.LogError("DiscordButton not found!");
            return;
        }

        GameObject modListButton = GameObject.Instantiate(discordButton, discordButton.transform.parent);

        RectTransform rectTransform = modListButton.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition -= new Vector2(0, rectTransform.rect.height);
        }
        else
        {
            Logger.LogError("RectTransform not found on DiscordButton clone!");
        }

        modListButton.name = "ModListButton";
        SetText(modListButton, "Label", "Mods");
        
        Sprite modStarSprite = Sprite.Create(ModStar, new Rect(0, 0, ModStar.width, ModStar.height), Vector2.one * 0.5f);
        GameObject ModButtonImage = modListButton.transform.Find("Image").gameObject;
        ModButtonImage.GetComponent<UnityEngine.UI.Image>().sprite = modStarSprite;
        
        GameObject itemOverviewScreen = FindInactiveObject("ItemOverviewScreen");
        if (itemOverviewScreen == null)
        {
            Logger.LogError("ItemOverviewScreen not found!");
            return;
        }

        modOverviewScreen = GameObject.Instantiate(itemOverviewScreen, itemOverviewScreen.transform.parent);
        modOverviewScreen.name = "ModOverviewScreen";

        Button buttonComponent = modListButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(() =>
            {
                modOverviewScreen.SetActive(true);
                SetText(modOverviewScreen, "Title", "Loaded Mods");
                SetPerkItemListText(modOverviewScreen, "PerkList", "Details");
                SetPerkItemListText(modOverviewScreen, "ItemList", "Mods");
                DisplayMods(modOverviewScreen);
            });
        }
        else
        {
            Logger.LogError("Button component not found on ModListButton!");
        }
    }

    private void SetText(GameObject parent, string childName, string newText)
    {
        Transform childTransform = parent.transform.Find(childName);
        if (childTransform != null)
        {
            TextMeshProUGUI textComponent = childTransform.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = newText;
            }
            else
            {
                Logger.LogError("TextMeshProUGUI component not found on " + childName + "!");
            }
        }
        else
        {
            Logger.LogError(childName + " child GameObject not found on " + parent.name + "!");
        }
    }

    private void SetPerkItemListText(GameObject parent, string listName, string newText, int fontSize = 14)
    {
        Transform listTransform = parent.transform.Find(listName);

        Transform nextButton = listTransform.Find("NextButton");
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }

        Transform prevButton = listTransform.Find("PrevButton");
        if (prevButton != null)
        {
            prevButton.gameObject.SetActive(false);
        }

        if (listTransform != null)
        {
            Transform itemName = listTransform.Find("Items");
            TextMeshProUGUI textComponent = itemName.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = newText;
                textComponent.fontSize = fontSize;
            }
            else
            {
                Logger.LogError("TextMeshProUGUI component not found on Items of " + listName + "!");
            }
        }
        else
        {
            Logger.LogError(listName + " not found on " + parent.name + "!");
        }
    }

    private void DisplayMods(GameObject modOverviewScreen)
    {
        GameObject itemList = modOverviewScreen.transform.Find("ItemList").gameObject;
        if (itemList != null)
        {
            GameObject existingList = itemList.transform.Find("List").gameObject;
            if (existingList != null)
            {
                GameObject.Destroy(existingList);
            }

            GameObject list = new GameObject("List");
            list.transform.SetParent(itemList.transform);
            RectTransform listRectTransform = list.AddComponent<RectTransform>();
            listRectTransform.localScale = Vector3.one;
            listRectTransform.localPosition = Vector3.zero;
            listRectTransform.sizeDelta = new Vector2(400, 500);

            VerticalLayoutGroup layoutGroup = list.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 15f;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            
            foreach (ModInfo modInfo in modList)
            {
                GameObject modEntry = new GameObject(modInfo.Name);
                modEntry.transform.SetParent(list.transform);
                RectTransform rt = modEntry.AddComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.sizeDelta = new Vector2(400, 60);

                TextMeshProUGUI text = modEntry.AddComponent<TextMeshProUGUI>();
                TextMeshProUGUI titleText = modOverviewScreen.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                TMP_FontAsset font = titleText.font;
                
                text.fontSize = titleText.fontSize * 0.4f;
                text.alignment = titleText.alignment;
                text.color = titleText.color;
                text.fontStyle = titleText.fontStyle;
                text.font = font;
                text.text = modInfo.Name;

                EventTrigger eventTrigger = text.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
                pointerEnter.eventID = EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener((data) => { OnPointerEnterMod(text, modInfo, modOverviewScreen); });
                eventTrigger.triggers.Add(pointerEnter);

                EventTrigger.Entry pointerExit = new EventTrigger.Entry();
                pointerExit.eventID = EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((data) => { OnPointerExitMod(text, titleText.color); });
                eventTrigger.triggers.Add(pointerExit);
            }
        }
    }

    private void OnPointerEnterMod(TextMeshProUGUI text, ModInfo modInfo, GameObject modOverviewScreen)
    {
        text.color = Color.yellow;
        string details = $"Name: {modInfo.Name}\nDescription: {modInfo.Description}\nVersion: {modInfo.Version}\nAuthor: {modInfo.Author}";

        GameObject itemList = modOverviewScreen.transform.Find("PerkList").gameObject;
        if (itemList != null)
        {
            GameObject existingList = itemList.transform.Find("List").gameObject;
            if (existingList != null)
            {
                GameObject.Destroy(existingList);
            }

            GameObject list = new GameObject("List");
            list.transform.SetParent(itemList.transform);
            RectTransform listRectTransform = list.AddComponent<RectTransform>();
            listRectTransform.localScale = Vector3.one;
            listRectTransform.localPosition = Vector3.zero;
            listRectTransform.sizeDelta = new Vector2(400, 300);

            VerticalLayoutGroup layoutGroup = list.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 15f;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            TextMeshProUGUI titleText = modOverviewScreen.transform.Find("Title").GetComponent<TextMeshProUGUI>();

            GameObject modEntry = new GameObject("textDesc");
            modEntry.transform.SetParent(list.transform);
            RectTransform rt = modEntry.AddComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(400, 60);

            TextMeshProUGUI textdesc = modEntry.AddComponent<TextMeshProUGUI>();
            textdesc.text = details;
            textdesc.fontSize = 30.0f;
            textdesc.alignment = titleText.alignment;
            textdesc.color = titleText.color;
            textdesc.fontStyle = titleText.fontStyle;
            textdesc.font = titleText.font;
        }
    }

    private void OnPointerExitMod(TextMeshProUGUI text, Color originalColor)
    {
        text.color = originalColor;
        GameObject itemList = modOverviewScreen.transform.Find("PerkList").gameObject;
        if (itemList != null)
        {
            GameObject List = itemList.transform.Find("List").gameObject;
            if (List != null)
            {
                GameObject textDescObject = List.transform.Find("textDesc").gameObject;
                if (textDescObject != null)
                {
                    TextMeshProUGUI testdesccomp = textDescObject.GetComponent<TextMeshProUGUI>();
                    testdesccomp.text = "";
                }
            }
        }
    }

    private GameObject FindInactiveObject(string objectName)
    {
        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.hideFlags == HideFlags.None && t.name == objectName)
            {
                return t.gameObject;
            }
        }
        return null;
    }
}

public interface IMod
{
    void OnLoad();
}

public class ModInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public string Author { get; set; }
    public string ModDllFileName { get; set; }
    public IMod ModInstance { get; set; }
}
