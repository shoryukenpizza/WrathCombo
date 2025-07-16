using System;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.Logging;
using ECommons.Reflection;
using WrathCombo.Services.IPC_Subscriber;
using EZ = ECommons.Throttlers.EzThrottler;
using TS = System.TimeSpan;

namespace WrathCombo.Data;

public static class ConflictingPluginsChecks
{
    private static bool _cancelConflictChecks = false;
    
    public static class BossMod
    {
        public static bool Conflicted;
        private static DateTime? _conflictRegistered;
        private static DateTime? _conflictFirstSeen;
        private static int _conflictsInARow;
        private static int _maxConflictsInARow = 4;

        public static void CheckForConflict()
        {
            // Throttle the BossMod check
            if (!EZ.Throttle("conflictCheckBossMod", TS.FromSeconds(8)) ||
                !Svc.Condition[ConditionFlag.InCombat])
                return;
            PluginLog.Verbose("[ConflictingPlugins] [BossMod] Performing Check ...");
#if DEBUG
            _maxConflictsInARow = 1;
#endif
            
            // Reset the conflict timer, must exceed the threshold within 2 minutes
            if (_conflictFirstSeen is not null &&
                _conflictFirstSeen - DateTime.Now > TS.FromMinutes(2))
            {
                PluginLog.Verbose("[ConflictingPlugins] [BossMod] Resetting Conflict Check");
                _conflictFirstSeen = null;
                _conflictsInARow = 0;
            }
            
            // Clear the conflict
            if (!BossModIPC.IsEnabled || // disabled
                (_conflictRegistered is not null && // there is a conflict currently
                 BossModIPC.LastModified() > _conflictRegistered)) // bm config changed
            {
                PluginLog.Verbose("[ConflictingPlugins] [BossMod] IPC not enabled, or config updated");
                Conflicted = false;
                _conflictsInARow = 0;
                return;
            }

            // Add a conflict note
            if (BossModIPC.HasAutomaticActionsQueued())
            {
                PluginLog.Verbose("[ConflictingPlugins] [BossMod] Actions are Queued");
                _conflictFirstSeen ??= DateTime.Now; // Set the time seen if empty
                _conflictsInARow++;
            }

            // Save a complete conflict
            if (_conflictsInARow > _maxConflictsInARow)
            {
                PluginLog.Debug("[ConflictingPlugins] [BossMod] Marked Conflict!");
                Conflicted = true;
                _conflictRegistered = DateTime.Now;
            }
        }
    }
    
    private static readonly Action RunChecks = () =>
    {
        if (_cancelConflictChecks)
            return;
        
        PluginLog.Verbose($"[ConflictingPlugins] Periodic check for conflicting plugins");
        BossMod.CheckForConflict();
        
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

    public static void Dispose() =>
        _cancelConflictChecks = true;
}