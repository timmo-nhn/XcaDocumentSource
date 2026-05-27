using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.Commons.Models.Custom;

public class AdditionalParameters
{
    public AdditionalParameters() { }
    
    public AdditionalParameters(string method, string identifier, AccessControlResponse accessControlResponse, string? urlPath = null)
    {
        HttpMethod = method;
        TraceIdentifier = identifier;
        UrlPath = urlPath;
        AccessControlResponse = accessControlResponse;
    }
    
    public AccessControlResponse AccessControlResponse { get; set; }
    public string? UrlPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? TraceIdentifier { get; set; }
}