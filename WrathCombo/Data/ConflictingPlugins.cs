using System.Linq;
using ECommons.DalamudServices;

namespace WrathCombo.Data;

public static class ConflictingPlugins
{
    /// <summary>
    /// List of the most popular conflicting plugins.
    /// </summary>
    /// <remarks>
    /// The list is case-sensitive, and needs to be lowercase.
    /// </remarks>
    private static string[] conflictingPluginsNames = new string[]
    {
        "xivcombo",
        "xivcomboexpanded",
        "xivcomboexpandedest",
        "xivcombovx",
        "xivslothcombo",
        "rotationsolver",
    };

    /// <summary>
    ///     Checks for nuanced conflicts, which are only conflicts under
    ///     certain conditions, and as such we actually need to check the settings
    ///     of such plugins.
    /// </summary>
    /// <param name="conflictingPlugins">
    ///     The output list of conflicting plugins, if any.
    /// </param>
    /// <returns>
    ///     Whether there are complicated conflicts.
    /// </returns>
    private static bool TryGetComplicatedComboConflicts(out string[]? conflictingPlugins)
    {
        conflictingPlugins = null;
        
        // Reaction
        // Redirect
        // 

        return (conflictingPlugins?.Length ?? 0) > 0;
    }

    /// <summary>
    ///     Searches for any enabled conflicting plugins.
    /// </summary>
    /// <returns>
    ///     Whether conflicts were found.
    /// </returns>
    /// <remarks>
    ///     Each <c>string</c> would be <c>InternalName(Version)</c>
    /// </remarks>
    public static bool TryGetComboPlugins(out string[]? conflictingPlugins)
    {
        conflictingPlugins = Svc.PluginInterface.InstalledPlugins
            .Where(x => conflictingPluginsNames.Contains(x.InternalName.ToLower()) && x.IsLoaded)
            .Select(x => $"{x.InternalName}({x.Version})")
            .ToArray();
        
        if (TryGetComplicatedComboConflicts(out var complicatedConflicts))
            conflictingPlugins = conflictingPlugins.Concat(complicatedConflicts!).ToArray();

        return conflictingPlugins.Length > 0;
    }
}
