using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using XcaXds.BusinessLogic.Models.Custom;
using XcaXds.BusinessLogic.Models.Custom.BusinessLogic;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.BusinessLogic.BusinessLogic;

/// <summary>
/// Filters a document list based on more granular and business-oriented parameters than what PEP performs. Allows for partial filtering of the document list
/// </summary>
public class BusinessLogicFiltererService
{
    private readonly ILogger<BusinessLogicFiltererService> _logger;
    private readonly BusinessLogicFiltersService _businessLogicFiltersService;

    public BusinessLogicFiltererService(ILogger<BusinessLogicFiltererService> logger, BusinessLogicFiltersService businessLogicFiltersService)
    {
        _logger = logger;
        _businessLogicFiltersService = businessLogicFiltersService;
        BusinessLogicRules = _businessLogicFiltersService.AllBusinessRules;
    }

    public Dictionary<string, BusinessRule<IdentifiableType>> BusinessLogicRules = null;


    public void AddRule(string key, BusinessRule<IdentifiableType> rule)
    {
        BusinessLogicRules.Add(key, rule);
    }

    public void RemoveRule(string key)
    {
        BusinessLogicRules.Remove(key);
    }

    private static readonly ConcurrentDictionary<LambdaExpression, Delegate> _compiled = new();

    private static Func<BusinessLogicParameters, bool> CompileCached(Expression<Func<BusinessLogicParameters, bool>> expr)
    {
        return (Func<BusinessLogicParameters, bool>)_compiled.GetOrAdd(expr, e => e.Compile());
    }

    public IEnumerable<IdentifiableType> FilterRegistryObjectListBasedOnBusinessLogic(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters? businessLogic, out Dictionary<string, int> results)
    {
        results = new Dictionary<string, int>();

        if (businessLogic == null) return registryObjects;

        var current = registryObjects;

        var rulesApplied = new List<BusinessLogicResult<IdentifiableType>>();

        var resultCounts = new Dictionary<string, int>();

        foreach (var businessRule in BusinessLogicRules)
        {
            var result = ExecuteRule(businessRule, current ?? [], businessLogic);

            if (result.RuleApplied)
            {
                rulesApplied.Add(result);
                current = result.RegistryObjects;
                resultCounts.Add(result.Name ?? "Unknown", (current != null && current.TryGetNonEnumeratedCount(out var count)) ? count : current?.Count() ?? 0);
            }
        }

        // The Business logic rules should cover every normal scenario...
        // If no rules were applicable, we've hit an edge case, so filter out everything!
        if (rulesApplied.Count == 0)
        {
            current = [];
        }

        results = resultCounts;
        return current ?? [];
    }

    public static BusinessLogicResult<T> ExecuteRule<T>(KeyValuePair<string, BusinessRule<T>> rule, IEnumerable<T> objects, BusinessLogicParameters logic)
    {
        var condition = CompileCached(rule.Value.Condition!);

        if (condition(logic))
        {
            var filtered = rule.Value.Filter!.Compile()(objects);
            return new(true, filtered, rule.Key);
        }

        return new(false, objects, rule.Key);
    }
}