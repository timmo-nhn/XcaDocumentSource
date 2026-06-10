
using XcaXds.Terminology.Extensions;

namespace XcaXds.Terminology.Models.Custom;

public class ComprehensiveCodeSystem
{
    public ComprehensiveCodeSystem() { }

    // For cases where we care about both the system and the values. E.g. for authorization purposes, where we want to check if a certain value is present in a certain system.
    public ComprehensiveCodeSystem(string oid, CodeSystemValue[] values)
    {
        SystemOid = oid;
        Values = values;
    }

    // For cases where we only care about the system, and not the values. E.g. for filtering purposes.
    public ComprehensiveCodeSystem(string system)
    {
        SystemOid = system;
    }

    public string? SystemOid { get; set; }
    public string? SystemUrl { get; set; }
    public string? Name { get; set; }

    public CodeSystemValue[]? Values { get; set; }
}

public static class ComprehensiveCodeSystemExtensions
{
    public static string[]? SystemOids(this IEnumerable<ComprehensiveCodeSystem> source)
    {
        return [.. source.Select(ccs => ccs.SystemOid)];
    }

    public static string[]? SystemUrls(this IEnumerable<ComprehensiveCodeSystem> source)
    {
        return [.. source.Select(ccs => ccs.SystemUrl)];
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
    public static KeyValuePair<string, string>? GetValueSystemOid(this IEnumerable<ComprehensiveCodeSystem> source, string value)
    {
        return source
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