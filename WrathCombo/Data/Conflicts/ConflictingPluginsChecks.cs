#region

using System;
using System.Linq;
using ECommons.DalamudServices;
using ECommons.Logging;
using WrathCombo.Core;
using WrathCombo.Extensions;
using WrathCombo.Services.IPC_Subscriber;
using EZ = ECommons.Throttlers.EzThrottler;
using TS = System.TimeSpan;

#endregion

namespace WrathCombo.Data.Conflicts;

public static class ConflictingPluginsChecks
{
    private static bool _cancelConflictChecks;

    private static readonly Action RunChecks = () =>
    {
        if (_cancelConflictChecks)
            return;

        PluginLog.Verbose(
            "[ConflictingPlugins] Periodic check for conflicting plugins");

        BossMod.CheckForConflict();
        BossModReborn.CheckForConflict();
        Redirect.CheckForConflict();
        MOAction.CheckForConflict();

        Svc.Framework.RunOnTick(RunChecks!, TS.FromSeconds(4.11));
    };

    internal static BossModCheck BossMod { get; } = new();
    internal static BossModCheck BossModReborn { get; } = new(true);
    internal static RedirectCheck Redirect { get; } = new();
    internal static MOActionCheck MOAction { get; } = new();

    public static void Begin()
    {
        // ReSharper disable once RedundantAssignment
        var ts = TS.FromMinutes(1); // 1m initial delay after plugin launch
#if DEBUG
        ts = TS.FromSeconds(10); // 10s for debug mode
#endif

        Svc.Framework.RunOnTick(RunChecks, ts);
    }

    public static void Dispose()
    {
        BossMod.Dispose();
        BossModReborn.Dispose();
        MOAction.Dispose();
        _cancelConflictChecks = true;
    }

    internal sealed class BossModCheck(bool reborn = false)
        : ConflictCheck(!reborn
            ? new BossModIPC("BossMod", new Version(0, 3, 1, 0))
            : new BossModIPC("BossModReborn", new Version(7, 2, 5, 90)))
    {
        private DateTime? _conflictFirstSeen;
        private DateTime? _conflictRegistered;
        private int _conflictsInARow;
        private int _maxConflictsInARow = 4;

        protected override BossModIPC IPC => (BossModIPC)_ipc;

        public override void CheckForConflict()
        {
            if (!ThrottlePassed(8, false))
                return;

#if DEBUG
            _maxConflictsInARow = 1;
#endif

            // Reset the conflict timer, must exceed the threshold within 2 minutes
            if (_conflictFirstSeen is not null &&
                _conflictFirstSeen - DateTime.Now > TS.FromMinutes(2))
            {
                PluginLog.Verbose(
                    $"[ConflictingPlugins] [{Name}] Resetting Conflict Check");
                _conflictFirstSeen = null;
                _conflictsInARow = 0;
            }

            // Clear the conflict
            if (!IPC.IsEnabled || // disabled
                (_conflictRegistered is not null && // there is a conflict marked
                 IPC.LastModified() > _conflictRegistered)) // bm config changed
            {
                PluginLog.Verbose(
                    $"[ConflictingPlugins] [{Name}] IPC not enabled, " +
                    $"or config updated");
                Conflicted = false;
                _conflictsInARow = 0;
                return;
            }

            // Bail if the IPC is not enabled
            if (!IPC.IsEnabled) return;

            // Add a conflict note
            if (IPC.HasAutomaticActionsQueued())
            {
                PluginLog.Verbose(
                    $"[ConflictingPlugins] [{Name}] Actions are Queued");
                _conflictFirstSeen ??= DateTime.Now; // Set the time seen if empty
                _conflictsInARow++;
            }

            // Save a complete conflict
            // ReSharper disable once InvertIf
            if (_conflictsInARow > _maxConflictsInARow)
            {
                _conflictRegistered = DateTime.Now;
                MarkConflict();
            }
        }
    }

    internal sealed class MOActionCheck() : ConflictCheck(new MOAction())
    {
        public uint[] ConflictingActions = [];
        protected override MOAction IPC => (MOAction)_ipc;

        public override void CheckForConflict()
        {
            if (!ThrottlePassed(20))
                return;

            var moActionRetargeted = IPC.GetRetargetedActions().ToHashSet();
            if (moActionRetargeted.Count != 0)
                PluginLog.Verbose(
                    $"[ConflictingPlugins] [{Name}] {moActionRetargeted.Count} Retargeted Actions Found");

            var wrathRetargeted = PresetStorage.AllRetargetedActions.ToHashSet();
            if (moActionRetargeted.Overlaps(wrathRetargeted))
            {
                ConflictingActions =
                    moActionRetargeted.Intersect(wrathRetargeted).ToArray();
                MarkConflict();
            }
            else
            {
                ConflictingActions = [];
                Conflicted = false;
            }
        }
    }

    internal sealed class RedirectCheck() : ConflictCheck(new Redirect())
    {
        public uint[] ConflictingActions = [];
        protected override Redirect IPC => (Redirect)_ipc;

        public override void CheckForConflict()
        {
            if (!ThrottlePassed())
                return;

            ConflictingActions = [];

            var conflictedThisCheck = false;
            var wrathRetargeted = PresetStorage.AllRetargetedActions.ToHashSet();

            // Check if all Ground Targeted Actions are redirected
            if (IPC.AreGroundTargetedActionsRedirected() &&
                wrathRetargeted.Any(x => x.IsGroundTargeted()))
            {
                PluginLog.Verbose(
                    $"[ConflictingPlugins] [{Name}] Ground Targeted Actions are Redirected");
                ConflictingActions = [1];
                MarkConflict();
                conflictedThisCheck = true;
            }

            // Check if all Beneficial Actions are redirected
            if (IPC.AreBeneficialActionsRedirected() &&
                wrathRetargeted.Any(x => x.IsFriendlyTargetable()))
            {
                PluginLog.Verbose(
                    $"[ConflictingPlugins] [{Name}] Beneficial Actions are Redirected");
                if (ConflictingActions.Length == 0)
                    ConflictingActions = [0, 1];
                else
                    ConflictingActions = [1, 1];
                MarkConflict();
            }

            // Check for individual Actions Retargeted
            var redirectRetargeted = IPC.GetRetargetedActions().ToHashSet();
            if (redirectRetargeted.Count != 0)
                PluginLog.Verbose(
                    $"[ConflictingPlugins] [{Name}] {redirectRetargeted.Count} Retargeted Actions Found");
            if (redirectRetargeted.Overlaps(wrathRetargeted))
            {
                var intersection = redirectRetargeted.Intersect(wrathRetargeted)
                    .ToArray();
                PluginLog.Verbose(
                    $"[ConflictingPlugins] [{Name}] " +
                    $"{intersection.Length} Overlapping Retargeted Actions Found");
                ConflictingActions =
                    ConflictingActions.Concat(intersection).ToArray();
                MarkConflict();
            }

            // Remove conflict if none were found this check
            // ReSharper disable once InvertIf
            if (!conflictedThisCheck && Conflicted)
                Conflicted = false;
        }
    }

    internal abstract class ConflictCheck : IDisposable
    {
        // ReSharper disable once InconsistentNaming
        protected readonly ReusableIPC _ipc;

        protected ConflictCheck(ReusableIPC ipc)
        {
            _ipc = ipc;
            if (_ipc.IsEnabled)
                PluginLog.Verbose(
                    $"[ConflictingPlugins] [{Name}] Setup for Checking " +
                    $"(v{ipc.InstalledVersion})");
        }

        // ReSharper disable once UnusedMemberInSuper.Global
        protected abstract ReusableIPC IPC { get; }

        public bool Conflicted { get; protected set; }

        protected string Name => _ipc.PluginName;

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public virtual void Dispose() => _ipc.Dispose();

        // ReSharper disable once UnusedMemberInSuper.Global
        public abstract void CheckForConflict();

        /// <summary>
        ///     Checks if an EZ Throttle passes, and if the plugin is enabled.
        /// </summary>
        /// <param name="frequency">
        ///     The frequency - in seconds - that must have passed since the last
        ///     check.
        /// </param>
        /// <param name="enabledCheck">
        ///     Whether to check if the plugin is enabled as well.
        /// </param>
        /// <returns>
        ///     If the <see cref="CheckForConflict" /> should be run or not.
        /// </returns>
        protected bool ThrottlePassed(int frequency = 10, bool enabledCheck = true)
        {
            if (!EZ.Throttle($"conflictCheck{Name}",
                    TS.FromSeconds(frequency)) ||
                (enabledCheck && !_ipc.IsEnabled))
                return false;

            PluginLog.Verbose($"[ConflictingPlugins] [{Name}] Performing Check ...");

            return true;
        }

        /// <summary>
        ///     Marks the plugin as conflicted, and logs the event.
        /// </summary>
        protected void MarkConflict()
        {
            if (!Conflicted)
                PluginLog.Information($"[ConflictingPlugins] [{Name}] " +
                                      "Marked Conflict!");
            Conflicted = true;
        }
    }
}