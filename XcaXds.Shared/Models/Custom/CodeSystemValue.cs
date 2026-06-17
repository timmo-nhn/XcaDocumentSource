namespace XcaXds.Shared.Models.Custom;

public class CodeSystemValue
{
    public CodeSystemValue() { }

    public CodeSystemValue(string? value)
    {
        Value = value;
    }

    public CodeSystemValue(string? value, string? name)
    {
        Value = value;
        Name = name;
    }

    public string? Value { get; set; }
    public string? Name { get; set; }
}