using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for OrderByDescending
using HarmonyLib;
using UnityEngine;
using GLOOM.MainMenu;

namespace GloomhavenFixes.Patches
{
    public static class SaveLoadFixes
    {
        // ------------------------------------------------------
        // Bug Fix 1: Save name parsing space error (Prevent save deletion)
        // ------------------------------------------------------
        [HarmonyPatch(typeof(GlobalData), "GetSaveNameInfo")]
        [HarmonyPrefix]
        public static bool GetSaveNameInfo_Prefix(string saveName, ref Tuple<string, string, bool, string> __result)
        {
            if (saveName.Contains("\\")) saveName = saveName.Substring(saveName.LastIndexOf("\\") + 1);
            if (saveName.Contains("//")) saveName = saveName.Substring(saveName.LastIndexOf("/") + 1);

            string[] array = saveName.Split('_');
            string partyName = string.Empty;
            string rulesetName = string.Empty;
            bool flag = false;

            for (int i = 1; i < array.Length - 1; i++)
            {
                string text3 = array[i];
                string[] array2 = text3.Split(new string[] { "[MOD]" }, StringSplitOptions.None);

                for (int j = 0; j < array2.Length; j++)
                {
                    if (array2[j] == "" && text3 != "") flag = !flag;
                    else if (flag) 
                    {
                        // [Critical Fix]: Prevent adding space at the beginning
                        if (rulesetName.Length > 0) rulesetName += " ";
                        rulesetName += array2[j];
                    }
                    else 
                    {
                        partyName += array2[j]; 
                        // Keep non-MOD segment concatenation logic
                        // Core purpose is to ensure RulesetName is clean
                        if (j < array2.Length -1) partyName += " ";
                    }
                }
                // Determine if space is needed between segments
                if (i < array.Length - 2 && !flag) partyName += " "; 
            }

            __result = new Tuple<string, string, bool, string>(partyName.Trim(), array[array.Length - 1], false, rulesetName);
            return false; 
        }

        // ------------------------------------------------------
        // Bug Fix 2: Startup automatic save scan (Recover "missing" saves)
        // ------------------------------------------------------
        [HarmonyPatch(typeof(SaveData), "LoadGlobalData")] 
        [HarmonyPostfix] 
        public static IEnumerator LoadGlobalData_Postfix(IEnumerator __result, SaveData __instance)
        {
            while (__result.MoveNext()) yield return __result.Current;

            if (__instance.Global != null)
            {
                 Debug.Log("[GloomFix] Performing save integrity scan...");
                 List<string> restoredLog = new List<string>();
                 yield return __instance.Global.ValidateSaves(EGameMode.Guildmaster, restoredLog);
                 yield return __instance.Global.ValidateSaves(EGameMode.Campaign, restoredLog);
                 if (restoredLog.Count > 0) Debug.Log($"[GloomFix] Found and restored: {string.Join(",", restoredLog.ToArray())}");
                 __instance.SaveGlobalData(); // Force save list
            }
        }

        // ------------------------------------------------------
        // Bug Fix 3: Resume Name persistence issue in Mod mode
        // ------------------------------------------------------
        [HarmonyPatch(typeof(SaveData), nameof(SaveData.LoadCampaignMode), new Type[] { 
            typeof(PartyAdventureData), typeof(bool), typeof(bool), typeof(System.Action), typeof(System.Action), typeof(bool) 
        })]
        [HarmonyPrefix]
        public static void LoadCampaignMode_Fix(PartyAdventureData partyData)
        {
            if (partyData != null && partyData.IsModded && SaveData.Instance.Global.ResumeCampaignName != partyData.PartyName)
            {
                SaveData.Instance.Global.ResumeCampaignName = partyData.PartyName;
                SaveData.Instance.SaveGlobalData(); 
            }
        }
        
        [HarmonyPatch(typeof(GlobalData), "get_ResumeCampaign")]
        [HarmonyPrefix]
        public static void GlobalData_get_ResumeCampaign_Fix(GlobalData __instance)
        {
            if (!string.IsNullOrEmpty(__instance.CurrentModdedRuleset) && string.IsNullOrEmpty(__instance.ResumeCampaignName))
            {
                var latest = __instance.ModdedCampaigns?.OrderByDescending(c => c.LastSavedTimeStamp).FirstOrDefault();
                if (latest != null)
                {
                    __instance.ResumeCampaignName = latest.PartyName;
                    SaveData.Instance.SaveGlobalData(); 
                }
            }
        }
    }
}