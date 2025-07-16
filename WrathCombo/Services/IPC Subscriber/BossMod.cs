using System;
using System.Collections.Generic;
using ECommons;
using ECommons.EzIpcManager;
using ECommons.Logging;
using ECommons.Reflection;

namespace WrathCombo.Services.IPC_Subscriber;

internal static class BossModIPC
{
    private static EzIPCDisposalToken[] _disposalTokens =
        EzIPC.Init(typeof(BossModIPC), "BossMod", SafeWrapper.None);

    internal static bool IsEnabled =>
        InstalledVersion >= ValidVersion || // release version
        InstalledVersion == new Version(0, 0, 0, 0); // debug version
    
    internal static Version InstalledVersion =>
        (DalamudReflector.TryGetDalamudPlugin("BossMod", out var dalamudPlugin, false, true)
            ? dalamudPlugin.GetType().Assembly.GetName().Version!
            : new Version(0, 0, 0, 1)); // no version found
    
    private static readonly Version ValidVersion = new(0,3,0,6);
    
    internal static bool HasAutomaticActionsQueued()
    {
        if (!IsEnabled)
        {
            PluginLog.Debug("[BossMod] BossMod is not enabled.");
            return false;
        }
        
        var hasEntries = HasEntries();
        PluginLog.Verbose($"[BossMod] `ActionQueue.HasEntries`: {hasEntries} ");
        return hasEntries;
    }
    
#pragma warning disable CS0649, CS8618 // EzIPC complaints
    [EzIPC("Configuration.%m")]
    internal static readonly Func<DateTime> LastModified;
    [EzIPC("Rotation.ActionQueue.%m")]
    private static readonly Func<bool> HasEntries;
#pragma warning restore CS8618, CS0649

    internal static void Dispose()
    {
        foreach (var token in _disposalTokens)
            try { token.Dispose(); }
            catch (Exception ex) { ex.Log(); }
    }
}