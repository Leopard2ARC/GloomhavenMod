using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Assets.Script.GUI.MainMenu.Modding;
using ScenarioRuleLibrary.YML;

namespace GloomhavenFixes.Patches
{
    public static class CompileFixes
    {
        // Prepare reflection tools for setting private set properties
        static readonly MethodInfo AbilityCardsSetter = AccessTools.PropertySetter(typeof(GHRuleset), "CompiledAbilityCards");
        static readonly MethodInfo ItemCardsSetter = AccessTools.PropertySetter(typeof(GHRuleset), "CompiledItemCards");

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GHRuleset), "CompileRuleset")]
        public static bool CompileRuleset_Prefix(GHRuleset __instance, ref bool __result)
        {
            try
            {
                // Logic copied from original, starting modification injection
                
                if (__instance.LinkedModNames.Count == 0)
                {
                    Debug.LogError("Unable to compile ruleset " + __instance.Name + " as it contains no linked mods");
                    __result = false;
                    return false; // Skip original method
                }

                // --- Phase 1: Handle RemoveYML ---
                RemoveYML removeYML = new RemoveYML();
                List<string> filesToRemoveList = new List<string>();
                
                // LinkedMods is a public property, access directly
                var linkedMods = __instance.LinkedMods;
                foreach (GHMod ghMod in linkedMods)
                {
                    // Validate Mod, do not write files
                    ghMod.Validate(false);

                    string[] files = PlatformLayer.FileSystem.GetFiles(ghMod.ModdedYMLDirectory, "*.yml", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        using (StreamReader streamReader = new StreamReader(file))
                        {
                            string parserLine = streamReader.ReadLine();
                            // Reference SceneController (located in GH.Runtime namespace)
                            if (SceneController.Instance.YML.GetParserType(parserLine, file) == YMLLoading.EYMLParserType.RemoveYML)
                            {
                                removeYML.ProcessFile(streamReader, file);
                            }
                        }
                    }
                }

                // Validate if there are any invalid Mods
                if (linkedMods.Any((GHMod a) => !a.IsValid))
                {
                    Debug.LogError("Unable to compile ruleset " + __instance.Name + " as one or more mods included are not valid.");
                    __result = false;
                    return false;
                }

                // --- Phase 2: Prepare Directory ---
                string compileFolder = __instance.RulesetCompileFolder;
                Debug.Log("[NoBurnMod] Creating RulesetCompileFolder: " + compileFolder);
                
                if (PlatformLayer.FileSystem.ExistsDirectory(compileFolder))
                {
                    PlatformLayer.FileSystem.RemoveDirectory(compileFolder, true);
                }
                PlatformLayer.FileSystem.CreateDirectory(compileFolder);

                // Use reflection to clear/initialize List
                AbilityCardsSetter.Invoke(__instance, new object[] { new List<string>() });
                ItemCardsSetter.Invoke(__instance, new object[] { new List<string>() });
                __instance.CompiledHash = null;

                // Collect list of files to remove
                foreach (RemoveYMLData item in removeYML.LoadedYML)
                {
                    filesToRemoveList.AddRange(item.FilesToRemove);
                    filesToRemoveList.Add(Path.GetFileName(item.FileName));
                }

                // --- Phase 3: Export Base Files ---
                if (GHMod.ExportYML(compileFolder, __instance.RulesetType))
                {
                    List<string> processedFiles = new List<string>();
                    
                    // --- Phase 4: Copy Mod Files (Put into Z directory) ---
                    // Re-acquire to prevent changes
                    linkedMods = __instance.LinkedMods; 
                    foreach (GHMod ghMod2 in linkedMods)
                    {
                        string targetDir = Path.Combine(compileFolder, "Z", ghMod2.MetaData.Name);
                        PlatformLayer.FileSystem.CreateDirectory(targetDir);
                        
                        // Filter .yml and .lvldat
                        foreach (string file in ghMod2.MetaData.AppliedFiles.Where((string w) => w.EndsWith(".yml") || w.EndsWith(".lvldat")))
                        {
                            if (file.EndsWith(".yml"))
                            {
                                processedFiles.Add(file);
                            }
                            // Copy and overwrite: false is original logic
                            PlatformLayer.FileSystem.CopyFile(file, Path.Combine(targetDir, Path.GetFileName(file)), false);
                        }
                    }

                    // --- Phase 5: Execute File Deletion (Critical Fix Location) ---
                    // -----------------------------------------------------------
                    foreach (string fileToRemove in filesToRemoveList)
                    {
                        string[] files = PlatformLayer.FileSystem.GetFiles(compileFolder, fileToRemove, SearchOption.AllDirectories);
                        foreach (string path in files)
                        {
                            // [Fix]: Added safety check, never delete files in Z (Mod) directory
                            // Original Bug: GetFiles recursively found Mod files just copied and deleted them
                            bool isModFile = path.IndexOf("\\Z\\", StringComparison.OrdinalIgnoreCase) >= 0 || 
                                             path.IndexOf("/Z/", StringComparison.OrdinalIgnoreCase) >= 0;

                            if (!isModFile)
                            {
                                // Only delete non-Mod files
                                PlatformLayer.FileSystem.RemoveFile(path);
                            }
                            else 
                            {
                                Debug.Log($"[NoBurnMod] Compile Fix: Protected Mod file from accidental deletion by RemoveYML: {path}");
                            }
                        }
                    }
                    // -----------------------------------------------------------

                    // --- Phase 6: Re-collect Metadata and Package ---
                    Dictionary<string, List<string>> filesByParser;
                    if (SceneController.Instance.YML.GetYMLParserTypes(processedFiles.ToArray(), out filesByParser))
                    {
                        if (filesByParser.ContainsKey(YMLLoading.EYMLParserType.AbilityCard.ToString()))
                        {
                             __instance.CompiledAbilityCards.AddRange(
                                 filesByParser[YMLLoading.EYMLParserType.AbilityCard.ToString()]
                                 .Select(s => Path.GetFileName(s))
                             );
                        }
                        if (filesByParser.ContainsKey(YMLLoading.EYMLParserType.ItemCard.ToString()))
                        {
                             __instance.CompiledItemCards.AddRange(
                                 filesByParser[YMLLoading.EYMLParserType.ItemCard.ToString()]
                                 .Select(s => Path.GetFileName(s))
                             );
                        }

                        // Call YML.CompileRuleset to generate final ZIP
                        if (SceneController.Instance.YML.CompileRuleset(compileFolder, __instance.RulesetCompiledZip, null))
                        {
                            __instance.CompiledHash = __instance.GetRulesetHash();
                            if (__instance.CompiledHash != string.Empty)
                            {
                                __result = __instance.Save();
                                return false; // Success, skip original method
                            }
                        }
                    }
                }
                __result = false;
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception running CompileRuleset (Patched).\n" + ex.Message + "\n" + ex.StackTrace);
                
                // Clean up residual directory
                string folder = __instance.RulesetCompileFolder;
                if (PlatformLayer.FileSystem.ExistsDirectory(folder))
                {
                    try
                    {
                        PlatformLayer.FileSystem.RemoveDirectory(folder, true);
                    }
                    catch { }
                }

                // Reset data (Reflection set)
                AbilityCardsSetter.Invoke(__instance, new object[] { new List<string>() });
                ItemCardsSetter.Invoke(__instance, new object[] { new List<string>() });
                __instance.CompiledHash = null;
                
                __result = false;
                return false;
            }
        }
    }
}