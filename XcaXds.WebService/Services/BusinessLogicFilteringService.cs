using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.BusinessLogic;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.WebService.Services;

/// <summary>
/// Filters a document list based on more granular and business-oriented parameters than what PEP performs (XACML). Allows for partial filtering of the document list
/// </summary>
public static class BusinessLogicFilteringService
{
    public static IEnumerable<IdentifiableType>? FilterRegistryObjectListBasedOnBusinessLogic(this IEnumerable<IdentifiableType>? registryObjects, BusinessLogicParameters? businessLogic, out Dictionary<string, int> results)
    {
        results = new Dictionary<string, int>();

        if (registryObjects == null || registryObjects.Count() == 0) return registryObjects;
        if (businessLogic == null) return registryObjects;

        var businessLogicRules = new List<BusinessLogicRule>
        {
            BusinessLogicFilters.CitizenShouldSeeOwnDocumentReferences,
            BusinessLogicFilters.CitizenBetween12And16ShouldNotSeeDocumentReferences,
            BusinessLogicFilters.CitizenBetween16And18ShouldAccesPartsOfDocumentReferences,
            BusinessLogicFilters.CitizenShouldSeeChildrenBelow12DocumentReferences,
            BusinessLogicFilters.CitizenShouldSeePowerOfAttorneyDocumentReferences,
            BusinessLogicFilters.CitizenShouldNotSeeNonPowerOfAttorneyDocumentReferences,

            //BusinessLogicFilters.CitizenShouldNotSeeCertainDocumentReferencesForThemself, // Filter out certain document types when returning document list for helsenorge

            BusinessLogicFilters.HealthcarePersonellShouldSeeOwnDocumentReferences,
            BusinessLogicFilters.HealthcarePersonellShouldSeeEmergencyRelatedPatientDocumentReferences,
            BusinessLogicFilters.HealthcarePersonellWithMissingAttributesShouldNotSeeDocumentReferences,

            // Comment out this to try only the one below instead
//#if DEBUG
//            BusinessLogicFilters.HealthcarePersonellFilterOutCertainDocumentReferencesForPatient, // Filter out certain document types when returning document list for kjernejournal
//#else
//            BusinessLogicFilters.HealthcarePersonellShouldSeeRelatedPatientDocumentReferences,
//#endif
        };

        var current = registryObjects;

        var rulesApplied = new List<BusinessLogicResult>();

        var resultCounts = new Dictionary<string, int>();

        foreach (var businessRule in businessLogicRules)
        {
            var result = businessRule(current ?? [], businessLogic);

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
}