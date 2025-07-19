#region

using System;
using WrathCombo.Core;
using WrathCombo.Window.Functions;

// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace WrathCombo.Attributes;

/// <summary>
///     Flags a Preset as one that is Retargeted with
///     <see cref="ActionRetargeting" />.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal class RetargetedAttribute(params uint[] retargetedActions) : Attribute
{
    public uint[] RetargetedActions { get; } = retargetedActions ?? [];
}

/// <summary>
///     Flags a Preset as one that may be Retargeted with
///     <see cref="ActionRetargeting" />.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal class PossiblyRetargetedAttribute : Attribute
{
    private const string DefaultSettingInfo =
        "Settings Tab > Retarget (Single Target) Healing Actions";
    private const Condition DefaultCondition =
        Condition.RetargetHealingActionsEnabled;
    
    /// <summary>
    ///     Designates a Preset as one that may be Retargeted.
    /// </summary>
    /// <param name="settingInfo">
    ///     The setting that controls whether this Preset is Retargeted or not.
    /// </param>
    /// <param name="condition">
    ///     The <see cref="Condition" /> that needs to be satisfied to turn from
    ///     yellow to green.
    /// </param>
    /// <param name="retargetedActions">
    ///     List of all actions that are Retargeted by this Preset.
    /// </param>
    /// <remarks>
    ///     Defaults to the <paramref name="settingInfo" /> and
    ///     <paramref name="condition" /> for Single Target Healing Actions.
    /// </remarks>
    internal PossiblyRetargetedAttribute
    (string settingInfo = DefaultSettingInfo,
        Condition condition = DefaultCondition,
        params uint[] retargetedActions)
    {
        SettingInfo = settingInfo;
        PossibleCondition = condition;
        RetargetedActions = retargetedActions ?? [];
    }
    
    /// <seealso cref="PossiblyRetargetedAttribute(string, Condition, uint[])"/>
    internal PossiblyRetargetedAttribute(params uint[] retargetedActions)
        : this(DefaultSettingInfo, DefaultCondition, retargetedActions)
    {
    }

    public string SettingInfo { get; }
    public Condition PossibleCondition { get; }
    public uint[] RetargetedActions { get; }

    /// <summary>
    ///     These conditions should be implemented in the switch in
    ///     <see cref="Presets.DrawRetargetedAttribute">
    ///         DrawRetargetedAttribute()
    ///     </see>.
    /// </summary>
    internal enum Condition
    {
        RetargetHealingActionsEnabled,
        ASTQuickTargetCardsFeatureEnabled,
    }
}