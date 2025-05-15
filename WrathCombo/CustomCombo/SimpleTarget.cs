#region

using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
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
        /// A stack of common "override" targets, that users regularly override
        /// automatic target selection with.
        /// <remarks>
        ///     (should also include <see cref="SimpleTarget.HardTarget" />, but
        ///     that override shouldn't be at the top of the stack with the other
        ///     overrides)
        /// </remarks>
        public static IGameObject? Overrides =>
            UIMouseOverTarget ?? ModelMouseOverTarget;

        /// A very common stack that targets an ally or self, if there are no manual
        /// overrides targeted.
        /// <!-- todo: maybe including focus target should be an option? -->
        public static IGameObject? OverridesAllies =>
            Overrides ?? FocusTarget ?? SoftTarget ?? HardTarget ?? Self;

        /// A very common stack that targets the player, if there are no manual
        /// overrides targeted.
        public static IGameObject? OverridesSelf =>
            Overrides ?? HardTarget ?? Self;
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

    public static IGameObject? UIMouseOverTarget =>
        PartyUITargeting.UiMouseOverTarget;

    public static IGameObject? ModelMouseOverTarget =>
        Svc.Targets.MouseOverNameplateTarget ?? Svc.Targets.MouseOverTarget;

    public static IGameObject? Chocobo =>
        Svc.Buddies.CompanionBuddy?.GameObject;

    #endregion

    #region Role Targets

    public static IGameObject? Tank() => null;
    public static IGameObject? Healer() => null;
    public static IGameObject? DPS() => null;

    #endregion

    // etc, etc, a la Reaction's Custom PlaceHolders
    // https://github.com/UnknownX7/ReAction/blob/master/PronounManager.cs
}
