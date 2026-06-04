using Hl7.Fhir.Model;
using XcaXds.Commons.Extensions;

namespace XcaXds.Commons.Models.Custom;

public class ComprehensiveCodeSystem
{
    // For cases where we care about both the system and the values. E.g. for authorization purposes, where we want to check if a certain value is present in a certain system.
    public ComprehensiveCodeSystem(string oid, string[] values)
    {
        System = oid;
        Values = values;
    }

    // For cases where we only care about the system, and not the values. E.g. for filtering purposes.
    public ComprehensiveCodeSystem(string system)
    {
        System = system;
    }

    public string System { get; set; }

    public string[]? Values { get; set; }
}

public static class ComprehensiveCodeSystemExtensions
{
    public static string[] Systems(this IEnumerable<ComprehensiveCodeSystem> source)
    {
        return source.Select(ccs => ccs.System).ToArray();
    }

    public static string[]? Values(this IEnumerable<ComprehensiveCodeSystem> source, string system)
    {
        return source.Where(sys => sys.System.NoUrn() == system.NoUrn())?.Values()?.ToArray();
    }

    public static string[]? Values(this IEnumerable<ComprehensiveCodeSystem> source)
    {
        var elements = source.SelectMany(v => v.Values ?? []).ToArray();

        return elements.Length > 0 ? elements : null;
    }

    /// <summary>
    /// Get a certain value, and its associated system. If the value is not found, returns null.
    /// </summary>
    public static KeyValuePair<string, string>? GetValue(this IEnumerable<ComprehensiveCodeSystem> source, string value)
    {
        return source.Where(v => v.Values?.Contains(value) ?? false).Select(v => new KeyValuePair<string, string>(v.System, value)).FirstOrDefault();
    }
}