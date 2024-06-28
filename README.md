# Dungeon Clawler Mod Handler

![Dungeon Clawler Mod Handler](https://your-image-link.com/banner.png)

A **BepInEx** mod handler/loader for the game **Dungeon Clawler**.

## 🚀 Installation

1. **Download BepInEx**:
    - Get the latest release of BepInEx [here](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.2).
    - Follow the BepInEx installation instructions [here](https://docs.bepinex.dev/articles/user_guide/installation/index.html).

2. **Download Mod Handler**:
    - Download the latest release of `DC_ModHandler.zip` and extract it.
    - Place all contents of the Mod Handler zip into the `plugins` folder of BepInEx.

3. **Run the Game**:
    - Launch the game.
    - You should see a new "Mods" button where all your installed mods will be listed.

## 🛠️ Mod Creation

Creating mods for Dungeon Clawler becomes more streamlined with this handler, especially when managing multiple mods. Although the handler currently doesn't simplify the mod creation process itself, future updates will include a custom API to facilitate creating game elements.

### 🔧 Sample Mod Project

A Sample Mod project is available in this GitHub repository called **DC-Sample Mod**. It's ready for compilation and use with the handler. This serves as a template to help you get started.

### 🛡️ Sample Item Mod Project

A Sample Item Mod project is available in this GitHub repository called **DC-SampleItem**. It's ready for compilation and use with the handler. This serves as a template to help you get started making items.

### 📂 Mod Structure

Each mod must be in its own folder within the `Mods` directory. For example:

```
GameFolder/Mods/SampleMod
```

Each mod folder must contain a manifest file named `Manifest.txt` with the following format:

```plaintext
name="Name of the mod"
description="Description of the mod"
version="1.0"
author="Author of the mod"
mod="SampleMod.dll"
```

Additionally, the mod folder should include the mod's DLL file as specified in the manifest.

### 🚀 Loading Your Mod

Once your mod is set up:
1. Start the game.
2. Open the mods list from the "Mods" button.
3. Your mod should be listed and ready to use.

---

Happy modding! 🎮✨
