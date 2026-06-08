using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.Commons.Models.Custom;

public class AdditionalParameters
{
    public AdditionalParameters() { }

    public AdditionalParameters(string method, string identifier, AccessControlResponse? accessControlResponse, Dictionary<string,int>? appliedBusinessLogic, string? urlPath = null, IEnumerable<DocumentEntryDto>? deletedRegistryObjects = null)
    {
        HttpMethod = method;
        TraceIdentifier = identifier;
        UrlPath = urlPath;
        AccessControlResponse = accessControlResponse;
        DeletedRegistryObjects = deletedRegistryObjects?.ToArray();
        AppliedBusinessLogic = appliedBusinessLogic;
    }

    public DocumentEntryDto[]? DeletedRegistryObjects { get; set; }
    public Dictionary<string, int>? AppliedBusinessLogic { get; set; }
    public AccessControlResponse? AccessControlResponse { get; set; }
    public string? UrlPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? TraceIdentifier { get; set; }
}