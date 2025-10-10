using System.Xml;
using Abc.Xacml.Context;
using Abc.Xacml.Runtime;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Services;

namespace XcaXds.Source.Source;

public class PolicyRepositoryWrapper
{
    private PolicySetDto _policySet = new();
    internal PolicySetDto policySet
    {
        get => _policySet;
        set
        {
            _policySet = value;
            RefreshEvaluationEngine();
        }
    }

    internal EvaluationEngine _evaluationEngine = null!;

    private readonly IPolicyRepository _policyRepository;
    private readonly ILogger<PolicyRepositoryWrapper> _logger;

    public PolicyRepositoryWrapper(IPolicyRepository policyRepository, ILogger<PolicyRepositoryWrapper> logger)
    {
        _logger = logger;
        _policyRepository = policyRepository;
        policySet = _policyRepository.GetAllPolicies();
    }

    // For use in unit tests
    public PolicyRepositoryWrapper(FileBasedPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
        policySet = _policyRepository.GetAllPolicies();
    }

    private void RefreshEvaluationEngine()
    {
        _evaluationEngine = new EvaluationEngine(
            PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(policySet)
        );
    }

    public PolicySetDto GetPoliciesAsPolicySet()
    {
        return policySet;
    }

    public PolicyDto? GetPolicy(string? id)
    {
        return policySet.Policies?.FirstOrDefault(pol => pol.Id == id);
    }

    public bool AddPolicy(PolicyDto? policyDto)
    {
        if (GetPolicy(policyDto?.Id) != null || policyDto == null)
        {
            return false;
        }

        policySet.Policies ??= new();
        policySet.Policies.Add(policyDto);
        RefreshEvaluationEngine();
        return _policyRepository.AddPolicy(policyDto);
    }

    public bool UpdatePolicy(PolicyDto policyDto, string id)
    {
        if (policySet.Policies == null) return false;

        id ??= policyDto.Id;

        var idx = policySet.Policies.FindIndex(p => p.Id == policyDto.Id);
        if (idx < 0) return false;

        policySet.Policies[idx] = policyDto;
        RefreshEvaluationEngine();

        return _policyRepository.UpdatePolicy(policyDto, id);
    }

    public bool PartiallyUpdatePolicy(PolicyDto patch, string? id, bool append)
    {
        if (policySet.Policies == null) return false;

        var policy = policySet.Policies.FirstOrDefault(p => p.Id == (id ?? patch.Id));
        if (policy == null) return false;

        policy.MergeWith(patch, append);

        RefreshEvaluationEngine();

        return _policyRepository.UpdatePolicy(policy, policy.Id);
    }

    public bool DeletePolicy(string? id)
    {
        var deleteResult = _policyRepository.DeletePolicy(id);
        if (!deleteResult) return false;

        policySet = _policyRepository.GetAllPolicies(); // triggers engine refresh automatically
        return true;
    }

    public XacmlContextResponse EvaluateRequest_V20(XacmlContextRequest? xacmlContextRequest)
    {
        if (_policySet.Policies?.Count == 0)
        {
            _logger.LogWarning("No policies are set up. XcaDocumentSource will deny all requests!");
        }

        var xmlDocument = new XmlDocument();
        return _evaluationEngine.Evaluate(xacmlContextRequest, xmlDocument);
    }
}
