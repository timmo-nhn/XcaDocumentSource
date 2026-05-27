using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.Tests.FakesAndDoubles;

public sealed class InMemoryPolicyRepository : IPolicyRepository
{
    private readonly PolicySet _policySet = new()
    {
        Policies = new List<AbacPolicy>()
    };

    public PolicySet CurrentPolicySet => _policySet;

    public string GetPolicyRepositoryPath()
    {
        return string.Empty;
    }

    public PolicySet GetAllPolicies()
    {
        return _policySet;
    }

    public bool AddPolicy(AbacPolicy? policyDto)
    {
        if (policyDto == null || string.IsNullOrWhiteSpace(policyDto.Id))
            return false;

        if (_policySet.Policies!.Any(p => p.Id == policyDto.Id))
            return false;

        _policySet.Policies ??= new();
        _policySet.Policies.Add(policyDto);
        return true;
    }

    public bool UpdatePolicy(AbacPolicy? policyDto, string? policyId = null)
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