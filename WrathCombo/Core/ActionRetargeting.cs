#region

using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global

#endregion

namespace WrathCombo.Core;

public class ActionRetargeting
{
    /// <summary>
    ///     List of active Actions that have a desired target, and the delegate to
    ///     resolve that target.
    /// </summary>
    /// <remarks>
    ///     <b>
    ///         <c>key</c>
    ///     </b>
    ///     — The ActionID that will be re-targeted.
    ///     <br /><br />
    ///     <b>
    ///         <c>val</c>
    ///     </b>
    ///     — A <see cref="TargetResolverDelegate" /> that will
    ///     resolve the target for the ActionID.
    /// </remarks>
    /// <example>
    ///     For
    ///     <b>
    ///         <c>val</c>
    ///     </b>
    ///     you can use:
    ///     <br />
    ///     A <see cref="SimpleTargets">SimpleTarget</see>, like:
    ///     <see cref="SimpleTargets.HardTarget">SimpleTargets.HardTarget</see>
    ///     <br />or<br />
    ///     A <see cref="TargetResolverDelegate">custom delegate</see>, like:
    ///     <see cref="Combos.PvE.AST.DPSCardsTargetResolver">AST.DPSCardsTargetResolver</see>
    /// </example>
    /// <seealso cref="TargetResolverDelegate" />
    private static Dictionary<uint, TargetResolverDelegate> _targetResolvers = new();

    /// <summary>
    ///     Register an action as one you want re-targeted.
    /// </summary>
    /// <param name="actionID">The Action to retarget.</param>
    /// <param name="targetResolver">
    ///     The <see cref="TargetResolverDelegate" /> to resolve the target.
    /// </param>
    /// <returns>
    ///     The <paramref name="actionID" /> that was registered.<br />
    ///     This only really returns to make
    ///     <see cref="Extensions.UIntExtensions.Retarget">(uint).Retarget()</see>
    ///     simpler.
    /// </returns>
    public static uint Register(uint actionID, TargetResolverDelegate targetResolver)
    {
        return actionID;
    }

    /// <summary>
    ///     Unregister an action from being re-targeted.
    /// </summary>
    /// <param name="actionID">
    ///     The Action to remove from the
    ///     <see cref="_targetResolvers">list of Re-Targeted Actions</see>.
    /// </param>
    public static void Unregister(uint actionID)
    {
    }

    /// <summary>
    ///     Check if an action should be re-targeted or not.
    /// </summary>
    /// <param name="actionID">
    ///     The Action to check against
    ///     <see cref="_targetResolvers">list of Re-Targeted Actions</see>.
    /// </param>
    /// <returns>If the action should be re-targeted</returns>
    public static bool CheckFor(uint actionID)
    {
        return false;
    }

    /// <summary>
    ///     Resolve the target for an action via the provided
    ///     <see cref="TargetResolverDelegate" />.
    /// </summary>
    /// <param name="actionID">The Action to re-target.</param>
    /// <returns>
    ///     The <see cref="IGameObject">Game Object</see> of the target
    /// </returns>
    public static bool TryGetTargetFor(uint actionID, out IGameObject? target)
    {
        // Find the target resolver
        target = null;
        if (!_targetResolvers.TryGetValue(actionID, out var targetResolver))
            return false;
        PluginLog.Debug("[ActionRetargeting] re-targeting " +
                        $"'{actionID.ActionName()}' " +
                        $"with {targetResolver.GetMethodName()}");

        // Run the target resolver
        Unregister(actionID);
        try
        {
            target = targetResolver.Invoke();
        }
        catch (Exception ex)
        {
            PluginLog.Error("[ActionRetargeting] error while resolving target for " +
                            $"{actionID.ActionName()} " +
                            $"with {targetResolver.GetMethodName()}:\n{ex}");
            return false;
        }

        // Return the results
        PluginLog.Verbose("[ActionRetargeting] re-targeted" +
                          $"{actionID.ActionName()} to {target?.Name ?? "null"} " +
                          $"(with {targetResolver.GetMethodName()})");
        return target != null;
    }

    #region Utilities

    /// Clears <see cref="_targetResolvers">cached re-targets</see>.
    internal static void ClearCachedRetargets()
    {
        _targetResolvers.Clear();
        PluginLog.Debug("[ActionRetargeting] cleared cached re-targets");
    }

    /// Formats the method name of target resolvers for logging.
    private static string GetMethodName(this TargetResolverDelegate targetResolver)
    {
        var resolverName = targetResolver.Method.Name;
        var resolverClass = targetResolver.Method.DeclaringType?.Name ?? "";

        resolverName = resolverClass switch
        {
            "SimpleTargets" => "SimpleTargets." + resolverName,
            "Stacks" => "SimpleTargets.Stacks." + resolverName,
            _ => resolverName,
        };

        return $"`{resolverName}`";
    }

    #endregion
}

/// <summary>
///     Delegate function to resolve a target for an action.
/// </summary>
/// <returns>
///     The <see cref="IGameObject">Game Object</see> of the target.
/// </returns>
/// <example>
///     For inline delegate-error checking purposes it is suggested you write your
///     resolvers as static field lambdas in your Job's class or JobHelper class.
///     <code>
///     public static TargetResolverDelegate ExampleTargetResolver = () => {
///         if (Config.HelpfulGroundTargetAbility_TargetOther)
///             return SimpleTargets.FocusTarget;
///     };
///     </code>
/// </example>
/// <seealso cref="Combos.PvE.AST.DPSCardsTargetResolver">
///     AST.DPSCardsTargetResolver
/// </seealso>
public delegate IGameObject? TargetResolverDelegate();

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
