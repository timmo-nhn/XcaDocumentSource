namespace XcaXds.Shared.Models.Custom;

public class ComprehensiveCodeSystem
{
    public ComprehensiveCodeSystem() { }

    // For cases where we care about both the system and the values. E.g. for authorization purposes, where we want to check if a certain value is present in a certain system.
    public ComprehensiveCodeSystem(string oid, CodeSystemValue[] values)
    {
        System = oid;
        Values = values;
    }

    // For cases where we only care about the system, and not the values. E.g. for filtering purposes.
    public ComprehensiveCodeSystem(string system)
    {
        System = system;
    }

    public string? System { get; set; }
    public string[]? SystemsAlternate { get; set; }

    public CodeSystemValue[]? Values { get; set; }
}