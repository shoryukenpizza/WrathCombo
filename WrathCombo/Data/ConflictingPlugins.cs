#region

using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin;
using ECommons.DalamudServices;

#endregion

namespace WrathCombo.Data;

public static class ConflictingPlugins
{
    #region Combo Conflicts

    /// <summary>
    ///     List of the most popular conflicting plugins.
    /// </summary>
    /// <remarks>
    ///     The list is case-sensitive, and needs to be lowercase.
    /// </remarks>
    private static string[] conflictingPluginsNames = new[]
    {
        "xivcombo",
        "xivcomboexpanded",
        "xivcomboexpandedest",
        "xivcombovx",
        "xivslothcombo",
        "rotationsolver",
    };

    /// <summary>
    ///     Searches for any enabled conflicting plugins.
    /// </summary>
    /// <param name="conflicts">
    ///     The output list of conflicting plugins.
    /// </param>
    /// <returns>
    ///     Whether there are any simple combo conflicts.
    /// </returns>
    private static bool TryGetSimpleComboConflicts(out Conflict[] conflicts)
    {
        conflicts = Svc.PluginInterface.InstalledPlugins
            .Where(x =>
                Enumerable.Contains(conflictingPluginsNames,
                    x.InternalName.ToLower()) &&
                x.IsLoaded)
            .Select(x => new Conflict(x.InternalName, ConflictType.Combo))
            .ToArray();

        return conflicts.Length > 0;
    }

    /// <summary>
    ///     Checks for nuanced conflicts, which are only conflicts under
    ///     certain conditions, and as such we actually need to check the settings
    ///     of such plugins.
    /// </summary>
    /// <param name="conflicts">
    ///     The output list of conflicting plugins.
    /// </param>
    /// <returns>
    ///     Whether there are any complex combo conflicts.
    /// </returns>
    private static bool TryGetComplexComboConflicts(out Conflict[] conflicts)
    {
        conflicts = [];

        // Reaction ?
        // Bossmod

        return conflicts.Length > 0;
    }

    #endregion

    #region Targeting Conflicts

    /// <summary>
    ///     Checks for targeting conflicts, which are also more complicated and
    ///     requires checking the settings of the plugins.
    /// </summary>
    /// <param name="conflicts">
    ///     The output list of conflicting plugins.
    /// </param>
    /// <returns>
    ///     Whether there are targeting conflicts.
    /// </returns>
    private static bool TryGetTargetingConflicts(out Conflict[] conflicts)
    {
        conflicts = [];

        // Redirect
        // Reaction

        return conflicts.Length > 0;
    }

    #endregion

    #region Setting Conflicts

    /// <summary>
    ///     Checks for conflicts from specific settings in other plugins,
    ///     like those that modify action queueing.
    /// </summary>
    /// <param name="conflicts">
    ///     The output list of conflicting plugins.
    /// </param>
    /// <returns>
    ///     Whether there are settings conflicts.
    /// </returns>
    private static bool TryGetSettingConflicts(out Conflict[] conflicts)
    {
        conflicts = [];

        // Bossmod

        return conflicts.Length > 0;
    }

    #endregion

    public static bool TryGetConflicts(out Conflicts conflicts)
    {
        conflicts = new Conflicts();
        
        Conflict[] complexCombos = [];
        if (TryGetSimpleComboConflicts(out var simpleCombos) ||
            TryGetComplexComboConflicts(out complexCombos))
            conflicts[ConflictType.Combo] = 
                simpleCombos.Concat(complexCombos).ToArray();
        
        if (TryGetTargetingConflicts(out var targetingConflicts))
            conflicts[ConflictType.Targeting] = targetingConflicts;
        
        if (TryGetSettingConflicts(out var settingConflicts))
            conflicts[ConflictType.Settings] = settingConflicts;
        
        return conflicts.ToArray().Length > 0;
    }

    public static void Draw()
    {
        if (!TryGetConflicts(out var conflicts))
            return;

        foreach (var conflict in conflicts.ToArray())
        {
            
        }
    }
}

public enum ConflictType
{
    Combo,
    Targeting,
    Settings,
}

public class Conflicts
{
    private readonly Dictionary<ConflictType, Conflict[]> _conflicts = [];
    public Conflict[] this[ConflictType type]
    {
        get => _conflicts.TryGetValue(type, out var conflicts) ? conflicts : [];
        set => _conflicts[type] = value;
    }
    
    public Conflict[] ToArray() => _conflicts.Values.SelectMany(x => x).ToArray();
}

public class Conflict
{
    private const string ComboConflictStart = "Conflicting Combo";
    private const string ComboConflictEnd = "Plugin(s) Detected!";
    private const string TargetingConflictStart = "Conflicting Action";
    private const string TargetingConflictEnd = "Retargeting Detected!";
    private const string SettingsConflictStart = "Conflicting Plugin";
    private const string SettingsConflictEnd = "Settings Detected!";

    private readonly IExposedPlugin _plugin;

    public Conflict(
        string internalName, ConflictType conflictType, string? reason = null)
    {
        var search = Svc.PluginInterface.InstalledPlugins
            .FirstOrDefault(x => x.InternalName == internalName);

        _plugin = search ?? throw new KeyNotFoundException(
            $"Plugin with internal name '{internalName}' not found.");
        ConflictType = conflictType;
        Reason = reason;
    }

    internal string Name => _plugin.Name;
    internal string InternalName => _plugin.InternalName;
    internal string Version => _plugin.Version.ToString();
    internal ConflictType ConflictType { get; }
    internal string? Reason { get; }

    internal string[] ConflictMessageParts =>
        ConflictType switch
        {
            ConflictType.Combo => [ComboConflictStart, ComboConflictEnd],
            ConflictType.Targeting => [TargetingConflictStart, TargetingConflictEnd],
            ConflictType.Settings => [SettingsConflictStart, SettingsConflictEnd],
            _ => throw new ArgumentOutOfRangeException(nameof(ConflictType),
                $"Unknown conflict type: {ConflictType}"),
        };
}