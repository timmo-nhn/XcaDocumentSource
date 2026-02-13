using Abc.Xacml.Context;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.BusinessLogic;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Source.Services;

/// <summary>
/// Filters a document list based on more granular and business-oriented parameters than what PEP performs (XACML). Allows for partial (non-atomic) filtering of the document list
/// </summary>
public static class BusinessLogicFilteringService
{
    public static IEnumerable<IdentifiableType>? FilterRegistryObjectListBasedOnBusinessLogic(this IEnumerable<IdentifiableType>? registryObjects, BusinessLogicParameters? businessLogic)
    {
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

        foreach (var businessRule in businessLogicRules)
        {
            var result = businessRule(current, businessLogic);

            if (result.RuleApplied)
            {
                rulesApplied.Add(result);
                current = result.RegistryObjects;
            }
        }

        return current;
    }


    public static BusinessLogicParameters? MapXacmlRequestToBusinessLogicParameters(XacmlContextRequest? xacmlRequest)
    {
        if (xacmlRequest == null) return null;

        var businessLogic = new BusinessLogicParameters();

        var xacmlAttributes = xacmlRequest.GetAllXacmlContextAttributes();

        businessLogic.Issuer = Enum.Parse<Issuer>(xacmlAttributes.GetXacmlAttributeValuesAsString(Constants.Xacml.CustomAttributes.AppliesTo)?.FirstOrDefault() ?? Issuer.Unknown.ToString());
        businessLogic.QueriedSubject = xacmlAttributes.GetXacmlAttributeValuesAsCodedValue(Constants.Xacml.CustomAttributes.AdhocQueryPatientIdentifier);
        businessLogic.QueriedSubjectAge = GetAgeFromPatientId(businessLogic.QueriedSubject?.Code);
        businessLogic.Purpose = xacmlAttributes.GetXacmlAttributeValuesAsCodedValue(Constants.Saml.Attribute.PurposeOfUse) ?? xacmlAttributes.GetXacmlAttributeValuesAsCodedValue(Constants.Saml.Attribute.PurposeOfUse_Helsenorge);
        businessLogic.Resource = xacmlAttributes.GetXacmlAttributeValuesAsCodedValue(Constants.Saml.Attribute.ResourceId20);
        businessLogic.ResourceAge = GetAgeFromPatientId(businessLogic.Resource?.Code);
        businessLogic.Subject = xacmlAttributes.GetXacmlAttributeValuesAsCodedValue(Constants.Saml.Attribute.ProviderIdentifier);
        businessLogic.SubjectOrganization = xacmlAttributes.GetXacmlAttributeValuesAsCodedValue(Constants.Saml.Attribute.OrganizationId);
        businessLogic.SubjectAge = GetAgeFromPatientId(businessLogic.Subject?.Code);
        businessLogic.Role = xacmlAttributes.GetXacmlAttributeValuesAsCodedValue(Constants.Saml.Attribute.Role) ?? xacmlAttributes.GetXacmlAttributeValuesAsCodedValue(Constants.Saml.Attribute.SubjectRole20);
        businessLogic.Acp = xacmlAttributes.GetXacmlAttributeValuesAsString(Constants.Saml.Attribute.XuaAcp)?.FirstOrDefault();
        businessLogic.Bppc = xacmlAttributes.GetXacmlAttributeValuesAsString(Constants.Saml.Attribute.BppcDocId)?.FirstOrDefault();

        return businessLogic;
    }

    public static int GetAgeFromPatientId(string? patientId)
    {
        if (string.IsNullOrWhiteSpace(patientId) || patientId.Length != 11) return 0;

        var patientNin = Hl7FhirExtensions.ParseNorwegianNinToDateTime(patientId);

        var year = DateTime.Today.Year - (patientNin.HasValue ? patientNin.Value.Year : 0);

        return year;
    }
}