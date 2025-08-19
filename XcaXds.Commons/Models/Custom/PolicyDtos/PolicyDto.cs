using Hl7.Fhir.Validation;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Models.Custom.PolicyDtos;

public class PolicyDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public PolicyMatch<CodedValue>? Subject { get; set; } 
    public PolicyMatch<CodedValue>? Role { get; set; }
    public PolicyMatch<CodedValue>? Organization { get; set; }
    public PolicyMatch<CodedValue>? ResourceId { get; set; }
    public XacmlPolicyAction[]? Action { get; set; }
}