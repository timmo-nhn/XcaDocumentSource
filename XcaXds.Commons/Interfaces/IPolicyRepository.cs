using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.Commons.Interfaces;

public interface IPolicyRepository
{
    public string GetPolicyRepositoryPath();
    public PolicySet GetAllPolicies();
    public bool AddPolicy(AbacPolicy? policyDto);
    public bool DeletePolicy(string? id);
    public bool DeleteAllPolicies();
    public bool UpdatePolicy(AbacPolicy? policyDto, string? policyId = null);
}
