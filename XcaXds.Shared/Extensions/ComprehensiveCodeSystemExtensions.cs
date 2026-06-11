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
    public static KeyValuePair<string, string>? GetValueSystemOid(this IEnumerable<ComprehensiveCodeSystem>? source, string value)
    {
        return source?
            .Where(systems => (systems.Values?.Any(val => val.Value?.Equals(value, StringComparison.OrdinalIgnoreCase) == true)) == true)
            .Select(v => new KeyValuePair<string, string>(v.SystemOid, value))
            .FirstOrDefault();

    }

    /// <summary>
    /// Get a certain value, and its associated system. If the value is not found, returns null.
    /// </summary>
    public static KeyValuePair<string, string>? GetValueSystemUrl(this IEnumerable<ComprehensiveCodeSystem> source, string value)
    {
        return source
            .Where(systems => (systems.Values?.Any(val => val.Value?.Equals(value, StringComparison.OrdinalIgnoreCase) == true)) == true)
            .Select(v => new KeyValuePair<string, string>(v.SystemUrl, value))
            .FirstOrDefault();

    }

    public static (string, string) AsTuple(this KeyValuePair<string, string>? source)
    {
        if (source.HasValue)
        {
            return (source.Value.Key, source.Value.Value);
        }
        return (string.Empty, string.Empty);
    }
}
