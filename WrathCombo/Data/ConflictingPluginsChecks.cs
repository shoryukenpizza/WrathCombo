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
    
    private static bool _cancelConflictChecks = false;
    
    private static readonly Action RunChecks = () =>
    {
        if (_cancelConflictChecks)
            return;
        
        PluginLog.Verbose($"[ConflictingPlugins] Periodic check for conflicting plugins");
        Bossmod.CheckForConflict();
        
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