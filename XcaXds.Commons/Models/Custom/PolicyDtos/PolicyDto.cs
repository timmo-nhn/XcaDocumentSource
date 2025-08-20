using Hl7.Fhir.Validation;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Models.Custom.PolicyDtos;

public class PolicyDto
{
    public string? Id { get; set; }
    public List<PolicyMatch> Subjects { get; set; } 
    public List<PolicyMatch> Role { get; set; }
    public List<PolicyMatch> Organization { get; set; }
    public List<PolicyMatch> Resources { get; set; }
    public List<XacmlPolicyAction> Action { get; set; }
}