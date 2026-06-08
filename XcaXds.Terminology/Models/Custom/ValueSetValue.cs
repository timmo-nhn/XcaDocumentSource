namespace XcaXds.Terminology.Models.Custom;

public class ValueSetValue
{
    public ValueSetValue() { }

    public ValueSetValue(string? value, string? name)
    {
        Value = value;
        Name = name;
    }

    public string? Value { get; set; }
    public string? Name { get; set; }
}