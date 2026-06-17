using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.WebService.Services.Policy;

public class PolicyRepositoryService
{
    private readonly ILogger<PolicyRepositoryService> _logger;
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;

    public PolicyRepositoryService(PolicyRepositoryWrapper policyRepositoryWrapper, ILogger<PolicyRepositoryService> logger)
    {
        _policyRepositoryWrapper = policyRepositoryWrapper;
        _logger = logger;
    }

    public PolicySet GetPoliciesAsPolicySetDto()
    {
        return _policyRepositoryWrapper.GetPoliciesAsPolicySet();
    }

    public AbacPolicy? GetSinglePolicy(string? id)
    {
        return _policyRepositoryWrapper.GetPolicy(id);
    }

    public bool AddPolicy(AbacPolicy? policyDto)
    {
        return _policyRepositoryWrapper.AddPolicy(policyDto);
    }

    public bool DeletePolicy(string? id)
    {
        return _policyRepositoryWrapper.DeletePolicy(id);
    }
    
    public bool UpdatePolicy(AbacPolicy abacPolicy, string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        return _policyRepositoryWrapper.UpdatePolicy(abacPolicy, id);
    }

    public bool PartiallyUpdatePolicy(AbacPolicy abacPolicy, string? id, bool append)
    {
        return _policyRepositoryWrapper.PartiallyUpdatePolicy(abacPolicy, id, append);
    }

    public bool DeleteAllPolicies()
    {
        return _policyRepositoryWrapper.DeleteAllPolicies();
    }
}
