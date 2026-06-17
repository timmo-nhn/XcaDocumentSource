using Hl7.Fhir.ElementModel;

namespace XcaXds.Commons.Extensions;

public static class TypedElementExtensions
{
    public static string? GetParentName(this ITypedElement element)
    {
        if (string.IsNullOrWhiteSpace(element.Location))
        {
            return null;
        }

        var parts = element.Location.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        var parentSegment = parts[^2];
        var bracketIndex = parentSegment.IndexOf('[');

        return bracketIndex >= 0
            ? parentSegment[..bracketIndex]
            : parentSegment;
    }
}
