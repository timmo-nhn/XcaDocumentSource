using Abc.Xacml.Context;
using Abc.Xacml.Runtime;
using System.Xml;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.WebService.Services;

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

    internal EvaluationEngine _evaluationEngine = null!;

    private readonly IPolicyRepository _policyRepository;
    private readonly ILogger<PolicyRepositoryWrapper> _logger;

    public PolicyRepositoryWrapper(IPolicyRepository policyRepository, ILogger<PolicyRepositoryWrapper> logger)
    {
        _logger = logger;
        _policyRepository = policyRepository;
        policySet = _policyRepository.GetAllPolicies();

        _policyRepositoryPath = _policyRepository.GetPolicyRepositoryPath();

        if (string.IsNullOrWhiteSpace(_policyRepositoryPath)) throw new InvalidOperationException("No PolicyRepository Path found!");

        _watcher = new FileSystemWatcher(_policyRepositoryPath)
        {
            NotifyFilter = NotifyFilters.LastWrite
        };
        _watcher.Changed += OnFileChanged;
        _watcher.EnableRaisingEvents = true;

    }

    private void RefreshEvaluationEngine()
    {
        lock (_lock)
        {
            _evaluationEngine = new EvaluationEngine(PolicyDtoTransformer.TransformPolicySetDtoToXacmlVersion20PolicySet(new PolicySetDto()
            {
                CombiningAlgorithm = policySet.CombiningAlgorithm,
                SetId = policySet.SetId,
                Policies = policySet.Policies
            }));
        }
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

        var addPolicy = _policyRepository.AddPolicy(policyDto);

        if (!addPolicy) return false;

        policySet.Policies ??= new();
        policySet.Policies.Add(policyDto);

        RefreshEvaluationEngine();
        return true;
    }

    public bool UpdatePolicy(PolicyDto policyDto, string id)
    {
        if (policySet.Policies == null || string.IsNullOrWhiteSpace(policyDto.Id)) return false;

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
        RefreshEvaluationEngine();
        return true;
    }

    public XacmlContextResponse EvaluateRequest_V20(XacmlContextRequest? xacmlContextRequest)
    {
        if (_policySetPractitioner.Policies?.Count == 0)
        {
            _logger.LogWarning("No policies are set up. Will deny all requests!");
        }

        return _evaluationEngine.Evaluate(xacmlContextRequest, new XmlDocument());
    }

    public bool DeleteAllPolicies()
    {
        var deleteAllResult = _policyRepository.DeleteAllPolicies();
        policySet = _policyRepository.GetAllPolicies();
        RefreshEvaluationEngine();
        return deleteAllResult;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
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
