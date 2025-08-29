﻿using Abc.Xacml.Policy;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;

namespace XcaXds.Source.Services;

public class PolicyRepositoryService
{
    private readonly ILogger<PolicyRepositoryService> _logger;
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;

    public PolicyRepositoryService(PolicyRepositoryWrapper policyRepositoryWrapper, ILogger<PolicyRepositoryService> logger)
    {
        _policyRepositoryWrapper = policyRepositoryWrapper;
        _logger = logger;
    }

    public PolicySetDto GetPoliciesAsPolicySetDto()
    {
        return _policyRepositoryWrapper.GetPoliciesAsPolicySet();
    }

    public PolicyDto? GetSinglePolicy(string? id)
    {
        return _policyRepositoryWrapper.GetPolicy(id);
    }

    public bool AddPolicy(PolicyDto? policyDto)
    {
        return _policyRepositoryWrapper.AddPolicy(policyDto);
    }

    public bool DeletePolicy(string? id)
    {
        return _policyRepositoryWrapper.DeletePolicy(id);
    }

    public XacmlPolicySet? GetPoliciesAsXacmlPolicySet()
    {
        var policySetDto = _policyRepositoryWrapper.GetPoliciesAsPolicySet();
        return PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(policySetDto);
    }

    public bool UpdatePolicy(PolicyDto policyDto, string? id)
    {
        return _policyRepositoryWrapper.UpdatePolicy(policyDto, id);
    }

    public bool PartiallyUpdatePolicy(PolicyDto policyDto, string? id, bool append)
    {
        return _policyRepositoryWrapper.PartiallyUpdatePolicy(policyDto, id, append);
    }
}
