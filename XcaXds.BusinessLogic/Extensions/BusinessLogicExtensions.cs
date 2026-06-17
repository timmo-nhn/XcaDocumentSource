using XcaXds.BusinessLogic.Models.Custom;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Extensions.No;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Shared.Constants;
using XcaXds.Shared.Enums;

namespace XcaXds.BusinessLogic.Extensions;

/// <summary>
/// Filters a document list based on more granular and business-oriented parameters than what PEP performs. Allows for partial (non-atomic) filtering of the document list
/// </summary>
public static class BusinessLogicExtensions
{
    /// <summary>
    /// Checks if an integer is within a certain range (inclusive).<para/> Returns true if it is, false otherwise.
    /// </summary>
    public static bool InRange(this int input, int lower, int upper)
    {
        return input >= lower && input <= upper;
    }

    public static string[]? GetAbacRequestAttributeAsString(AbacRequest? abacRequest, string attribute)
    {
        return abacRequest?.Attributes?.Where(att => att.Key.Contains(attribute)).SelectMany(v => v.Value).ToArray();
    }

    public static CodedValue? GetAbacRequestAttributesAsCodedValue(AbacRequest? abacRequest, string attribute)
    {
        if (abacRequest?.Attributes == null)
            return null;

        var indexed = abacRequest.Attributes.IndexAttributesWithPrefix(attribute);

        if (!indexed.TryGetValue($"{attribute}:code", out var code))
            return null;

        return new CodedValue
        {
            Code = code,
            CodeSystem = indexed.GetValueOrDefault($"{attribute}:codeSystem"),
            DisplayName = indexed.GetValueOrDefault($"{attribute}:displayName")
        };
    }
}