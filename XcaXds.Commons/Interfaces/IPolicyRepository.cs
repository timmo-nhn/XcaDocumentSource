using Abc.Xacml.Policy;
using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.Commons.Interfaces;

public interface IPolicyRepository
{
    public PolicySetDto GetAllPolicies();
    public bool AddPolicy(PolicyDto? policyDto);
    public bool DeletePolicy(string? id);
    public bool UpdatePolicy(PolicyDto? policyDto, string? policyId = null);
}
