using Hl7.Fhir.Model;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;
using XcaXds.Shared;
using XcaXds.Shared.Enums;
using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.WebService.Services.PolicyEnforcementPoint.Policy;

public class PolicyDecisionPointService
{
    private Dictionary<string, CompiledPolicy> _compiledPolicies = new();
    private readonly PolicyRepositoryWrapper _policyRepositoryWrapper;
    private readonly ManualResetEventSlim _policiesReady = new(false);

    public PolicyDecisionPointService(PolicyRepositoryWrapper policyRepositoryWrapper)
    {
        _policyRepositoryWrapper = policyRepositoryWrapper;
        ReloadPolicies();
    }

    private void ReloadPolicies()
    {
        _policiesReady.Reset();
        var compiled = CompilePolicies() ?? new Dictionary<string, CompiledPolicy>();
        _policiesReady.Set();
        Interlocked.Exchange(ref _compiledPolicies, compiled);
    }

    private Dictionary<string, CompiledPolicy> CompilePolicies()
    {
        return _policyRepositoryWrapper.GetPoliciesAsPolicySet().Policies?.ToDictionary(p => p.Id, CompiledPolicy.CompilePolicy);
    }

    public AccessControlResponse Evaluate(AbacRequest request)
    {
        ReloadPolicies();

        ArgumentNullException.ThrowIfNull(request);

        if (_compiledPolicies is null || _compiledPolicies.Count == 0)
            throw new InvalidOperationException("Policy engine not initialized");

        var attributes = request.Attributes;

        attributes.TryGetValue(Constants.Urn.Custom.AppliesTo, out var appliesToValues);
        attributes.TryGetValue(Constants.Xacml.Attribute.ActionId, out var actionValues);

        var diagnostics = new List<PolicyEvaluationDiagnostics>();
        var anyApplicable = false;

        foreach (var policy in _compiledPolicies.Values)
        {
            if (!PolicyAppliesToRequest(policy, appliesToValues, actionValues))
                continue;

            anyApplicable = true;

            var result = policy.Evaluate(attributes);

            diagnostics.Add(new PolicyEvaluationDiagnostics()
            {
                Id = result.PolicyId,
                Decision = result.Decision,
                FailedConditions  = result.Conditions.Where(p => !p.Matches).ToList(),
                MatchedConditions =  result.Conditions.Where(p => p.Matches).ToList(),
            });

            if (result.Decision == Decision.Deny)
            {
                return new AccessControlResponse()
                {
                    PolicyId = result.PolicyId,
                    Decision = Decision.Deny,
                    Reason = $"Denied by policy '{result.PolicyId}'",
                    Diagnostics = diagnostics
                };
            }
        }

        if (appliesToValues?.Count > 0)
        {
            // diagnostics.Add();
        }
        
        
        var permitPolicy = diagnostics.FirstOrDefault(d =>
            d.Decision == Decision.Permit);

        if (permitPolicy != null)
        {
            return new AccessControlResponse
            {
                PolicyId = permitPolicy.Id,
                Decision = Decision.Permit,
                Reason = $"Permitted by policy '{permitPolicy.Id}'",
                Diagnostics = diagnostics
            };
        }

        return new AccessControlResponse
        {
            Decision = Decision.NotApplicable,
            Reason = anyApplicable
                ? "Applicable policies evaluated but none permitted access."
                : "No applicable policy matched.",
            Diagnostics = diagnostics
        };
    }

    private bool PolicyAppliesToRequest(CompiledPolicy policy, List<string>? appliesToValues, List<string>? actionValues)
    {
        if (policy.AppliesTo.Count > 0 &&
            (appliesToValues == null ||
             !policy.AppliesTo.Any(p => appliesToValues.Contains(p.ToString()))))
        {
            return false;
        }

        if (policy.Actions.Count > 0 &&
            (actionValues == null ||
             !policy.Actions.Any(a => actionValues.Contains(a.ToString()))))
        {
            return false;
        }

        return true;
    }
}

public class CompiledPolicy
{
    public string Id { get; init; }
    public string Effect { get; init; }
    public List<AppliesTo> AppliesTo { get; init; } = [];
    public List<string> Actions { get; init; } = [];
    public Func<Dictionary<string, List<string>>, EvaluatedPolicy> Evaluate { get; init; }

    public static CompiledPolicy CompilePolicy(AbacPolicy policy)
    {
        var compiledRuleGroups = policy.Rules?.Select(CompileRuleGroup).ToList();

        return new CompiledPolicy
        {
            Id = policy.Id,
            Effect = policy.Effect,
            AppliesTo = policy.AppliesTo ?? [],
            Actions = policy.Actions ?? [],

            Evaluate = attributes => EvaluatePolicy(compiledRuleGroups, policy, attributes)
        };
    }

    private static EvaluatedPolicy EvaluatePolicy(
        List<Func<Dictionary<string, List<string>>, EvaluatedCondition>>? compiledRuleGroups,
        AbacPolicy policy,
        Dictionary<string, List<string>> attributes)
    {
        var result = new EvaluatedPolicy
        {
            PolicyId = policy.Id
        };

        var anyConditionMatched = false;

        foreach (var ruleGroup in compiledRuleGroups ?? [])
        {
            var groupResult = ruleGroup(attributes);
            groupResult.Diagnostics.ForEach(gr => gr.RelatedPolicyId = policy.Id);
            
            result.Conditions.AddRange(groupResult.Diagnostics);

            // Partial applicability
            if (groupResult.Diagnostics.Any(d => d.Matches))
            {
                anyConditionMatched = true;
            }

            // Full group match
            if (groupResult.Matches)
            {
                result.Decision =
                    policy.Effect == "Permit"
                        ? Decision.Permit
                        : Decision.Deny;

                result.Reason = "At least one rule group matched.";

                return result;
            }
        }

        // Partial group match for Permit (Excplicit Deny-policies should only deny if the whole group result matches
        if (anyConditionMatched && policy.Effect == "Permit")
        {
            result.Decision = Decision.Deny;
            result.Reason = "Policy was applicable but one or more conditions failed.";

            return result;
        }
        
        

        // Policy never applied
        result.Decision = Decision.NotApplicable;
        result.Reason = "No rule groups matched.";

        return result;
    }
    
    public static Func<Dictionary<string, List<string>>, EvaluatedCondition> CompileRuleGroup(AbacRuleGroup group)
    {
        var compiled = group.Conditions.Select(CompileCondition).ToList();

        return attributes => EvaluateCondition(compiled, attributes);
    }

    private static EvaluatedCondition EvaluateCondition(List<Func<Dictionary<string, List<string>>, ConditionResult>> compiled, Dictionary<string, List<string>> attributes)
    {
        var results = new List<ConditionResult>();
        bool allMatch = true;

        foreach (var cond in compiled)
        {
            var result = cond(attributes);
            results.Add(result);

            if (!result.Matches)
                allMatch = false;
        }

        return new(allMatch, results);
    }

    private static Func<Dictionary<string, List<string>>, ConditionResult> CompileCondition(AbacCondition condition)
    {
        return attributes => EvaluateCondition(condition, attributes);
    }

    private static ConditionResult EvaluateCondition(AbacCondition condition, Dictionary<string, List<string>> attributes)
    {
        if (!attributes.TryGetValue(condition.AttributeId, out var values))
        {
            return new(condition.AttributeId, false);
        }
            
        var valuesParts = condition.Value?.Split(";");

        if (condition.CompareAttributes == true)
        {
            valuesParts = valuesParts?.SelectMany(att => attributes.TryGetValue(att ?? "", out var value) ? value : null).ToArray();
        }

        return condition.CompareRule switch
        {
            AttributeCompareRule.Equals =>
                new(condition.AttributeId, valuesParts?.Any(val => values.Contains(val)) == true),

            AttributeCompareRule.NotEquals =>
                new(condition.AttributeId, valuesParts?.Any(val => !values.Contains(val)) == false),

            _ => throw new NotSupportedException(condition.CompareRule.ToString())
        };
    }
}