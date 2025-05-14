#region

using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
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
    ///     resolve the target for the ActionID.<br />
    ///     — A <see cref="uint" /> that is the replaced action for the combo this
    ///     retargeting was ran in.
    /// </remarks>
    /// <seealso cref="Register" />
    private static readonly Dictionary<uint,
            (TargetResolverDelegate resolver, uint replacedAction)>
        _targetResolvers = [];

    /// <summary>
    ///     Register an action as one you want re-targeted.
    /// </summary>
    /// <param name="actionID">The Action to retarget.</param>
    /// <param name="replacedActionID">The Action the combo is replacing</param>
    /// <param name="resolver">
    ///     The <see cref="TargetResolverDelegate" /> to resolve the target.<br />
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
    ///     <see cref="Extensions.UIntExtensions.Retarget(uint,TargetResolverDelegate)">(uint).Retarget()</see>
    ///     simpler.
    /// </returns>
    /// <remarks>
    ///     Should only be called by
    ///      <see cref="Extensions.UIntExtensions.Retarget(uint,TargetResolverDelegate)">(uint).Retarget()</see>.
    /// </remarks>
    internal static uint Register
    (uint actionID, TargetResolverDelegate resolver,
        uint? replacedActionID = null)
    {
        // Limit spam from the same actionID
        if (!EZ.Throttle($"retargetingFor{actionID}", TS.FromSeconds(1)))
            return actionID;

        // Cleaning up the old target resolver
        if (_targetResolvers.TryGetValue(actionID, out var old))
        {
            // Keep the old resolver if it's <10 seconds old
            if (old.resolver.Method.Name == resolver.Method.Name &&
                !EZ.Throttle($"retargetingOver{actionID}", TS.FromSeconds(10)))
                return actionID;
            // Unregister the old resolver (just when different)
            Unregister(actionID);
            PluginLog.Verbose(
                $"[ActionRetargeting] overwriting retargeting for " +
                $"'{actionID.ActionName()}'");
        }

        // Save the resolver
        PluginLog.Verbose("[ActionRetargeting] registering " +
                          $"'{actionID.ActionName()}' for retargeting " +
                          $"with {resolver.GetMethodName()}");
        _targetResolvers.Add(actionID, (resolver, replacedActionID ?? actionID));

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
        target = null;

        // Find the target resolver
        TargetResolverDelegate? targetResolver = null;
        if (!_targetResolvers.TryGetValue(actionID, out var retargeting))
        {
            // Do another search for the replaced action
            var extraSearch = _targetResolvers
                .FirstOrDefault(kv => kv.Value.replacedAction == actionID).Value.resolver;
            if (extraSearch is null)
                return false;
            targetResolver = extraSearch;
        }

        // Find the target resolver if it is a replaced-action resolver
        targetResolver ??= retargeting.resolver;

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
                            $"'{actionID.ActionName()}' " +
                            $"with {targetResolver.GetMethodName()}:\n{ex}");
            return false;
        }

        // Return the results
        PluginLog.Verbose("[ActionRetargeting] re-targeted " +
                          $"'{actionID.ActionName()}' to " +
                          $"'{target?.Name ?? "null"}' " +
                          $"(with {targetResolver.GetMethodName()})");
        return target != null;
    }

    #region Utilities

    /// Clears old re-targets from the
    /// <see cref="_targetResolvers">list</see>
    /// .
    internal static Action ClearOldRetargets = () =>
    {
        var oldRetargets = _targetResolvers.Keys
            .Where(key =>
                !EZ.Throttle($"retargetingFor{key}", TS.FromSeconds(30)));

        foreach (var key in oldRetargets)
        {
            Unregister(key);
            PluginLog.Verbose("[ActionRetargeting] cleared old re-target for " +
                              $"'{key.ActionName()}'");
        }

        Svc.Framework.RunOnTick(ClearOldRetargets!, TS.FromSeconds(25));
    };

    /// Clears
    /// <see cref="_targetResolvers">cached re-targets</see>
    /// .
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
            "<>c" => "<CustomResolver>",
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
