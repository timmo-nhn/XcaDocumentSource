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
    public static void FilterRegistryObjectListBasedOnBusinessLogic(this IEnumerable<IdentifiableType> registryObjects, XacmlContextRequest? xacmlRequest)
    {
        if (xacmlRequest == null) return;

        if (registryObjects == null || registryObjects.Count() == 0) return;

        var businessLogic = MapXacmlRequestToBusinessLogicParameters(xacmlRequest);

        if (businessLogic == null) return;

        bool status;

        BusinessLogicFilters.CitizenShouldSeeOwnDocumentReferences(registryObjects, businessLogic, out status);
        BusinessLogicFilters.CitizenBetween12And16ShouldNotSeeDocumentReferences(registryObjects, businessLogic, out status);
        BusinessLogicFilters.CitizenBetween16And18ShouldAccesPartsOfDocumentReferences(registryObjects, businessLogic, out status);
        BusinessLogicFilters.CitizenShouldSeeChildrenBelow12DocumentReferences(registryObjects, businessLogic, out status);
        BusinessLogicFilters.CitizenShouldSeePowerOfAttorneyDocumentReferences(registryObjects, businessLogic, out status);
        BusinessLogicFilters.CitizenShouldNotSeeNonPowerOfAttorneyDocumentReferences(registryObjects, businessLogic, out status);

        BusinessLogicFilters.HealthcarePersonellShouldSeeOwnDocumentReferences(registryObjects,businessLogic,out status);
        BusinessLogicFilters.HealthcarePersonellShouldSeeRelatedPatientDocumentReferences(registryObjects, businessLogic, out status);
        BusinessLogicFilters.HealthcarePersonellShouldSeeEmergencyRelatedPatientDocumentReferences(registryObjects, businessLogic, out status);
        BusinessLogicFilters.HealthcarePersonellWithMissingAttributesShouldNotSeeDocumentReferences(registryObjects,businessLogic,out status);
    }



    public static BusinessLogicParameters? MapXacmlRequestToBusinessLogicParameters(XacmlContextRequest? xacmlRequest)
    {
        if (xacmlRequest == null) return null;

        var businessLogic = new BusinessLogicParameters();

        var xacmlAttributes = xacmlRequest.GetAllXacmlContextAttributes();

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

    private static int GetAgeFromPatientId(string? patientId)
    {
        if (string.IsNullOrWhiteSpace(patientId) || patientId.Length != 11) return 0;

        var patientNin = Hl7FhirExtensions.ParseNorwegianNinToDateTime(patientId);

        var year = DateTime.Today.Year - (patientNin.HasValue ? patientNin.Value.Year : 0);

        return year;
    }
}