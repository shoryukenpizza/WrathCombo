#region

using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using WrathCombo.Extensions;
using WrathCombo.Services;

// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#endregion

namespace WrathCombo.CustomComboNS;

internal static class SimpleTarget
{
    #region Common Target Stacks

    /// <summary>
    ///     A collection of common targeting "stacks", used when you want a number of
    ///     different target options, with fallback values.<br />
    ///     (and overriding values)
    /// </summary>
    internal static class Stack
    {
        /// A stack of Mouse Over targets, primarily used for Overrides.
        public static IGameObject? MouseOver =>
            UIMouseOverTarget ?? ModelMouseOverTarget;

        /// A very common stack that targets an ally or self, if there are no manual
        /// overrides targeted.
        /// <remarks>
        ///     "Overrides" include MouseOver at the top of the stack and the
        ///     Hard Target near the bottom.
        /// </remarks>
        /// <!-- todo: maybe including focus target should be an option? -->
        public static IGameObject? OverridesAllies =>
            MouseOver ?? FocusTarget ?? SoftTarget ?? HardTarget ?? Self;

        /// A very common stack that targets the player, if there are no manual
        /// overrides targeted.
        /// <remarks>
        ///     "Overrides" include MouseOver at the top of the stack and the
        ///     Hard Target near the bottom.
        /// </remarks>
        public static IGameObject? OverridesSelf =>
            MouseOver ?? HardTarget ?? Self;

        /// Temporary until the UI MouseOver checking is moved to be a Setting
        public static bool AllyToHealUseMouseOver { get; set; } = false;

        /// <summary>
        ///     A very common stack to pick a heal target.
        /// </summary>
        public static IGameObject? AllyToHeal =>
            (AllyToHealUseMouseOver ? MouseOver : null) ??
            SoftTarget.IfFriendly() ?? HardTarget.IfFriendly() ?? Self;
    }

    #endregion

    #region Core Targets

    public static IGameObject? Self =>
        Player.Available ? Player.Object : null;

    public static IGameObject? HardTarget =>
        Svc.Targets.Target;

    public static IGameObject? SoftTarget =>
        Svc.Targets.SoftTarget;

    public static IGameObject? FocusTarget =>
        Svc.Targets.FocusTarget;

    public static IGameObject? TargetsTarget =>
        Svc.Targets.Target is { TargetObjectId: not 0xE0000000 }
            ? Svc.Targets.Target.TargetObject
            : null;

    public static IGameObject? UIMouseOverTarget
    {
        get
        {
            if (!PronounService.PronounsReady) return null;
            unsafe
            {
                return GameObjectExtensions.GetObjectFrom(
                    PronounService.Module->UiMouseOverTarget);
            }
        }
    }

    public static IGameObject? ModelMouseOverTarget =>
        Svc.Targets.MouseOverNameplateTarget ?? Svc.Targets.MouseOverTarget;

    public static IGameObject? Chocobo =>
        Svc.Buddies.CompanionBuddy?.GameObject;

    #region Previous Targets

    public static IGameObject? LastHardTarget =>
        PronounService.GetIGameObjectFromPronounID(1006);

    public static IGameObject? LastHostileHardTarget =>
        PronounService.GetIGameObjectFromPronounID(1084);

    public static IGameObject? MostRecentAttacker =>
        PronounService.GetIGameObjectFromPronounID(1008);

    #endregion

    #region Party Slots

    public static IGameObject? PartyMember1 => GetPartyMemberInSlotSlot(1);
    public static IGameObject? PartyMember2 => GetPartyMemberInSlotSlot(2);
    public static IGameObject? PartyMember3 => GetPartyMemberInSlotSlot(3);
    public static IGameObject? PartyMember4 => GetPartyMemberInSlotSlot(4);
    public static IGameObject? PartyMember5 => GetPartyMemberInSlotSlot(5);
    public static IGameObject? PartyMember6 => GetPartyMemberInSlotSlot(6);
    public static IGameObject? PartyMember7 => GetPartyMemberInSlotSlot(7);
    public static IGameObject? PartyMember8 => GetPartyMemberInSlotSlot(8);

    /// <summary>
    ///     Tries to get a party member, by slot number (1–8).
    /// </summary>
    /// <param name="slot">
    ///     The party slot (1 for local player, 2–8 for party members).
    /// </param>
    /// <returns>
    ///     An <see cref="IGameObject" /> for the party member if found;
    /// </returns>
    /// <remarks>IDs start at 44 and go to 51</remarks>
    public static IGameObject? GetPartyMemberInSlotSlot(int slot) =>
        slot switch
        {
            < 1 or > 8 => null,
            1 => Self,
            _ => PronounService.GetIGameObjectFromPronounID(42 + slot),
        };

    #endregion

    #endregion

    #region Role Targets

    public static IGameObject? Tank() => null;
    public static IGameObject? Healer() => null;
    public static IGameObject? DPS() => null;

    #endregion

    // etc, etc, a la Reaction's Custom PlaceHolders
    // https://github.com/UnknownX7/ReAction/blob/master/PronounManager.cs
}
