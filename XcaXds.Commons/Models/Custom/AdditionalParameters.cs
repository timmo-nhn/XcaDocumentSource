namespace XcaXds.Commons.Models.Custom;

public class AdditionalParameters
{
    public AdditionalParameters() { }
    public AdditionalParameters(string method, string identifier)
    {
        HttpMethod = method;
        TraceIdentifier = identifier;
    }

    public string? HttpMethod { get; set; }
    public string? TraceIdentifier { get; set; }
}