using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using GloomhavenFixes.Patches;

namespace GloomhavenFixes
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "com.Leopard2ARC.gloomhaven.fixes";
        public const string NAME = "Gloomhaven Fixes & Mod";
        public const string VERSION = "0.0.0";

        internal static Harmony HarmonyInstance;

        void Awake()
        {
            // Use BepInEx.cfg in the config directory instead of creating a separate file
            string bepInExConfigPath = Path.Combine(Paths.ConfigPath, "BepInEx.cfg");
            ConfigFile coreConfig = new ConfigFile(bepInExConfigPath, true);

            // Initialize Config using the core config file
            ModConfig.Init(coreConfig);

            HarmonyInstance = new Harmony(GUID);

            // Load modules based on configuration
            
            // 1. No Burn Module
            if (ModConfig.EnableNoBurn.Value)
            {
                Logger.LogInfo("[Mod] Loading Module: NoBurn (No card burn on rest)");
                HarmonyInstance.PatchAll(typeof(NoBurnFixes));
            }

            // 2. Compile Fix Module
            if (ModConfig.EnableCompileFix.Value)
            {
                Logger.LogInfo("[Mod] Loading Module: Compile Fix (Prevent mod file deletion)");
                HarmonyInstance.PatchAll(typeof(CompileFixes));
            }

            // 3. Save/Load Fix Module (Highly recommended)
            if (ModConfig.EnableSaveLoadFix.Value)
            {
                Logger.LogInfo("[Mod] Loading Module: Save/Load Fixes (Prevent corrupted saves/progress loss)");
                HarmonyInstance.PatchAll(typeof(SaveLoadFixes));
            }
            
            Logger.LogInfo($"{NAME} v{VERSION} Loaded successfully!");
        }
    }
}