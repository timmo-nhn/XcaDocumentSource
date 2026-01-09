using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.Tests.FakesAndDoubles;

public sealed class InMemoryPolicyRepository : IPolicyRepository
{
    private readonly PolicySetDto _policySet = new()
    {
        CombiningAlgorithm = Constants.Xacml.CombiningAlgorithms.V20_PolicyCombining_DenyOverrides,
        Policies = new List<PolicyDto>()
    };

    public PolicySetDto CurrentPolicySet => _policySet;

    public string GetPolicyRepositoryPath()
    {
        return string.Empty;
    }

    public PolicySetDto GetAllPolicies()
    {
        return _policySet;
    }

    public bool AddPolicy(PolicyDto? policyDto)
    {
        if (policyDto == null || string.IsNullOrWhiteSpace(policyDto.Id))
            return false;

        if (_policySet.Policies!.Any(p => p.Id == policyDto.Id))
            return false;

        _policySet.Policies.Add(policyDto);
        return true;
    }

    public bool UpdatePolicy(PolicyDto? policyDto, string? policyId = null)
    {
        if (policyDto == null)
            return false;

        var id = policyId ?? policyDto.Id;
        if (string.IsNullOrWhiteSpace(id))
            return false;

        var existing = _policySet.Policies!.FirstOrDefault(p => p.Id == id);
        if (existing == null)
            return false;

        // Replace policy (simplest + safest)
        _policySet.Policies!.Remove(existing);
        _policySet.Policies!.Add(policyDto);

        return true;
    }

    public bool DeletePolicy(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        var existing = _policySet.Policies!.FirstOrDefault(p => p.Id == id);
        if (existing == null)
            return false;

        _policySet.Policies!.Remove(existing);
        return true;
    }

    public bool DeleteAllPolicies()
    {
        _policySet.Policies!.Clear();
        return true;
    }
}