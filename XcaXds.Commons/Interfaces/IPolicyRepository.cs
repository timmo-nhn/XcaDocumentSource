using Abc.Xacml.Policy;
using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.Commons.Interfaces;

public interface IPolicyRepository
{
    public PolicySetDto GetAllPolicies();
    public bool AddPolicy(PolicyDto xacmlPolicy);
    public bool DeletePolicy(string? id);
    public bool UpdatePolicy(PolicyDto xacmlPolicy, string? policyId = null);
}
