#region

using System;
using ECommons;
using ECommons.EzIpcManager;
using ECommons.Logging;
using ECommons.Reflection;

#endregion

namespace WrathCombo.Services.IPC_Subscriber;

internal class BossModIPC : IDisposable
{
    private readonly EzIPCDisposalToken[] _disposalTokens;

    public readonly string PluginName;

    private readonly Version _validVersion;

    public BossModIPC
    (string pluginName = "BossMod",
        Version? validVersion = null)
    {
        PluginName = pluginName;
        _validVersion = validVersion ?? new Version(0, 3, 0, 6);
        _disposalTokens = EzIPC.Init(this, PluginName, SafeWrapper.IPCException);
    }

    public void Dispose()
    {
        foreach (var token in _disposalTokens)
            try
            {
                token.Dispose();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
    }

    public bool HasAutomaticActionsQueued()
    {
        if (!IsEnabled)
        {
            PluginLog.Debug($"[{PluginName}] {PluginName} is not enabled.");
            return false;
        }

        try
        {
            var hasEntries = _hasEntries();
            PluginLog.Verbose(
                $"[{PluginName}] `ActionQueue.HasEntries`: {hasEntries} ");
            return hasEntries;
        }
        catch (Exception e)
        {
            e.Log();
            return false;
        }
    }

    public DateTime LastModified()
    {
        if (!IsEnabled) return DateTime.MinValue;

        try
        {
            return _lastModified();
        }
        catch (Exception e)
        {
            e.Log();
            return DateTime.MinValue;
        }
    }

    #region Version Checking

    public bool IsEnabled =>
        InstalledVersion >= _validVersion || // release version
        InstalledVersion == new Version(0, 0, 0, 0); // debug version

    public Version InstalledVersion =>
        (DalamudReflector.TryGetDalamudPlugin(PluginName, out var dalamudPlugin,
            false, true)
            ? dalamudPlugin.GetType().Assembly.GetName().Version!
            : new Version(0, 0, 0, 1)); // no version found

    #endregion

#pragma warning disable CS0649, CS8618 // Complaints of the method
    [EzIPC("Rotation.ActionQueue.HasEntries")]
    private readonly Func<bool> _hasEntries = null!;

    [EzIPC("Configuration.LastModified")]
    private readonly Func<DateTime> _lastModified = null!;
#pragma warning restore CS8618, CS0649
}