using Hl7.Fhir.Model;
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
public static class BusinessLogicFilterer
{
    public static readonly List<BusinessRule<IdentifiableType>> BusinessLogicRules = new List<BusinessRule<IdentifiableType>>()
    {
        BusinessLogicFiltersService.CitizenShouldSeeOwnDocumentReferences,
        BusinessLogicFiltersService.CitizenBetween12And16ShouldNotSeeDocumentReferences,
        BusinessLogicFiltersService.CitizenBetween16And18ShouldAccesPartsOfDocumentReferences,
        BusinessLogicFiltersService.CitizenShouldSeeChildrenBelow12DocumentReferences,
        BusinessLogicFiltersService.CitizenShouldSeePowerOfAttorneyDocumentReferences,
        BusinessLogicFiltersService.CitizenShouldNotSeeNonPowerOfAttorneyDocumentReferences,
        BusinessLogicFiltersService.CitizenShouldNotAccessDocumentsForPatientOver12,

        BusinessLogicFiltersService.HealthcarePersonellShouldSeeOwnDocumentReferences,
        BusinessLogicFiltersService.HealthcarePersonellShouldSeeEmergencyRelatedPatientDocumentReferences,
        BusinessLogicFiltersService.HealthcarePersonellWithMissingAttributesShouldNotSeeDocumentReferences,

        //BusinessLogicFilters.HealthcarePersonellKjernejournalForskriften,

        BusinessLogicFiltersService.HealthcarePersonellShouldSeeRelatedPatientDocumentReferences,
    };

    public static void AddRule(BusinessRule<IdentifiableType> rule)
    {
        BusinessLogicRules.Add(rule);
    }

    public static void RemoveRule(string ruleName)
    {
        BusinessLogicRules.RemoveAll(rul => rul.Name == ruleName);
    }

    private static readonly ConcurrentDictionary<LambdaExpression, Delegate> _compiled = new();

    private static Func<BusinessLogicParameters, bool> CompileCached(Expression<Func<BusinessLogicParameters, bool>> expr)
    {
        return (Func<BusinessLogicParameters, bool>)_compiled.GetOrAdd(expr, e => e.Compile());
    }

    public static IEnumerable<IdentifiableType> FilterRegistryObjectListBasedOnBusinessLogic(this IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters? businessLogic, out Dictionary<string, int> results)
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

    public static BusinessLogicResult<T> ExecuteRule<T>(BusinessRule<T> rule, IEnumerable<T> objects, BusinessLogicParameters logic)
    {
        var condition = CompileCached(rule.Condition!);

        if (condition(logic))
        {
            var filtered = rule.Filter!.Compile()(objects);
            return new(true, filtered, rule.Name);
        }

        return new(false, objects, rule.Name);
    }
}