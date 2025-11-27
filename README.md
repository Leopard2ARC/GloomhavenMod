# **Gloomhaven Fixes & NoBurn Mod**

### **🎯 Stop Losing Your Saves. Play Your Way.**

A essential mod for **Gloomhaven (Digital)** that fixes game-breaking bugs and gives you control over how you play.

---

## **Why You Need This**

### **😤 Are You Experiencing These Problems?**

- ✅ **Your modded saves vanish from the load menu**
- ✅ **The Resume button randomly stops working**
- ✅ **Your custom mod files get deleted when compiling**
- ✅ **Save files conflict and corrupt your progress**
- ✅ **You want to experiment without permanently burning cards**

**This mod fixes all of these issues.**

---

## **What You Get**

### **🛡️ Critical Bug Fixes (Enabled by Default)**

**✓ Save Protection**  
Never lose your modded saves again. This fixes the broken save name parsing that makes your games disappear from the menu. Includes automatic recovery of "missing" saves on startup.

**✓ Resume Button Fix**  
Resume buttons now work correctly with custom rulesets.

**✓ Compile Safety**  
Prevents the game from accidentally deleting your mod source files when compiling custom rulesets.

### **🎮 Optional Gameplay Features**

**✓ No Burn Mode (Disabled by Default)**  
Play without the card burn mechanic:
- **Short Rest:** All discarded cards return to your hand (no random burn)
- **Long Rest:** All discarded cards return to your hand (no card selection needed)

Perfect for experimenting with builds, learning new classes, or just relaxing without pressure.

---

## **Key Advantages**

### **🔒 100% Safe & Reversible**

- **No game files modified** - Works alongside the vanilla game
- **Toggle any feature on/off** - Simple config file
- **Save/Load for backups** - Create restore points anytime
- **Uninstall cleanly** - Just delete the mod files

### **👥 For Everyone**

- **Hardcore players:** Get the bug fixes, disable the cheats
- **Casual players:** Enable No Burn for stress-free gameplay
- **Mod developers:** Essential fixes for custom ruleset development
- **Everyone:** Stop losing your saves!

---

## **Quick Start**

### **Step 1: Install (30 seconds)**

1. **Download** the latest release
2. **Extract** everything into your Gloomhaven folder (where `GH.exe` is)
3. **Launch** the game - that's it!

Your folder should look like:
```
Gloomhaven/
├── GH.exe
├── winhttp.dll
├── doorstop_config.ini
└── BepInEx/
    └── plugins/
        └── GloomhavenFixes.dll
```

### **Step 2: Configure (Optional)**

Want to enable No Burn mode or disable fixes? 

**Location:** `Gloomhaven\BepInEx\config\BepInEx.cfg`

```ini
[Modules]
## Bug Fixes (Recommended to keep enabled)
EnableCompileFix = true        # Prevents mod file deletion
EnableSaveLoadFix = true       # Fixes missing saves

## Gameplay Mods (Your choice)
EnableNoBurn = false           # Change to 'true' for infinite cards
```

**Edit with any text editor, save, restart the game.**

---

## **FAQ**

**Q: Will this break my existing saves?**  
A: No. The mod is designed to *fix* and *recover* saves, not break them.

**Q: Can I use this for just the bug fixes?**  
A: Absolutely! No Burn is disabled by default. You get the fixes automatically.

**Q: Is this cheating?**  
A: The bug fixes are not cheats - they fix broken game code. No Burn mode is optional and clearly marked as a cheat feature.

**Q: Does this work with other mods?**  
A: Yes, it's designed specifically to make custom rulesets work properly.

**Q: Can I turn features on/off mid-campaign?**  
A: Yes, just edit the config file and restart the game.

---

## **For Mod Developers**

If you're creating custom rulesets, the **Compile Fix** and **Save/Load Fix** are essential:

- **Compile Fix:** Stops the game from deleting your source files in the Z folder
- **Save/Load Fix:** Ensures your modded saves load correctly and don't conflict

These fixes solve fundamental issues in the game's mod compilation system.

---

## **Troubleshooting**

**Saves still missing?**  
The mod auto-scans on startup. If saves don't appear immediately, restart the game once more.

**Config file not found?**  
Launch the game once - BepInEx creates the config file on first run.

**Need help?**  
Check the Issues tab or create a new issue with your problem.

---

## **Technical Details**

- Built on **BepInEx** (industry-standard modding framework)
- Uses **Harmony** for safe runtime patching
- No permanent game modifications
- Open source under MIT License

---

## **Credits**

- [Harmony](https://github.com/pardeike/Harmony) - Runtime patching library
- [BepInEx](https://github.com/BepInEx/BepInEx) - Modding framework

---

## **Support This Project**

Found this helpful? ⭐ **Star this repository** to help others discover it!

---

**License:** [MIT License](LICENSE) - Free to use, modify, and share.