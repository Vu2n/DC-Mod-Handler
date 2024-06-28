# Dungeon Clawler Mod Handler

![Dungeon Clawler Mod Handler](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2356780/header.jpg?t=1719496518)

A **BepInEx** mod handler/loader for the game **Dungeon Clawler**.

![GitHub release (latest by date)](https://img.shields.io/github/v/release/Vu2n/DC-Mod-Handler)
![GitHub](https://img.shields.io/github/license/Vu2n/DC-Mod-Handler)
![GitHub issues](https://img.shields.io/github/issues/Vu2n/DC-Mod-Handler)
![GitHub pull requests](https://img.shields.io/github/issues-pr/Vu2n/DC-Mod-Handler)

## 📑 Table of Contents

- [Installation](#-installation)
- [Mod Creation](#-mod-creation)
  - [Sample Mod Project](#-sample-mod-project)
  - [Sample Item Mod Project](#-sample-item-mod-project)
  - [Mod Structure](#-mod-structure)
  - [Loading Your Mod](#-loading-your-mod)
- [FAQs](#-faqs)
- [Support](#-support)
- [Contributing](#-contributing)
- [License](#-license)

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

## ❓ FAQs

**Q1: How do I troubleshoot common issues?**

A1: Check the BepInEx logs located in the `BepInEx` folder. Ensure all files are placed correctly and the game version is compatible.

**Q2: Can I create mods for other versions of the game?**

A2: Currently, the mod handler is optimized for the latest version of Dungeon Clawler.

## 🤝 Support

If you encounter any issues or have questions, please open an issue on the GitHub repository or get in contact with me on Dicsord @ perle.btr

## 📜 License

This project is licensed under the MIT License.

---

Happy modding! 🎮✨
