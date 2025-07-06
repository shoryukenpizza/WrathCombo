using System;

namespace WrathCombo.Attributes;

[AttributeUsage(AttributeTargets.Field)]
internal class SimpleCombo : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
internal class AdvancedCombo : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
internal class BasicCombo : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
internal class HealingCombo : Attribute
{
}