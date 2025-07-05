using WrathCombo.CustomComboNS.Functions;

namespace WrathCombo.Extensions;

internal static class ByteExtensions
{
    internal static string JobAbbreviation(this byte job) => CustomComboFunctions.JobIDs.JobIDToShorthand(job);
}