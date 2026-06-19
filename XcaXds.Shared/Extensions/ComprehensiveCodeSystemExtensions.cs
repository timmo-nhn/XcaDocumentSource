using Hl7.Fhir.Validation;
using XcaXds.Shared.Models.Custom;

namespace XcaXds.Shared.Extensions;


public static class ComprehensiveCodeSystemExtensions
{
    public static string[]? SystemOids(this IEnumerable<ComprehensiveCodeSystem> source)
    {
        return [.. source.Select(ccs => ccs.SystemOid).OfType<string>()];
    }

    public static string[]? SystemUrls(this IEnumerable<ComprehensiveCodeSystem> source)
    {
        return [.. source.Select(ccs => ccs.SystemUrl).OfType<string>()];
    }

    public static CodeSystemValue[]? Values(this IEnumerable<ComprehensiveCodeSystem> source, string system)
    {
        return source.Where(sys =>
            sys.SystemOid?.NoUrn() == system.NoUrn() ||
            sys.SystemUrl == system.NoUrn())?
            .Values()?.ToArray();
    }

    public static CodeSystemValue[]? Values(this IEnumerable<ComprehensiveCodeSystem> source)
    {
        var elements = source.SelectMany(v => v.Values ?? []).ToArray();

        return elements.Length > 0 ? [.. elements] : null;
    }

    /// <summary>
    /// Get a certain value, and its associated system. If the value is not found, returns null.
    /// </summary>
    public static KeyValuePair<string, string>? GetByValueOid(this IEnumerable<ComprehensiveCodeSystem>? source, string value)
    {
        var ss = source?.SelectMany(v => v.Values ?? []);
        var returnValue = ss?.FirstOrDefault(v => v.Value == value);

        var systemForValue = source?.FirstOrDefault(systems => (systems.Values?.Any(val => val.Value?.Equals(value, StringComparison.OrdinalIgnoreCase) == true)) == true);

        if (systemForValue == null || string.IsNullOrWhiteSpace(systemForValue?.SystemOid) || string.IsNullOrWhiteSpace(returnValue?.Value))
        {
            return null;
        }

        return new(systemForValue.SystemOid, returnValue.Value);
    }

    /// <summary>
    /// Get a certain value, and its associated system, based on the name. If the value is not found, returns null.
    /// </summary>
    public static string? GetByName(this IEnumerable<ComprehensiveCodeSystem>? source, string name)
    {
        var ss = source?.SelectMany(v => v.Values ?? []);
        return ss?.FirstOrDefault(v => v.Name == name)?.Value;
    }

    /// <summary>
    /// Get a certain value, and its associated system, based on the name. If the value is not found, returns null.
    /// </summary>
    public static string? GetByValue(this IEnumerable<ComprehensiveCodeSystem>? source, string value)
    {
        var ss = source?.SelectMany(v => v.Values ?? []);
        return ss?.FirstOrDefault(v => v.Value == value)?.Value;
    }

    /// <summary>
    /// Get a certain value, and its associated system. If the value is not found, returns null.
    /// </summary>
    public static KeyValuePair<string, string>? GetByValueSystemUrl(this IEnumerable<ComprehensiveCodeSystem> source, string value)
    {
        var returnValue = source?.SelectMany(v => v.Values ?? []).FirstOrDefault(v => v.Value == value);

        var systemForValue = source?.FirstOrDefault(systems => (systems.Values?.Any(val => val.Value?.Equals(value, StringComparison.OrdinalIgnoreCase) == true)) == true);

        if (systemForValue != null && string.IsNullOrWhiteSpace(systemForValue?.SystemUrl) && string.IsNullOrWhiteSpace(returnValue?.Value))
        {
            return null;
        }

        return new(systemForValue!.SystemUrl!, returnValue!.Value!);
    }

    public static ValueTuple<string, string>? AsTuple(this KeyValuePair<string, string>? source)
    {
        if (source.HasValue)
        {
            return (source.Value.Key, source.Value.Value);
        }
        return null;
    }

    public static CodedValue[]? AsCodedValue(this ValueTuple<string, string>[]? source)
    {
        return source?.Select(s => new CodedValue(s.Item1, s.Item2)).ToArrayOrNull();
    }
}
