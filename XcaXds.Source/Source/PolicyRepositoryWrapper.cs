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
    private PolicySetDto _policySetPractitioner = new();
    private readonly object _lock = new();
    private readonly FileSystemWatcher _watcher;
    private readonly string _policyRepositoryPath;

    internal PolicySetDto policySet
    {
        get => _policySetPractitioner;
        set
        {
            _policySetPractitioner = value;
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
        policySet = _policyRepository.GetAllPolicies();

        _policyRepositoryPath = _policyRepository.GetPolicyRepositoryPath();

        _watcher = new FileSystemWatcher(_policyRepositoryPath)
        {
            NotifyFilter = NotifyFilters.LastWrite
        };

        _watcher.Changed += OnConfigFileChanged;
        _watcher.EnableRaisingEvents = true;

    }

    // For use in unit tests
    public PolicyRepositoryWrapper(FileBasedPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
        policySet = _policyRepository.GetAllPolicies();
    }

    private void RefreshEvaluationEngine()
    {
        lock (_lock)
        {
            _evaluationEnginePractitioner = new EvaluationEngine(
                PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(new PolicySetDto()
                {
                    CombiningAlgorithm = policySet.CombiningAlgorithm,
                    SetId = policySet.SetId,
                    Policies = policySet.Policies?.Where(pol => pol.AppliesTo == Issuer.HelseId).ToList()
                }));

            _evaluationEngineCitizen = new EvaluationEngine(
                PolicyDtoTransformerService.TransformPolicySetDtoToXacmlVersion20PolicySet(new PolicySetDto()
                {
                    CombiningAlgorithm = policySet.CombiningAlgorithm,
                    SetId = policySet.SetId,
                    Policies = policySet.Policies?.Where(pol => pol.AppliesTo == Issuer.Helsenorge).ToList()
                }));
        }
    }

    public PolicySetDto GetPoliciesAsPolicySet()
    {
        return policySet;
    }

    public PolicySetDto GetPoliciesAsPolicySet(Issuer issuer)
    {
        return issuer switch
        {
            Issuer.HelseId => new PolicySetDto()
            {
                CombiningAlgorithm = policySet.CombiningAlgorithm,
                SetId = policySet.SetId,
                Policies = policySet.Policies?.Where(pol => pol.AppliesTo == Issuer.HelseId).ToList()
            },

            Issuer.Helsenorge => new PolicySetDto()
            {
                CombiningAlgorithm = policySet.CombiningAlgorithm,
                SetId = policySet.SetId,
                Policies = policySet.Policies?.Where(pol => pol.AppliesTo == Issuer.Helsenorge).ToList()
            },

            _ => new PolicySetDto()
            {
                CombiningAlgorithm = policySet.CombiningAlgorithm,
                SetId = policySet.SetId,
                Policies = policySet.Policies
            }
        };
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

        var addPolicy = _policyRepository.AddPolicy(policyDto);

        if (!addPolicy) return false;

        policySet.Policies ??= new();
        policySet.Policies.Add(policyDto);

        RefreshEvaluationEngine();
        return true;
    }

    public bool UpdatePolicy(PolicyDto policyDto, string id)
    {
        if (policySet.Policies == null) return false;

        id ??= policyDto.Id;

        var idx = policySet.Policies.FindIndex(p => p.Id == policyDto.Id);
        if (idx < 0) return false;

        var updatePolicy = _policyRepository.UpdatePolicy(policyDto, id);

        if (!updatePolicy) return false;

        policySet.Policies[idx] = policyDto;

        RefreshEvaluationEngine();

        return true;
    }

    public bool PartiallyUpdatePolicy(PolicyDto patch, string? id, bool append)
    {
        if (policySet.Policies == null) return false;

        var policy = policySet.Policies.FirstOrDefault(p => p.Id == (id ?? patch.Id));
        if (policy == null) return false;

        policy.MergeWith(patch, append);

        var patchPolicy = _policyRepository.UpdatePolicy(policy, policy.Id);

        if (!patchPolicy) return false;

        RefreshEvaluationEngine();

        return true;
    }

    public bool DeletePolicy(string? id)
    {
        var deleteResult = _policyRepository.DeletePolicy(id);
        if (!deleteResult) return false;

        policySet = _policyRepository.GetAllPolicies();
        return true;
    }

    public XacmlContextResponse EvaluateRequest_V20(XacmlContextRequest? xacmlContextRequest, Issuer appliesTo)
    {
        if (_policySetPractitioner.Policies?.Count == 0)
        {
            _logger.LogWarning("No policies are set up. XcaDocumentSource will deny all requests!");
        }
        
        switch (appliesTo)
        {
            case Issuer.Helsenorge:
                return _evaluationEngineCitizen.Evaluate(xacmlContextRequest, new XmlDocument());

            case Issuer.HelseId:
                return _evaluationEnginePractitioner.Evaluate(xacmlContextRequest, new XmlDocument());

            default:
                return _evaluationEnginePractitioner.Evaluate(xacmlContextRequest, new XmlDocument());
        }
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        Task.Delay(500).ContinueWith(_ =>
        {
            try
            {
                policySet = _policyRepository.GetAllPolicies();
                RefreshEvaluationEngine();
                _logger.LogInformation($"{Path.GetFileName(_policyRepositoryPath)} reloaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading policy repository.");
            }
        });
    }
}
