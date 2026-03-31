namespace XcaXds.Commons.Models.Custom;

public class AdditionalParameters
{
    public AdditionalParameters() { }
    public AdditionalParameters(string method, string identifier, string? urlPath = null)
    {
        HttpMethod = method;
        TraceIdentifier = identifier;
        UrlPath = urlPath;
    }

    public string? UrlPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? TraceIdentifier { get; set; }
}