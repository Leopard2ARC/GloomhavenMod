# **Gloomhaven Fixes & NoBurn Mod**

A BepInEx plugin for **Gloomhaven (Digital)** that fixes critical bugs related to Custom Rulesets (Mods) and offers an optional "No Burn" cheat feature.

## **Overview**

This mod serves two main purposes:

1. **Bug Fixes:** It patches critical issues in the base game's code regarding how Custom Rulesets are compiled and how modded saves are handled. These fixes are essential for anyone playing with or developing mods.  
2. **Gameplay Modification (Optional):** It includes a "No Burn" mode where Long and Short rests return discarded cards to your hand instead of burning one, allowing for infinite play (cheat).

## **Features**

### **🛠️ Core Fixes (Highly Recommended)**

* **Compile Fix:** Prevents the game from accidentally deleting your source mod files (in the Z folder) when compiling a Custom Ruleset.  
* **Save/Load Fix:** Fixes serious bugs where:  
  * Modded saves would disappear from the menu due to incorrect save name parsing.  
  * Resume buttons would fail to appear for modded games.  
  * Saves could be lost due to file naming conflicts.  
  * *Includes an auto-scan feature on startup to recover "missing" mod saves.*

### **🎮 Gameplay Mods**

* **No Burn Mode:** \* **Short Rest:** Does not burn a random card. All discarded cards return to hand.  
  * **Long Rest:** Does not burn a selected card. All discarded cards return to hand.

## **Installation**

1. Download the latest release zip file.  
2. Extract the contents directly into your **Gloomhaven game directory** (where GH.exe is located).  
3. Ensure your folder structure looks like this:  
   Gloomhaven/  
   ├── GH.exe  
   ├── winhttp.dll  
   ├── doorstop\_config.ini  
   ├── .doorstop\_version  
   └── BepInEx/  
       ├── core/  
       ├── plugins/  
       │   └── GloomhavenFixes.dll  
       └── ...

4. Run the game.

## **Configuration**

This mod appends its configuration settings to the standard BepInEx.cfg file.

Location: Gloomhaven\\BepInEx\\config\\BepInEx.cfg

Open the file with a text editor and look for the \[Modules\] section:

\[Modules\]

\#\# Enable No Burn: Long and Short rests no longer destroy cards (Cheat).  
\# Setting type: Boolean  
\# Default value: false  
EnableNoBurn \= false

\#\# Enable Compile Fix: Fixes the issue where the game incorrectly deletes Mod source files when compiling a Custom Ruleset (Recommended).  
\# Setting type: Boolean  
\# Default value: true  
EnableCompileFix \= true

\#\# Enable Save/Load Fix: Fixes critical bugs where Custom Ruleset saves fail to appear or save name parsing errors cause data loss (Highly Recommended).  
\# Setting type: Boolean  
\# Default value: true  
EnableSaveLoadFix \= true

## **Build Instructions**

### **Prerequisites**

* Visual Studio or MSBuild tools.  
* Gloomhaven game files (for references).

### **Setup**

1. Clone this repository.  
2. **Important:** Open GloomhavenNoBurnMod.csproj in a text editor.  
3. Locate the \<GloomhavenRoot\> tag and change the path to your local Gloomhaven game directory:  
   \<PropertyGroup\>  
     \<GloomhavenRoot\>C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gloomhaven\</GloomhavenRoot\>  
     \<\!-- ... \--\>  
   \</PropertyGroup\>

### **Building via Command Line**

**Debug Build (with detailed logging):**

MSBuild GloomhavenNoBurnMod.csproj /v:detailed /fl /flp:logfile=build-detail.log /p:Configuration=Debug

**Release Build:**

MSBuild GloomhavenNoBurnMod.csproj /p:Configuration=Release

## **Credits**

* Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching.  
* Built on [BepInEx](https://github.com/BepInEx/BepInEx).

## **License**

[MIT License](https://www.google.com/search?q=LICENSE)