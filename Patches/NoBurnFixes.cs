using System;
using System.Reflection;
using HarmonyLib;
using ScenarioRuleLibrary;
using GLOOM.MainMenu;
using UnityEngine;

namespace GloomhavenFixes.Patches
{
    public static class NoBurnFixes
    {
        // ============================================================
        // Utility: Safely access and modify GameState private fields
        // ============================================================
        private static void ClearCurrentActionSelectionFlag()
        {
            // Use AccessTools.Field to get field info
            FieldInfo fieldInfo = AccessTools.Field(typeof(GameState), "s_CurrentActionSelectionFlag");
            if (fieldInfo != null)
            {
                // Since it is a static field, pass null for the object
                fieldInfo.SetValue(null, GameState.EActionSelectionFlag.None);
            }
        }
           
        // --------------------------------------------------------
        // Patch: Long Rest
        // --------------------------------------------------------
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameState), nameof(GameState.PlayerLongRested))]
        public static bool PlayerLongRested_Prefix(CAbilityCard abilityCard, CPlayerActor playerActor, bool improvedShortRestUsed, bool fromStateUpdate)
        {
            // If argument is null, try to get current actor
            if (playerActor == null)
            {
                playerActor = (CPlayerActor)GameState.InternalCurrentActor;
            }
            if (playerActor == null) return true; // Safety fallback: if no actor, run original logic

            CCharacterClass characterClass = playerActor.CharacterClass;

            // -- Original Logic Part 1: Set Flag --
            if (!improvedShortRestUsed)
            {
                // [Modification] Clear Flag. Since it's private, use reflection.
                ClearCurrentActionSelectionFlag();
                characterClass.HasLongRested = true;
            }
            else
            {
                characterClass.HasShortRested = true;
                characterClass.HasImprovedShortRested = true;
            }

            int health = playerActor.Health;

            // -- Core Modification: Do not move abilityCard to Lost --
            // Instead, move all discarded cards (including the one selected for burning) back to Hand
            if (!fromStateUpdate)
            {
                while (characterClass.DiscardedAbilityCards.Count > 0)
                {
                    // Get the first card in discard pile
                    var cardToReturn = characterClass.DiscardedAbilityCards[0];
                    
                    // Logic: Discard -> Hand
                    playerActor.CharacterClass.MoveAbilityCard(
                        cardToReturn, 
                        characterClass.DiscardedAbilityCards, 
                        characterClass.HandAbilityCards, 
                        "DiscardedAbilityCards", 
                        "HandAbilityCards"
                    );
                }
            }

            // -- Original Logic Part 2: Heal and Refresh Items (Kept intact) --
            playerActor.Healed(2, false, true, true); // Heal 2
            playerActor.Inventory.RefreshItems(CItem.EItemSlotState.Spent);
            playerActor.Inventory.RefreshItems(CItem.EItemSlotState.Locked, CItem.EItemSlot.None, CItem.EUsageType.Spent);

            // -- Original Logic Part 3: Reset UI Highlights --
            bool improvedShortRest = playerActor.CharacterClass.ImprovedShortRest;
            if (!improvedShortRest)
            {
                playerActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.DuringOwnTurn);
            }

            // Reset flags
            playerActor.CharacterClass.LongRest = false;
            playerActor.CharacterClass.ImprovedShortRest = false;

            // -- Original Logic Part 4: Network Messages and Logging --
            if (improvedShortRest)
            {
                // The abilityCard is only used for logging here, not actually burned
                playerActor.CharacterClass.ShortRestCardBurned = abilityCard; 
                playerActor.Inventory.HighlightUsableItems(null, default(CItem.EItemTrigger));
                
                ScenarioRuleClient.MessageHandler(new CPlayerImprovedShortRested_MessageData(health, abilityCard, playerActor));
                
                SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(
                    CAbility.EAbilityType.ShortRest, ESESubTypeAbility.AbilityEnded, null, 
                    int.MaxValue, CBaseCard.ECardType.None, playerActor.Class.ID, 0, 
                    null, null, playerActor?.Type, false, playerActor?.Tokens.CheckPositiveTokens, 
                    playerActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, false, null, null
                ));
            }
            else
            {
                playerActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.AtEndOfTurn, CItem.EItemTrigger.DuringOwnTurn);
                ScenarioRuleClient.MessageHandler(new CPlayerLongRested_MessageData(health, abilityCard, playerActor));
                
                SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(
                    CAbility.EAbilityType.LongRest, ESESubTypeAbility.AbilityEnded, null, 
                    int.MaxValue, CBaseCard.ECardType.None, playerActor.Class.ID, 0, 
                    null, null, playerActor?.Type, false, playerActor?.Tokens.CheckPositiveTokens, 
                    playerActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, false, null, null
                ));
            }

            // -- Original Logic Part 5: Trigger Passive Bonuses --
            if (GameState.PendingOnLongRestBonuses != null)
            {
                GameState.PendingOnLongRestBonuses.AddRange(
                    playerActor.FindApplicableActiveBonuses(
                        CAbility.EAbilityType.AddActiveBonus, 
                        CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnLongRest
                    )
                );
            }

            // Return false to intercept, preventing original burn logic
            return false; 
        }

        // --------------------------------------------------------
        // Patch: Short Rest
        // --------------------------------------------------------
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameState), nameof(GameState.PlayerShortRested))]
        public static bool PlayerShortRested_Prefix(CPlayerActor playerActor, CAbilityCard discardedAbilityCard, bool loseHealth, bool updateScenarioRNG, bool fromStateUpdate)
        {
            CCharacterClass characterClass = playerActor.CharacterClass;
            
            // Guard against recursive re-entry
            if (characterClass.HasShortRested && fromStateUpdate)
            {
                Debug.Log("[NoBurnMod] Short Rest triggered but already done. Skipping.");
                return false;
            }

            characterClass.HasShortRested = true;
            characterClass.ShortRestCardBurned = discardedAbilityCard;
            characterClass.ShortRestCardRedrawn = loseHealth;

            // -- Core Modification: Short Rest does not burn cards, move all to Hand --
            if (!fromStateUpdate)
            {
                // Move all discarded cards back to hand (including the one supposed to be randomly burned)
                while (characterClass.DiscardedAbilityCards.Count > 0)
                {
                    var card = characterClass.DiscardedAbilityCards[0];
                    playerActor.CharacterClass.MoveAbilityCard(
                        card,
                        characterClass.DiscardedAbilityCards,
                        characterClass.HandAbilityCards,
                        "DiscardedAbilityCards",
                        "HandAbilityCards"
                    );
                }
            }

            // Maintain RNG state synchronization (Must execute for multiplayer stability)
            if (updateScenarioRNG)
            {
                ScenarioManager.CurrentScenarioState.ScenarioRNG.Next();
                if (loseHealth)
                    ScenarioManager.CurrentScenarioState.ScenarioRNG.Next();
            }

            // Health penalty (Optional logic, kept for fairness even if no card is burned)
            if (loseHealth)
            {
                int health = playerActor.Health;
                
                // Use named arguments to ensure correct call
                playerActor.Damaged(1, false, null, null); 
                
                // Trigger events (Safe Invoke)
                playerActor.m_OnDamagedListeners?.Invoke(playerActor);
                playerActor.m_OnTakenDamageListeners?.Invoke(1, null, 0, 1);

                // Simplified Damage Log
            }

            // Send Short Rest message
            ScenarioRuleClient.MessageHandler(new CPlayerShortRested_MessageData(playerActor, discardedAbilityCard, playerActor));

            // Log (Short Rest Ended)
            SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(
                CAbility.EAbilityType.ShortRest, ESESubTypeAbility.AbilityEnded, null, 
                int.MaxValue, CBaseCard.ECardType.None, playerActor.Class.ID, 0, 
                null, null, playerActor?.Type, false, playerActor?.Tokens.CheckPositiveTokens, 
                playerActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, false, null, null
            ));

            // Exhaust Check
            // Skipped KillActor call to support "Infinite Hand" playstyle
            if (!fromStateUpdate)
            {
                 // Replicate state message
                 var cardState = playerActor.CharacterClass.GetCurrentCardState();
                 if (cardState != null)
                 {
                    ScenarioRuleClient.MessageHandler(new CReplicateStartRoundCardState_MessageData(playerActor, 51, cardState));
                 }
            }

            // Intercept original method
            return false;
        }
    }
}