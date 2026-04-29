namespace XcaXds.Commons.Models.Jwk;

public class Jwk
{
    public Key[]? Keys { get; set; }
}

public class Key
{
    public string? Kty { get; set; }
    public string? E { get; set; }
    public string? Use { get; set; }
    public string? Kid { get; set; }
    public string? N { get; set; }
    public string[]? X5C { get; set; }
}
