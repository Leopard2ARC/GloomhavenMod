using BepInEx.Configuration;

namespace GloomhavenFixes
{
    public static class ModConfig
    {
        public static ConfigEntry<bool> EnableNoBurn;
        public static ConfigEntry<bool> EnableCompileFix;
        public static ConfigEntry<bool> EnableSaveLoadFix;

        public static void Init(ConfigFile config)
        {
            // Bind settings to the "Modules" section in BepInEx.cfg
            
            EnableNoBurn = config.Bind("Modules", "EnableNoBurn", false, 
                "Enable No Burn: Long and Short rests no longer destroy cards (Cheat).");

            EnableCompileFix = config.Bind("Modules", "EnableCompileFix", true, 
                "Enable Compile Fix: Fixes the issue where the game incorrectly deletes Mod source files when compiling a Custom Ruleset (Recommended).");

            EnableSaveLoadFix = config.Bind("Modules", "EnableSaveLoadFix", true, 
                "Enable Save/Load Fix: Fixes critical bugs where Custom Ruleset saves fail to appear or save name parsing errors cause data loss (Highly Recommended).");
        }
    }
}