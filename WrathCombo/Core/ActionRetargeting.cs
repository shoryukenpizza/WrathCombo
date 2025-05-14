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
    ///     — The ActionID that will be re-targeted.<br />
    ///     — A <see cref="uint" /> that is the replaced action for the combo this
    ///     retargeting was ran in.
    ///     <br /><br />
    ///     <b>
    ///         <c>val</c>
    ///     </b>
    ///     — A <see cref="TargetResolverDelegate" /> that will
    ///     resolve the target for the ActionID.
    /// </remarks>
    /// <seealso cref="Register" />
    private static readonly Dictionary<(uint action, uint replacedAction),
            TargetResolverDelegate>
        _targetResolvers = [];

    /// <summary>
    ///     Register an action as one you want re-targeted.
    /// </summary>
    /// <param name="action">The Action to retarget.</param>
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
    ///     The <paramref name="action" /> that was registered.<br />
    ///     This only really returns to make
    ///     <see cref="Extensions.UIntExtensions.Retarget(uint,TargetResolverDelegate)">(uint).Retarget()</see>
    ///     simpler.
    /// </returns>
    /// <remarks>
    ///     Should only be called by
    ///     <see cref="Extensions.UIntExtensions.Retarget(uint,TargetResolverDelegate)">(uint).Retarget()</see>
    ///     .
    /// </remarks>
    internal static uint Register
    (uint action, TargetResolverDelegate resolver,
        uint? replacedActionID = null)
    {
        // Handle non-nullable actionID
        var replaced = replacedActionID ?? action;

        // Limit spam from the same actionID
        if (!EZ.Throttle($"retargetingFor{action}{replaced}", TS.FromSeconds(1)))
            return action;

        // Cleaning up the old target resolver
        if (_targetResolvers.TryGetValue((action, replaced), out var old))
        {
            // Keep the old resolver if it's <10 seconds old
            if (old.Method.Name == resolver.Method.Name &&
                !EZ.Throttle($"retargetingOver{action}{replaced}", TS
                .FromSeconds(10)))
                return action;
            // Unregister the old resolver (just when different)
            Unregister(action, replaced);
            PluginLog.Verbose(
                $"[ActionRetargeting] overwriting retargeting for " +
                $"'{action.ActionName()}'");
        }

        // Save the resolver
        PluginLog.Verbose("[ActionRetargeting] registering " +
                          $"'{action.ActionName()}' for retargeting " +
                          $"with {resolver.GetMethodName()}");
        _targetResolvers.Add((action, replaced), resolver);

        return action;
    }

    /// <summary>
    ///     Unregister an action from being re-targeted.
    /// </summary>
    /// <param name="actionID">
    ///     The Action to remove from the
    ///     <see cref="_targetResolvers">list of Re-Targeted Actions</see>.
    /// </param>
    /// <param name="replacedActionID">The replaced Action key.</param>
    private static void Unregister(uint actionID, uint replacedActionID) =>
        _targetResolvers.Remove((actionID, replacedActionID));

    /// <summary>
    ///     Resolve the target for an action via the provided
    ///     <see cref="TargetResolverDelegate" />.
    /// </summary>
    /// <param name="action">The Action to re-target.</param>
    /// <param name="target">
    ///     The output  <see cref="IGameObject">Game Object</see> of the target, if
    ///     the action was found to be re-targeted.
    /// </param>
    /// <returns>
    ///     Whether the action is registered for re-targeting.
    /// </returns>
    public static bool TryGetTargetFor(uint action, out IGameObject? target)
    {
        target = null;

        // Find the target resolver
        uint replacedAction = 0;
        if (!_targetResolvers.TryGetValue((action, action), out var targetResolver))
        {
            // Do another search for the replaced action
            var extraSearch = _targetResolvers
                .Where(kv => kv.Key.replacedAction == action)
                .Select(kv => (Resolver: kv.Value,
                    ReplacedAction: kv.Key.replacedAction))
                .FirstOrDefault();
            if (extraSearch.Resolver is null)
                return false;
            replacedAction = extraSearch.ReplacedAction;
            targetResolver = extraSearch.Resolver;
        }

        replacedAction = replacedAction == 0 ? action : replacedAction;

        PluginLog.Debug("[ActionRetargeting] re-targeting " +
                        $"'{action.ActionName()}' " +
                        $"with {targetResolver.GetMethodName()}");

        // Run the target resolver
        Unregister(action, replacedAction);
        try
        {
            target = targetResolver.Invoke();
        }
        catch (Exception ex)
        {
            PluginLog.Error("[ActionRetargeting] error while resolving target for " +
                            $"'{action.ActionName()}' " +
                            $"with {targetResolver.GetMethodName()}:\n{ex}");
            return false;
        }

        // Return the results
        PluginLog.Verbose("[ActionRetargeting] re-targeted " +
                          $"'{action.ActionName()}' to " +
                          $"'{target?.Name ?? "null"}' " +
                          $"(with {targetResolver.GetMethodName()})");
        return target != null;
    }

    #region Utilities

    /// Clears old re-targets from the <see cref="_targetResolvers">list</see>.
    internal static Action ClearOldRetargets = () =>
    {
        var oldRetargets = _targetResolvers.Keys
            .Where(key =>
                !EZ.Throttle($"retargetingFor{key}", TS.FromSeconds(30)));

        foreach (var key in oldRetargets)
        {
            Unregister(key.action, key.replacedAction);
            PluginLog.Verbose("[ActionRetargeting] cleared old re-target for " +
                              $"'{key.action.ActionName()}'");
        }

        Svc.Framework.RunOnTick(ClearOldRetargets!, TS.FromSeconds(25));
    };

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
