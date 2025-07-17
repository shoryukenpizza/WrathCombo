using System;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.Logging;
using WrathCombo.Services.IPC_Subscriber;
using EZ = ECommons.Throttlers.EzThrottler;
using TS = System.TimeSpan;

namespace WrathCombo.Data;

public static class ConflictingPluginsChecks
{
    private static bool _cancelConflictChecks;
    
    internal static BossModCheck BossMod { get; } = new();
    internal static BossModCheck BossModReborn { get; } = new(true);

    public class BossModCheck
    {
        private BossModIPC IPC;
        private string Name => IPC.PluginName;
        
        public bool Conflicted;
        private DateTime? _conflictRegistered;
        private DateTime? _conflictFirstSeen;
        private int _conflictsInARow;
        private int _maxConflictsInARow = 4;
        
        public BossModCheck(bool reborn = false)
        {
            IPC = !reborn
                ? new BossModIPC()
                : new BossModIPC("BossModReborn", new Version(7, 2, 5, 87));
            PluginLog.Verbose($"[ConflictingPlugins] [{Name}] Set up for Checking");
        }

        public void CheckForConflict()
        {
            // Throttle the BossMod check
            if (!EZ.Throttle($"conflictCheck{Name}", TS.FromSeconds(8)) ||
                !Svc.Condition[ConditionFlag.InCombat])
                return;
            
            PluginLog.Verbose($"[ConflictingPlugins] [{Name}] Performing Check ...");
            
#if DEBUG
            _maxConflictsInARow = 1;
#endif

            // Reset the conflict timer, must exceed the threshold within 2 minutes
            if (_conflictFirstSeen is not null &&
                _conflictFirstSeen - DateTime.Now > TS.FromMinutes(2))
            {
                PluginLog.Verbose($"[ConflictingPlugins] [{Name}] Resetting Conflict Check");
                _conflictFirstSeen = null;
                _conflictsInARow = 0;
            }

            // Clear the conflict
            if (!IPC.IsEnabled || // disabled
                (_conflictRegistered is not null && // there is a conflict currently
                 IPC.LastModified() > _conflictRegistered)) // bm config changed
            {
                PluginLog.Verbose($"[ConflictingPlugins] [{Name}] IPC not enabled, or config updated");
                Conflicted = false;
                _conflictsInARow = 0;
                return;
            }

            // Add a conflict note
            if (IPC.HasAutomaticActionsQueued())
            {
                PluginLog.Verbose($"[ConflictingPlugins] [{Name}] Actions are Queued");
                _conflictFirstSeen ??= DateTime.Now; // Set the time seen if empty
                _conflictsInARow++;
            }

            // Save a complete conflict
            if (_conflictsInARow > _maxConflictsInARow)
            {
                PluginLog.Debug($"[ConflictingPlugins] [{Name}] Marked Conflict!");
                Conflicted = true;
                _conflictRegistered = DateTime.Now;
            }
        }

        // ReSharper disable once InconsistentNaming
        public void dispose()
        {
            IPC.Dispose();
        }
    }

    private static readonly Action RunChecks = () =>
    {
        if (_cancelConflictChecks)
            return;

        PluginLog.Verbose($"[ConflictingPlugins] Periodic check for conflicting plugins");
        BossMod.CheckForConflict();
        BossModReborn.CheckForConflict();

        Svc.Framework.RunOnTick(RunChecks!, TS.FromSeconds(4.11));
    };

    public static void Begin()
    {
        // ReSharper disable once RedundantAssignment
        var ts = TS.FromMinutes(1); // 1m initial delay after plugin launch
#if DEBUG
        ts = TS.FromSeconds(10); // 10s for debug mode
#endif

        Svc.Framework.RunOnTick(RunChecks, ts);
    }

    public static void Dispose() {
        BossMod.dispose();
        BossModReborn.dispose();
        _cancelConflictChecks = true;
    }
}