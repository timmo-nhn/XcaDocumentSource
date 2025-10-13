using Abc.Xacml.Context;
using Abc.Xacml.Runtime;
using Microsoft.Extensions.Logging;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Services;

namespace XcaXds.Source.Source;

public class PolicyRepositoryWrapper
{
    private PolicySetDto _policySetPractitioner = new();
    private PolicySetDto _policySetCitizen = new();

    internal PolicySetDto policySetPractitioner
    {
        get => _policySetPractitioner;
        set
        {
            _policySetPractitioner = value;
            RefreshEvaluationEngine();
        }
    }

    internal PolicySetDto policySetCitizen
    {
        get => _policySetCitizen;
        set
        {
            _policySetCitizen = value;
            RefreshEvaluationEngine();
        }
    }

    internal EvaluationEngine _evaluationEnginePractitioner = null!;
    internal EvaluationEngine _evaluationEngineCitizen = null!;

    private readonly IPolicyRepository _policyRepository;
    private readonly ILogger<PolicyRepositoryWrapper> _logger;

    public PolicyRepositoryWrapper(IPolicyRepository policyRepository, ILogger<PolicyRepositoryWrapper> logger)
    {
        _logger = logger;
        _policyRepository = policyRepository;
        policySetPractitioner = _policyRepository.GetAllPolicies();
    }

    // For use in unit tests
    public PolicyRepositoryWrapper(FileBasedPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
        policySetPractitioner = _policyRepository.GetAllPolicies();
    }

    private void RefreshEvaluationEngine()
    {
        _evaluationEnginePractitioner = new EvaluationEngine(
            PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(policySetPractitioner)
        );

        _evaluationEngineCitizen = new EvaluationEngine(
            PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(policySetCitizen)
        );
    }

    public PolicySetDto GetPoliciesAsPolicySet()
    {
        return policySetPractitioner;
    }

    public PolicyDto? GetPolicy(string? id)
    {
        var practitionerPolicies = policySetPractitioner.Policies?.FirstOrDefault(pol => pol.Id == id);
        var citizenPolicies = policySetCitizen.Policies?.FirstOrDefault(pol => pol.Id == id);
        return citizenPolicies ?? practitionerPolicies;
    }

    public bool AddPolicy(PolicyDto? policyDto)
    {
        if (GetPolicy(policyDto?.Id) != null || policyDto == null)
        {
            return false;
        }
        switch (policyDto.AppliesTo)
        {
            case Issuer.Helsenorge:
                policySetCitizen.Policies ??= new();
                policySetCitizen.Policies.Add(policyDto);
                break;

            case Issuer.HelseId:
                policySetPractitioner.Policies ??= new();
                policySetPractitioner.Policies.Add(policyDto);
                break;

            default:
                break;
        }
        RefreshEvaluationEngine();
        return _policyRepository.AddPolicy(policyDto);
    }

    public bool UpdatePolicy(PolicyDto policyDto, string id)
    {
        switch (policyDto.AppliesTo)
        {
            case Issuer.Helsenorge:
                if (policySetCitizen.Policies == null) return false;

                id ??= policyDto.Id;

                var idx = policySetCitizen.Policies.FindIndex(p => p.Id == policyDto.Id);
                if (idx < 0) return false;

                policySetCitizen.Policies[idx] = policyDto;
                break;

            case Issuer.HelseId:
                if (policySetPractitioner.Policies == null) return false;

                id ??= policyDto.Id;

                var idxPrac = policySetPractitioner.Policies.FindIndex(p => p.Id == policyDto.Id);
                if (idxPrac < 0) return false;

                policySetCitizen.Policies[idxPrac] = policyDto;
                break;

            default:
                break;
        }
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
        if (_policySetPractitioner.Policies?.Count == 0)
        {
            _logger.LogWarning("No policies are set up. XcaDocumentSource will deny all requests!");
        }

        var xmlDocument = new XmlDocument();
        return _evaluationEnginePractitioner.Evaluate(xacmlContextRequest, xmlDocument);
    }
}
