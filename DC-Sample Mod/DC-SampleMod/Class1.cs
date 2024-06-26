using BepInEx.Logging;

public class SampleMod : IMod
{
    public string Name => "Sample Mod";

    public void OnLoad() // Function MUST be in the mod, This is what the handler calls first.
    {
        ModLoader.Logger.LogInfo("Sample Mod has been loaded!"); // Prints to the BepinEX console (If its enabled)
        ModStuff();
    }

    private void ModStuff()
    {
        // Do your Mod stuff here etc
        // I plan to work on an API to make creating items, characters and just anything really to make it easier but for now ill leave it to you smart cookies <3
    }
}