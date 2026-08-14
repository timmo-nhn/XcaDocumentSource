using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint.Policy;

public class PolicyRepositoryWrapper
{
    private PolicySet _policySet;

    private readonly IPolicyRepository _policyRepository;
    private readonly ILogger<PolicyRepositoryWrapper> _logger;

    public PolicyRepositoryWrapper(IPolicyRepository policyRepository, ILogger<PolicyRepositoryWrapper> logger)
    {
        _logger = logger;
        _policyRepository = policyRepository;

        _policySet = _policyRepository.GetAllPolicies();
    }

    public PolicySet GetPoliciesAsPolicySet()
    {
        return _policySet;
    }

    public AbacPolicy? GetPolicy(string? id)
    {
        return _policySet.Policies?.FirstOrDefault(pol => pol.Id == id);
    }

    public bool AddPolicy(AbacPolicy? policyDto)
    {
        if (GetPolicy(policyDto?.Id) != null || policyDto == null)
        {
            return false;
        }

        var addPolicy = _policyRepository.AddPolicy(policyDto);

        if (!addPolicy) return false;

        _policySet = _policyRepository.GetAllPolicies();

        return true;
    }

    public bool UpdatePolicy(AbacPolicy abacPolicy, string id)
    {
        if (_policySet.Policies == null || string.IsNullOrWhiteSpace(abacPolicy.Id)) return false;

        id ??= abacPolicy.Id;

        var idx = _policySet.Policies.FindIndex(p => p.Id == abacPolicy.Id);
        if (idx < 0) return false;

        var updatePolicy = _policyRepository.UpdatePolicy(abacPolicy, id);

        if (!updatePolicy) return false;

        _policySet.Policies[idx] = abacPolicy;

        return true;
    }

    public bool PartiallyUpdatePolicy(AbacPolicy patch, string? id, bool append)
    {
        if (_policySet.Policies == null) return false;

        var policy = _policySet.Policies.FirstOrDefault(p => p.Id == (id ?? patch.Id));
        if (policy == null) return false;

        // policy.MergeWith(patch, append);

        var patchPolicy = _policyRepository.UpdatePolicy(policy, policy.Id);

        if (!patchPolicy) return false;

        _policySet = _policyRepository.GetAllPolicies();

        return true;
    }

    public bool DeletePolicy(string? id)
    {
        var deleteResult = _policyRepository.DeletePolicy(id);
        if (!deleteResult) return false;

        _policySet = _policyRepository.GetAllPolicies();

        return true;
    }

    public bool DeleteAllPolicies()
    {
        var deleteAllResult = _policyRepository.DeleteAllPolicies();
        _policySet = _policyRepository.GetAllPolicies();

        return deleteAllResult;
    }
}