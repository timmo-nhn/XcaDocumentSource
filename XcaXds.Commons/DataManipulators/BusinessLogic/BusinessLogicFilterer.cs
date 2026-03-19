using System.Collections.Concurrent;
using System.Linq.Expressions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.BusinessLogic;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.DataManipulators.BusinessLogic;

/// <summary>
/// Filters a document list based on more granular and business-oriented parameters than what PEP performs (XACML). Allows for partial filtering of the document list
/// </summary>
public static class BusinessLogicFilterer
{
    public static List<BusinessRule<IdentifiableType>> BusinessLogicRules = new List<BusinessRule<IdentifiableType>>()
    {
        BusinessLogicFilters.CitizenShouldSeeOwnDocumentReferences,
        BusinessLogicFilters.CitizenBetween12And16ShouldNotSeeDocumentReferences,
        BusinessLogicFilters.CitizenBetween16And18ShouldAccesPartsOfDocumentReferences,
        BusinessLogicFilters.CitizenShouldSeeChildrenBelow12DocumentReferences,
        BusinessLogicFilters.CitizenShouldSeePowerOfAttorneyDocumentReferences,
        BusinessLogicFilters.CitizenShouldNotSeeNonPowerOfAttorneyDocumentReferences,

        BusinessLogicFilters.HealthcarePersonellShouldSeeOwnDocumentReferences,
        BusinessLogicFilters.HealthcarePersonellShouldSeeEmergencyRelatedPatientDocumentReferences,
        BusinessLogicFilters.HealthcarePersonellWithMissingAttributesShouldNotSeeDocumentReferences,

        BusinessLogicFilters.HealthcarePersonellKjernejournalForskriften,

        BusinessLogicFilters.HealthcarePersonellShouldSeeRelatedPatientDocumentReferences,
    };

    private static readonly ConcurrentDictionary<LambdaExpression, Delegate> _compiled = new();

    private static Func<BusinessLogicParameters, bool> CompileCached(Expression<Func<BusinessLogicParameters, bool>> expr)
    {
        return (Func<BusinessLogicParameters, bool>)_compiled.GetOrAdd(expr, e => e.Compile());
    }

    public static IEnumerable<IdentifiableType>? FilterRegistryObjectListBasedOnBusinessLogic(this IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters? businessLogic, out Dictionary<string, int> results)
    {
        results = new Dictionary<string, int>();

        if (registryObjects == null) return null;
        if (businessLogic == null) return null;

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

        results = resultCounts;
        return current;
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