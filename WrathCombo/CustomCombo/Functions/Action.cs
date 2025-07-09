using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Linq;
using WrathCombo.Core;
using WrathCombo.Data;
using WrathCombo.Services;

namespace WrathCombo.CustomComboNS.Functions
{
    internal abstract partial class CustomComboFunctions
    {
        public const float BaseActionQueue = 0.5f;
        public const float BaseAnimationLock = 0.6f;

        /// <summary> Gets the original hook for an action. </summary>
        /// <param name="actionId"> The action ID. </param>
        public static uint OriginalHook(uint actionId) => Service.ActionReplacer.OriginalHook(actionId);

        /// <summary> Checks if an action matches its original hook. </summary>
        /// <param name="actionId"> The action ID. </param>
        public static bool IsOriginal(uint actionId) => Service.ActionReplacer.OriginalHook(actionId) == actionId;

        /// <summary> Checks if the player has learned an action and is high enough level to use it. </summary>
        /// <param name="actionId"> The action ID. </param>
        public static bool LevelChecked(uint actionId) => LocalPlayer.Level >= GetActionLevel(actionId) && IsActionUnlocked(actionId);

        /// <summary> Checks if the player is high enough level to benefit from a trait. </summary>
        /// <param name="traitId"> The trait ID. </param>
        public static bool TraitLevelChecked(uint traitId) => LocalPlayer.Level >= GetTraitLevel(traitId);

        /// <summary> Gets the name of an action as a string. </summary>
        /// <param name="actionId"> The action ID. </param>
        public static string GetActionName(uint actionId) => ActionWatching.GetActionName(actionId);

        /// <summary> Gets the minimum level required to use an action. </summary>
        /// <param name="actionId"> The action ID. </param>
        public static int GetActionLevel(uint actionId) => ActionWatching.GetActionLevel(actionId);

        /// <summary> Gets the minimum level required to benefit from a trait. </summary>
        /// <param name="traitId"> The trait ID. </param>
        public static int GetTraitLevel(uint traitId) => ActionWatching.GetTraitLevel(traitId);

        /// <summary> Gets the cast time of an action. </summary>
        /// <param name="actionId"> The action ID. </param>
        internal static float GetActionCastTime(uint actionId) => ActionWatching.GetActionCastTime(actionId);

        /// <summary>
        ///     Checks if the player is within range to use an action. <br/>
        ///     If the action requires a target, defaults to CurrentTarget unless specified.
        /// </summary>
        public static bool InActionRange(uint actionId, IGameObject? optionalTarget = null)
        {
            var target = optionalTarget ?? CurrentTarget;
            var actionRange = ActionWatching.GetActionRange(actionId);

            // Targeted Actions
            if (actionRange > 0)
                return target != null && GetTargetDistance(target) <= actionRange;

            var actionRadius = ActionWatching.GetActionEffectRange(actionId);

            // Self AoE Actions
            if (actionRadius > 0)
                return target != null && GetTargetDistance(target) <= actionRadius;

            // Self Actions
            return true;
        }

        /// <summary> Checks if the player can use an action based on the level required and whether it has charges / is off cooldown. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns> Non-charge actions have a charge value of 1 when off cooldown; otherwise they have a value of 0. </returns>
        public static unsafe bool ActionReady(uint id)
        {
            uint hookedId = OriginalHook(id);

            return ((GetCooldownRemainingTime(hookedId) <= RemainingGCD + 0.5f && ActionWatching.GetAttackType(hookedId) != ActionWatching.ActionAttackType.Ability) ||
                HasCharges(hookedId)) && ActionManager.Instance()->GetActionStatus(ActionType.Action, hookedId, checkRecastActive: false, checkCastingActive: false) is 0 or 582 or 580;
        }

        /// <summary> Checks if all passed actions are ready to be used. </summary>
        /// <param name="ids"> IDs of the actions. </param>
        /// <returns></returns>
        public static bool ActionsReady(uint[] ids)
        {
            foreach (var id in ids)
                if (!ActionReady(id)) return false;

            return true;
        }

        /// <summary> Checks if the last action performed was the passed ID. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns></returns>
        public static bool WasLastAction(uint id) => ActionWatching.CombatActions.Count > 0 && ActionWatching.CombatActions.LastOrDefault() == id;

        /// <summary> Returns how many times in a row the last action was used. </summary>
        /// <returns></returns>
        public static int LastActionCounter() => ActionWatching.LastActionUseCount;

        /// <summary> Checks if the last weaponskill used was the passed ID. Does not have to be the last action performed, just the last weaponskill used. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns></returns>
        public static bool WasLastWeaponskill(uint id) => ActionWatching.LastWeaponskill == id;

        /// <summary> Checks if the last spell used was the passed ID. Does not have to be the last action performed, just the last spell used. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns></returns>
        public static bool WasLastSpell(uint id) => ActionWatching.LastSpell == id;

        /// <summary> Checks if the last ability used was the passed ID. Does not have to be the last action performed, just the last ability used. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns></returns>
        public static bool WasLastAbility(uint id) => ActionWatching.LastAbility == id;

        /// <summary> Returns if the player has set the spell as active in the Blue Mage Spellbook </summary>
        /// <param name="id"> ID of the BLU spell. </param>
        /// <returns></returns>
        public static bool IsSpellActive(uint id) => Service.Configuration.ActiveBLUSpells.Contains(id);

        /// <summary> Calculate the best action to use, based on cooldown remaining. If there is a tie, the original is used. </summary>
        /// <param name="original"> The original action. </param>
        /// <param name="actions"> Action data. </param>
        /// <returns> The appropriate action to use. </returns>
        public static uint CalcBestAction(uint original, params uint[] actions)
        {
            static (uint ActionID, CooldownData Data) Compare(
                uint original,
                (uint ActionID, CooldownData Data) a1,
                (uint ActionID, CooldownData Data) a2)
            {
                // Neither, return the first parameter
                if (!a1.Data.IsCooldown && !a2.Data.IsCooldown)
                    return original == a1.ActionID ? a1 : a2;

                // Both, return soonest available
                if (a1.Data.IsCooldown && a2.Data.IsCooldown)
                {
                    if (a1.Data.HasCharges && a2.Data.HasCharges)
                    {
                        if (a1.Data.RemainingCharges == a2.Data.RemainingCharges)
                        {
                            return a1.Data.ChargeCooldownRemaining < a2.Data.ChargeCooldownRemaining
                                ? a1 : a2;
                        }

                        return a1.Data.RemainingCharges > a2.Data.RemainingCharges
                            ? a1 : a2;
                    }

                    else if (a1.Data.HasCharges)
                    {
                        if (a1.Data.RemainingCharges > 0)
                            return a1;

                        return a1.Data.ChargeCooldownRemaining < a2.Data.CooldownRemaining
                            ? a1 : a2;
                    }

                    else if (a2.Data.HasCharges)
                    {
                        if (a2.Data.RemainingCharges > 0)
                            return a2;

                        return a2.Data.ChargeCooldownRemaining < a1.Data.CooldownRemaining
                            ? a2 : a1;
                    }

                    else
                    {
                        return a1.Data.CooldownRemaining < a2.Data.CooldownRemaining
                            ? a1 : a2;
                    }
                }

                // One or the other
                return a1.Data.IsCooldown ? a2 : a1;
            }

            static (uint ActionID, CooldownData Data) Selector(uint actionID) => (actionID, GetCooldown(actionID));

            return actions
                .Select(Selector)
                .Aggregate((a1, a2) => Compare(original, a1, a2))
                .ActionID;
        }

        /// <summary> Checks if a certain amount of actions were weaved within the GCD window. </summary>
        public static bool HasWeaved(int weaveAmount = 1) => ActionWatching.WeaveActions.Count >= weaveAmount;

        /// <summary> Checks if a specific action was weaved within the GCD window. </summary>
        public static bool HasWeavedAction(uint actionId) => ActionWatching.WeaveActions.Contains(actionId);

        /// <summary>
        ///     Checks if an action can be woven within this GCD window.
        /// </summary>
        /// <param name="estimatedWeaveTime">
        ///     Amount of time required before the GCD is off cooldown.<br/>
        ///     An Estimate of how long this oGCD will take.
        /// </param>
        /// <param name="maximumWeaves">
        ///     Maximum amount of weaves allowed in this GCD window.<br/>
        ///     Defaults to <see cref="PluginConfiguration.MaximumWeavesPerWindow"/>.
        /// </param>
        public static unsafe bool CanWeave(float estimatedWeaveTime = BaseAnimationLock, int? maximumWeaves = null)
        {
            var player = LocalPlayer;
            var allowableWeaves = maximumWeaves ?? Service.Configuration.MaximumWeavesPerWindow;
            var remainingCast = player.TotalCastTime - player.CurrentCastTime;
            var animationLock = ActionManager.Instance()->AnimationLock;

            return animationLock <= BaseActionQueue &&                                     // Animation Threshold
                   remainingCast <= BaseActionQueue &&                                     // Casting Threshold
                   RemainingGCD > (remainingCast + estimatedWeaveTime + animationLock) &&  // Window End Threshold
                   ActionWatching.WeaveActions.Count < allowableWeaves;                    // Multi-weave Check
        }

        /// <summary> Checks if an action can be weaved within the GCD window when casting spells or weaponskills. </summary>
        /// <param name="weaveEnd"> Remaining GCD time when the window ends. </param>
        [Obsolete("Use CanWeave instead. This method will be removed in a future update.")]
        public static bool CanSpellWeave(float weaveEnd = BaseAnimationLock) => CanWeave(weaveEnd);

        /// <summary> Checks if an action can be weaved within the GCD window, limited by specific GCD thresholds. </summary>
        /// <param name="weaveStart">
        ///     Remaining GCD time when the window starts. <br/>
        ///     Cannot be set higher than half the GCD.
        /// </param>
        /// <param name="weaveEnd">
        ///     Remaining GCD time when the window ends. <br/>
        ///     Defaults to 0.6s unless specified.
        /// </param>
        /// <param name="maxWeaves">
        ///     Maximum amount of weaves allowed per window.<br/>
        ///     Defaults to <see cref="PluginConfiguration.MaximumWeavesPerWindow"/>.
        /// </param>
        public static unsafe bool CanDelayedWeave(float weaveStart = 1.25f, float weaveEnd = BaseAnimationLock, int? maxWeaves = null)
        {
            var halfGCD = GCDTotal * 0.5f;
            var remainingGCD = RemainingGCD;
            var weaveLimit = maxWeaves ?? Service.Configuration.MaximumWeavesPerWindow;
            var animationLock = ActionManager.Instance()->AnimationLock;

            return animationLock <= BaseActionQueue &&                             // Animation Threshold
                remainingGCD > (weaveEnd + animationLock) &&                       // Window End Threshold
                remainingGCD <= (weaveStart > halfGCD ? halfGCD : weaveStart) &&   // Window Start Threshold
                ActionWatching.WeaveActions.Count < weaveLimit;                    // Multi-weave Check
        }

        public enum WeaveTypes
        {
            None,
            Weave,
            DelayWeave,
            SpellWeave
        }
        public static bool CheckWeave(WeaveTypes weave) => weave switch
        {
            WeaveTypes.None => true,
            WeaveTypes.Weave => CanWeave(),
            WeaveTypes.DelayWeave => CanDelayedWeave(),
            WeaveTypes.SpellWeave => CanSpellWeave(),
            _ => false
        };

        /// <summary> Gets the current combo timer. </summary>
        public static unsafe float ComboTimer => ActionManager.Instance()->Combo.Timer;

        /// <summary> Gets the last combo action. </summary>
        public static unsafe uint ComboAction => ActionManager.Instance()->Combo.Action;

        /// <summary> Gets the current limit break action (PvE only). </summary>
        public static unsafe uint LimitBreakAction => LimitBreakController.Instance()->GetActionId(Player.Object.Character(), (byte)Math.Max(0, (LimitBreakLevel - 1)));

        public static unsafe bool CanQueue(uint actionID)
        {
            bool alreadyQueued = ActionManager.Instance()->QueuedActionId != 0;
            bool inSlidecast = (LocalPlayer.TotalCastTime - LocalPlayer.CurrentCastTime) <= 0.5f;
            bool animLocked = ActionManager.Instance()->AnimationLock > 0;

            bool ret = !alreadyQueued && inSlidecast && !animLocked && ActionReady(actionID);
            return ret;
        }

        private static bool _raidwideInc;
        public static unsafe bool RaidWideCasting(float timeRemaining = 0f)
        {
            if (!EzThrottler.Throttle("RaidWideCheck", 100))
                return _raidwideInc;

            foreach (var obj in Svc.Objects)
            {
                if (obj is not IBattleChara caster || !caster.IsHostile() || !caster.IsCasting)
                    continue;

                if (ActionWatching.ActionSheet.TryGetValue(caster.CastActionId, out var spellSheet))
                {
                    byte type = spellSheet.CastType;
                    byte range = spellSheet.EffectRange;

                    if (type is 2 or 5 && range >= 30)
                    {
                        if (timeRemaining == 0f)
                            return _raidwideInc = true;
                       
                        if ((caster.TotalCastTime - caster.CurrentCastTime) <= timeRemaining)
                            return _raidwideInc = true;
                    }
                }
            }

            return _raidwideInc = false;
        }

        private static bool _beingTargetedHostile;
        public static bool BeingTargetedHostile
        {
            get
            {
                if (!EzThrottler.Throttle("BeingTargetedHostile", 100))
                    return _beingTargetedHostile;

                return _beingTargetedHostile = Svc.Objects.Any(x => x is IBattleChara chara && chara.IsHostile() && chara.CastTargetObjectId == LocalPlayer.GameObjectId);
            }
        }

        /// <summary>
        /// Counts how many times an action has been used since combat started.
        /// </summary>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public static int ActionCount(uint actionId) => ActionWatching.CombatActions.Count(x => x == OriginalHook(actionId));

        /// <summary>
        /// Counts how many times multiple actions have been used since combat started.
        /// </summary>
        /// <param name="actionIds"></param>
        /// <returns></returns>
        public static int ActionCount(uint[] actionIds)
        {
            int output = 0;
            foreach (var a in actionIds)
                output += ActionCount(a);

            return output;
        }

        /// <summary>
        /// Counts how many times an action has been used in combat since using another action
        /// </summary>
        /// <param name="actionToCheckAgainst"></param>
        /// <param name="actionToCount"></param>
        /// <returns></returns>
        public static int TimesUsedSinceOtherAction(uint actionToCheckAgainst, uint actionToCount)
        {
            if (!ActionWatching.CombatActions.Any(x => x == actionToCheckAgainst)) return 0;

            int startIdx = ActionWatching.CombatActions.LastIndexOf(actionToCheckAgainst);

            int output = 0;
            for (int i = startIdx; i < ActionWatching.CombatActions.Count; i++)
            {
                if (ActionWatching.CombatActions[i] == actionToCount)
                    output++;
            }

            return output;
        }

        /// <summary>
        /// Counts how many times multiple actions have been used in combat since using another action
        /// </summary>
        /// <param name="actionToCheckAgainst"></param>
        /// <param name="actionsToCount"></param>
        /// <returns></returns>
        public static int TimesUsedSinceOtherAction(uint actionToCheckAgainst, uint[] actionsToCount)
        {
            int output = 0;
            foreach(uint a in actionsToCount)
            {
                output += TimesUsedSinceOtherAction(actionToCheckAgainst, a);
            }

            return output;
        }
    }
}
