#region

using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Logging;
using WrathCombo.CustomComboNS;
using WrathCombo.Extensions;
using EZ = ECommons.Throttlers.EzThrottler;
using TS = System.TimeSpan;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global

#endregion

namespace WrathCombo.Core;

public static class ActionRetargeting
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
    /// <seealso cref="Register" />
    private static readonly Dictionary<uint, TargetResolverDelegate>
        _targetResolvers = [];

    /// <summary>
    ///     Register an action as one you want re-targeted.
    /// </summary>
    /// <param name="actionID">The Action to retarget.</param>
    /// <param name="resolver">
    ///     The <see cref="TargetResolverDelegate" /> to resolve the target.<br/>
    ///     Examples:
    ///     a <see cref="SimpleTargets">SimpleTarget</see> (like
    ///     <see cref="SimpleTargets.HardTarget">HardTarget</see>)
    ///     or
    ///     a <see cref="TargetResolverDelegate">custom delegate</see> (like:
    ///     <see cref="Combos.PvE.AST.DPSCardsTargetResolver">AST.CardsResolver</see>)
    /// </param>
    /// <returns>
    ///     The <paramref name="actionID" /> that was registered.<br />
    ///     This only really returns to make
    ///     <see cref="Extensions.UIntExtensions.Retarget">(uint).Retarget()</see>
    ///     simpler.
    /// </returns>
    /// <remarks>
    ///     Should only be called by
    ///     <see cref="Extensions.UIntExtensions.Retarget">(uint).Retarget()</see>.
    /// </remarks>
    internal static uint Register(uint actionID, TargetResolverDelegate resolver)
    {
        // Limit spam from the same actionID
        if (!EZ.Throttle($"retargetingFor{actionID}", TS.FromSeconds(1)))
            return actionID;

        // Cleaning up the old target resolver
        if (_targetResolvers.TryGetValue(actionID, out var oldResolver))
        {
            // Keep the old resolver if it's <30 seconds old
            if (oldResolver.Method.Name == resolver.Method.Name &&
                !EZ.Throttle($"retargetingOver{actionID}", TS.FromSeconds(30)))
                return actionID;
            // Unregister the old resolver (just when different)
            Unregister(actionID);
            PluginLog.Verbose(
                $"[ActionRetargeting] overwriting retargeting for" +
                $"'{actionID.ActionName()}'");
        }

        // Save the resolver
        PluginLog.Verbose("[ActionRetargeting] registering" +
                          $"'{actionID.ActionName()}' for retargeting " +
                          $"with {resolver.GetMethodName()}");
        _targetResolvers.Add(actionID, resolver);
        return actionID;
    }

    /// <summary>
    ///     Unregister an action from being re-targeted.
    /// </summary>
    /// <param name="actionID">
    ///     The Action to remove from the
    ///     <see cref="_targetResolvers">list of Re-Targeted Actions</see>.
    /// </param>
    private static void Unregister(uint actionID) =>
        _targetResolvers.Remove(actionID);

    /// <summary>
    ///     Resolve the target for an action via the provided
    ///     <see cref="TargetResolverDelegate" />.
    /// </summary>
    /// <param name="actionID">The Action to re-target.</param>
    /// <param name="target">
    ///     The output  <see cref="IGameObject">Game Object</see> of the target, if
    ///     the action was found to be re-targeted.
    /// </param>
    /// <returns>
    ///     Whether the action is registered for re-targeting.
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
    private static string GetMethodName(this TargetResolverDelegate resolver)
    {
        var resolverName = resolver.Method.Name;
        var resolverClass = resolver.Method.DeclaringType?.Name ?? "";

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
///     AST.CardsResolver
/// </seealso>
public delegate IGameObject? TargetResolverDelegate();
