#region

using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#endregion

namespace WrathCombo.CustomComboNS;

internal static class SimpleTargets
{
    #region Common Target Stacks

    internal static class Stacks
    {
        public static IGameObject? OverrideAllyOrSelf() =>
            ModelMouseOverTarget() ?? MouseOverTarget() ??
            FocusTarget() ?? SoftTarget() ?? HardTarget() ?? Self();

        public static IGameObject? OverrideOrSelf() =>
            ModelMouseOverTarget() ?? MouseOverTarget() ?? HardTarget() ?? Self();
    }

    #endregion

    #region Core Targets

    public static IGameObject? Self() =>
        Player.Available ? Player.Object : null;

    public static IGameObject? HardTarget() =>
        Svc.Targets.Target;

    public static IGameObject? SoftTarget() =>
        Svc.Targets.SoftTarget;

    public static IGameObject? FocusTarget() =>
        Svc.Targets.FocusTarget;

    public static IGameObject? MouseOverTarget() =>
        Svc.Targets.MouseOverTarget;

    public static IGameObject? ModelMouseOverTarget() =>
        Svc.Targets.MouseOverNameplateTarget;

    #endregion

    #region Role Targets

    public static IGameObject? Tank() => null;
    public static IGameObject? Healer() => null;
    public static IGameObject? DPS() => null;

    #endregion

    // etc, etc, a la Reaction's Custom PlaceHolders
    // https://github.com/UnknownX7/ReAction/blob/master/PronounManager.cs
}
