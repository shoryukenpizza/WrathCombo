#region

using System;
using System.Collections.Generic;
using System.Linq;
using ECommons;
using ECommons.Logging;
using ECommons.Reflection;

#endregion

namespace WrathCombo.Services.IPC_Subscriber;

public class ReActionIPC(
    string pluginName,
    Version validVersion)
    : ReusableIPC(pluginName, validVersion, true)
{
    private object Configuration =>
        Plugin.GetFoP("Config");

    private bool AutoTarget =>
        Configuration.GetFoP<bool>("EnableAutoTarget");

    private List<object> Stacks =>
        (Configuration.GetFoP("ActionStacks") as IEnumerable<object>)?.ToList() ??
        [];

    public (uint Action, string stackName)[] GetRetargetedActions
        (bool includeMetaActions = false)
    {
        if (!IsEnabled)
        {
            PluginLog.Debug($"[ConflictingPlugins] [{PluginName}] is not enabled.");
            return [];
        }

        try
        {
            List<(uint Action, string stackName)> workingActions = [];

            foreach (var stack in Stacks)
            {
                var name = stack.GetFoP<string>("Name");
                var actionsToBeRetargeted =
                    (stack.GetFoP("Actions") as IEnumerable<object>)?.ToArray();

                if (actionsToBeRetargeted == null ||
                    actionsToBeRetargeted.Length == 0)
                {
                    PluginLog.Verbose(
                        $"[ConflictingPlugins] [{PluginName}] " +
                        $"Stack `{name}` has no actions to retarget.");
                    continue;
                }

                // Add all retargeted actions to the list
                workingActions.AddRange(actionsToBeRetargeted!
                    .Select(x => (x.GetFoP<uint>("ID"), name)));

                var actionsToUseWhenRetargeting =
                    (stack.GetFoP("Items") as IEnumerable<object>)?.ToArray();

                if (actionsToUseWhenRetargeting == null ||
                    actionsToUseWhenRetargeting.Length == 0)
                {
                    PluginLog.Warning(
                        $"[ConflictingPlugins] [{PluginName}] " +
                        $"Stack `{name}` has no items to use when retargeting.");
                    continue;
                }

                // Add all actions to use when retargeting to the list
                workingActions.AddRange(actionsToUseWhenRetargeting
                    .Where(x => x.GetFoP<uint>("ID") != 0) // exclude "Same Action"
                    .Select(x => (x.GetFoP<uint>("ID"), name)));
            }

            // Remove "all actions", etc. from the list, unless desired
            if (!includeMetaActions)
                workingActions = workingActions.Where(a => a.Action > 2).ToList();

            var actions = workingActions.ToArray();
            PluginLog.Verbose(
                $"[ConflictingPlugins] [{PluginName}] Has `StackActions`: " +
                (actions.Length > 0));
            return actions;
        }
        catch (Exception e)
        {
            PluginLog.Warning($"[ConflictingPlugins] [{PluginName}] " +
                              $"Getting `StackActions` failed: {e.ToStringFull()}");
            return [];
        }
    }

    public bool IsAutoTargetingEnabled()
    {
        PluginLog.Verbose(
            $"[ConflictingPlugins] [{PluginName}] " +
            $"Has `AutoTarget`: {AutoTarget}");

        return AutoTarget;
    }

    public bool AreAllActionsRetargeted(out string stackName)
    {
        var stacks = GetRetargetedActions(true);
        var hasAllActionsRetargeted = stacks.Length > 0 &&
                                      stacks.Any(x => x.Action == 0);
        stackName = stacks
            .Where(x => x.Action == 0).Select(x => x.stackName)
            .FirstOrDefault() ?? "";
        PluginLog.Verbose(
            $"[ConflictingPlugins] [{PluginName}] " +
            $"Has `AllActionsRetargeted`: {hasAllActionsRetargeted}");

        return hasAllActionsRetargeted;
    }

    public bool AreHarmfulActionsRetargeted(out string stackName)
    {
        var stacks = GetRetargetedActions(true);
        var hasHarmfulActionsRetargeted = stacks.Length > 0 &&
                                          stacks.Any(x => x.Action == 1);
        stackName = stacks
            .Where(x => x.Action == 1).Select(x => x.stackName)
            .FirstOrDefault() ?? "";
        PluginLog.Verbose(
            $"[ConflictingPlugins] [{PluginName}] " +
            $"Has `HarmfulActionsRetargeted`: {hasHarmfulActionsRetargeted}");

        return hasHarmfulActionsRetargeted;
    }

    public bool AreBeneficialActionsRetargeted(out string stackName)
    {
        var stacks = GetRetargetedActions(true);
        var hasBeneficialActionsRetargeted = stacks.Length > 0 &&
                                             stacks.Any(x => x.Action == 2);
        stackName = stacks
            .Where(x => x.Action == 2).Select(x => x.stackName)
            .FirstOrDefault() ?? "";
        PluginLog.Verbose(
            $"[ConflictingPlugins] [{PluginName}] " +
            $"Has `BeneficialActionsRetargeted`: {hasBeneficialActionsRetargeted}");

        return hasBeneficialActionsRetargeted;
    }
}