using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap.XdsTypes;

using static XcaXds.Commons.Commons.Constants.Oid.CodeSystems.Hl7.ConfidentialityCode;
using static XcaXds.Commons.Commons.Constants.Oid.CodeSystems.Hl7.PurposeOfUse;
using static XcaXds.Commons.Commons.Constants.Oid.CodeSystems.OtherIsoDerived.PurposeOfUse;

namespace XcaXds.Commons.Models.Custom.BusinessLogic;

public static class BusinessLogicFilters
{
    /// <summary>
    /// Jeg som innbygger (voksen) skal se alle mine egne dokumentreferanser; og ha tilgang til mine egne dokumenter
    /// </summary>
    public static BusinessLogicResult CitizenShouldSeeOwnDocumentReferences(
        IEnumerable<IdentifiableType> registryObjects,
        BusinessLogicParameters logic)
    {
        if (
            logic.Resource?.Code == logic.Subject?.Code &&
            logic.Resource?.CodeSystem == logic.Subject?.CodeSystem &&
            logic.QueriedSubject?.Code == logic.Subject?.Code &&
            logic.QueriedSubject?.CodeSystem == logic.Subject?.CodeSystem &&
            logic.Purpose?.Code?.IsAnyOf(PATRQT, SubjectOfCare_13) == true &&
            logic.Acp == Constants.Oid.Saml.Acp.NullValue &&
            logic.SubjectAge > 12
        )
        {
            return new(true, FilterByConfidentiality(registryObjects, Normal, Restricted), nameof(CitizenShouldSeeOwnDocumentReferences));
        }

        return new(false, registryObjects, nameof(CitizenShouldSeeOwnDocumentReferences));
    }

    /// <summary>
    /// Jeg som innbygger (barn) med alder mellom 12-16 skal ikke ha tilgang til dokumentreferanser/dokumenter
    /// </summary>
    public static BusinessLogicResult CitizenBetween12And16ShouldNotSeeDocumentReferences(
        IEnumerable<IdentifiableType> registryObjects,
        BusinessLogicParameters logic)
    {
        if (
            logic.Subject?.Code == logic.Resource?.Code &&
            logic.Purpose?.Code?.IsAnyOf(PATRQT, SubjectOfCare_13) == true &&
            logic.SubjectAge is > 12 and < 16
        )
        {
            return new(true, DenyAll(), nameof(CitizenBetween12And16ShouldNotSeeDocumentReferences));
        }

        return new(false, registryObjects, nameof(CitizenBetween12And16ShouldNotSeeDocumentReferences));
    }

    /// <summary>
    /// Jeg som innbygger (ungdom) med alder mellom 16-18 skal ha tilgang til til deler av dokumentreferanser/dokumenter
    /// </summary>
    public static BusinessLogicResult CitizenBetween16And18ShouldAccesPartsOfDocumentReferences(
        IEnumerable<IdentifiableType> registryObjects,
        BusinessLogicParameters logic)
    {
        if (
            logic.Subject?.Code == logic.Resource?.Code &&
            logic.Purpose?.Code?.IsAnyOf(PATRQT, SubjectOfCare_13) == true &&
            logic.SubjectAge is > 16 and < 18
        )
        {
            return new(true, FilterByConfidentiality(registryObjects, Normal, Restricted), nameof(CitizenBetween16And18ShouldAccesPartsOfDocumentReferences));
        }

        return new(false, registryObjects, nameof(CitizenBetween16And18ShouldAccesPartsOfDocumentReferences));
    }

    /// <summary>
    /// Jeg som innbygger skal se dokumentreferanser/dokumenter for mine barn under 12 år
    /// </summary>
    public static BusinessLogicResult CitizenShouldSeeChildrenBelow12DocumentReferences(
        IEnumerable<IdentifiableType> registryObjects,
        BusinessLogicParameters logic)
    {
        if (
            logic.Subject?.Code != logic.Resource?.Code &&
            logic.Purpose?.Code?.IsAnyOf(FAMRQT, SubjectOfCare_13) == true &&
            logic.Acp == Constants.Oid.Saml.Acp.RepresentCitizenUnder12 &&
            logic.SubjectAge < 12
        )
        {
            return new(true, FilterByConfidentiality(registryObjects, Normal, Restricted), nameof(CitizenShouldSeeChildrenBelow12DocumentReferences));
        }

        return new(false, registryObjects, nameof(CitizenBetween12And16ShouldNotSeeDocumentReferences));
    }

    /// <summary>
    /// Jeg som innbygger skal se dokumentreferanser/dokumenter til den som jeg har representasjonsforhold for
    /// </summary>
    public static BusinessLogicResult CitizenShouldSeePowerOfAttorneyDocumentReferences(
        IEnumerable<IdentifiableType> registryObjects,
        BusinessLogicParameters logic)
    {
        if (
            logic.Subject?.Code != logic.Resource?.Code &&
            logic.Purpose?.Code?.IsAnyOf(PWATRNY, SubjectOfCare_13) == true &&
            logic.Acp == Constants.Oid.Saml.Acp.RepresentAnotherCitizen &&
            logic.SubjectAge is not (> 12 and < 16)
        )
        {
            return new(true, FilterByConfidentiality(registryObjects, Normal, Restricted), nameof(CitizenShouldSeePowerOfAttorneyDocumentReferences));
        }

        return new(false, registryObjects, nameof(CitizenShouldSeePowerOfAttorneyDocumentReferences));
    }

    /// <summary>
    /// Jeg som innbygger skal IKKE se dokumentreferanser/dokumenter til den som jeg IKKE har representasjonsforhold eller foreldreansvar for
    /// </summary>
    public static BusinessLogicResult CitizenShouldNotSeeNonPowerOfAttorneyDocumentReferences(
        IEnumerable<IdentifiableType> registryObjects,
        BusinessLogicParameters logic)
    {
        if (
            logic.Subject?.Code != logic.Resource?.Code &&
            logic.Acp == Constants.Oid.Saml.Acp.NullValue &&
            logic.Purpose?.Code?.IsAnyOf(PATRQT, FAMRQT, PWATRNY, SubjectOfCare_13) == true
        )
        {
            return new(true, DenyAll(), nameof(CitizenShouldNotSeeNonPowerOfAttorneyDocumentReferences));
        }

        return new(false, registryObjects, nameof(CitizenShouldNotSeeNonPowerOfAttorneyDocumentReferences));
    }

    /// <summary>
    /// Jeg som helsepersonell skal se alle mine egne dokumentreferanser; og ha tilgang til mine egne dokumenter
    /// </summary>
    public static BusinessLogicResult HealthcarePersonellShouldSeeOwnDocumentReferences(
        IEnumerable<IdentifiableType> registryObjects,
        BusinessLogicParameters logic)
    {
        if (
            logic.Subject?.Code == logic.Resource?.Code &&
            logic.Acp == Constants.Oid.Saml.Acp.NullValue &&
            logic.Purpose?.Code?.IsAnyOf(TREAT, CAREMGT, ClinicalCare_1, Management_5) == true
        )
        {
            return new(true, FilterByConfidentiality(registryObjects, Normal, Restricted), nameof(HealthcarePersonellShouldSeeOwnDocumentReferences));
        }

        return new(false, registryObjects, nameof(HealthcarePersonellShouldSeeOwnDocumentReferences));
    }

    /// <summary>
    /// Jeg som helsepersonell skal se alle dokumentreferanser/dokumenter for en pasient med relasjon til virksomheten som jeg representerer i en normal situasjon
    /// </summary>
    public static BusinessLogicResult HealthcarePersonellShouldSeeRelatedPatientDocumentReferences(
        IEnumerable<IdentifiableType> registryObjects,
        BusinessLogicParameters logic)
    {
        if (
            logic.Subject?.Code != logic.Resource?.Code &&
            logic.SubjectOrganization?.Code != null &&
            logic.Purpose?.Code?.IsAnyOf(TREAT, CAREMGT, ClinicalCare_1, Management_5) == true
        )
        {
            return new(true, FilterByConfidentiality(registryObjects, Normal, Restricted), nameof(HealthcarePersonellShouldSeeRelatedPatientDocumentReferences));
        }

        return new(false, registryObjects, nameof(HealthcarePersonellShouldSeeRelatedPatientDocumentReferences));
    }

    /// <summary>
    /// Jeg som helsepersonell skal se alle dokumentreferanser/dokumenter for en pasient med relasjon til virksomheten som jeg representerer i en akutt situasjon
    /// </summary>
    public static BusinessLogicResult HealthcarePersonellShouldSeeEmergencyRelatedPatientDocumentReferences(
        IEnumerable<IdentifiableType> registryObjects,
        BusinessLogicParameters logic)
    {
        if (
            logic.Subject?.Code != logic.Resource?.Code &&
            logic.SubjectOrganization?.Code != null &&
            logic.Purpose?.Code?.IsAnyOf(ETREAT, EmergencyCare_2) == true
        )
        {
            return new(true, FilterByConfidentiality(registryObjects, Normal, Restricted, VeryRestricted), nameof(HealthcarePersonellShouldSeeEmergencyRelatedPatientDocumentReferences));
        }

        return new(false, registryObjects, nameof(HealthcarePersonellShouldSeeEmergencyRelatedPatientDocumentReferences));
    }

    /// <summary>
    /// Jeg som helsepersonell som representerer en helsevirksomhet skal ikke se noen dokumentreferanser/dokumenter dersom det mangler viktige elementer som f.eks. korrekt angitt Purpose of Use
    /// </summary>
    public static BusinessLogicResult HealthcarePersonellWithMissingAttributesShouldNotSeeDocumentReferences(
        IEnumerable<IdentifiableType> registryObjects,
        BusinessLogicParameters logic)
    {
        if (
            logic.Subject?.Code == null ||
            logic.Resource?.Code == null ||
            logic.SubjectOrganization?.Code == null ||
            logic.Purpose?.Code?.IsAnyOf(TREAT, ETREAT, COC, BTG, PATRQT, FAMRQT, PWATRNY, ClinicalCare_1, EmergencyCare_2, Management_5, SubjectOfCare_13) == false
        )
        {
            return new(true, DenyAll(), nameof(HealthcarePersonellWithMissingAttributesShouldNotSeeDocumentReferences));
        }

        return new(false, registryObjects, nameof(HealthcarePersonellWithMissingAttributesShouldNotSeeDocumentReferences));
    }

    private static IEnumerable<IdentifiableType> FilterByConfidentiality(
    IEnumerable<IdentifiableType> source,
    params string[] allowedLevels)
    {
        return source
            .OfType<ExtrinsicObjectType>()
            .Where(ext =>
                ext.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
                   .All(cc => allowedLevels.Contains(cc.NodeRepresentation)))
            .Cast<IdentifiableType>();
    }

    private static IEnumerable<IdentifiableType> DenyAll()
    => Enumerable.Empty<IdentifiableType>();

}
