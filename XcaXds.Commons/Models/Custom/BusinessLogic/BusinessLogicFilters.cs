using Microsoft.Extensions.Logging.Abstractions;
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
    public static void CitizenShouldSeeOwnDocumentReferences(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters businessLogic, out bool status)
    {
        if (
            businessLogic.Resource?.Code == businessLogic.Subject?.Code &&
            businessLogic.Resource?.CodeSystem == businessLogic.Subject?.CodeSystem &&
            businessLogic.QueriedSubject?.Code == businessLogic.Subject?.Code &&
            businessLogic.QueriedSubject?.CodeSystem == businessLogic.Subject?.CodeSystem &&
            businessLogic.Purpose?.Code?.IsAnyOf(PATRQT, SubjectOfCare_13) == true &&
            businessLogic.Acp == Constants.Oid.Saml.Acp.NullValue &&
            businessLogic.SubjectAge > 12
        )
        {
            status = true;
            registryObjects = registryObjects.Where(ro =>
            {
                if (ro is ExtrinsicObjectType extrinsicObject)
                {
                    return extrinsicObject.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
                        .All(ccode => ccode.NodeRepresentation.IsAnyOf(Normal, Restricted, VeryRestricted));
                }
                return false;
            });

            return;
        }

        status = false;
    }

    /// <summary>
    /// Jeg som innbygger (barn) med alder mellom 12-16 skal ikke ha tilgang til dokumentreferanser/dokumenter
    /// </summary>
    public static void CitizenBetween12And16ShouldNotSeeDocumentReferences(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters businessLogic, out bool status)
    {
        if (
            businessLogic.Subject?.Code == businessLogic.Resource?.Code &&
            businessLogic.Purpose?.Code?.IsAnyOf(PATRQT, SubjectOfCare_13) == true &&
            businessLogic.SubjectAge > 12
        )
        {
            status = true;
            registryObjects = registryObjects.Where(ro =>
            {
                return false;
            });

            return;
        }

        status = false;
    }

    /// <summary>
    /// Jeg som innbygger (ungdom) med alder mellom 16-18 skal ha tilgang til til deler av dokumentreferanser/dokumenter
    /// </summary>
    public static void CitizenBetween16And18ShouldAccesPartsOfDocumentReferences(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters businessLogic, out bool status)
    {
        if (
            businessLogic.Subject?.Code == businessLogic.Resource?.Code &&
            businessLogic.Purpose?.Code?.IsAnyOf(PATRQT, SubjectOfCare_13) == true &&
            businessLogic.SubjectAge is > 12 and < 16
        )
        {
            status = true;
            registryObjects = registryObjects.Where(ro =>
            {
                if (ro is ExtrinsicObjectType extrinsicObject)
                {
                    return extrinsicObject.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
                        .All(ccode => ccode.NodeRepresentation.IsAnyOf(Normal, Restricted) && ccode.NodeRepresentation != VeryRestricted);
                }
                return false;
            });

            return;
        }

        status = false;
    }

    /// <summary>
    /// Jeg som innbygger skal se dokumentreferanser/dokumenter for mine barn under 12 år
    /// </summary>
    public static void CitizenShouldSeeChildrenBelow12DocumentReferences(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters businessLogic, out bool status)
    {
        if (
            businessLogic.Subject?.Code != businessLogic.Resource?.Code &&
            businessLogic.Purpose?.Code?.IsAnyOf(FAMRQT, SubjectOfCare_13) == true &&
            businessLogic.Acp == Constants.Oid.Saml.Acp.RepresentCitizenUnder12 &&
            businessLogic.SubjectAge < 12
        )
        {
            status = true;
            registryObjects = registryObjects.Where(ro =>
            {
                if (ro is ExtrinsicObjectType extrinsicObject)
                {
                    return extrinsicObject.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
                        .All(ccode => ccode.NodeRepresentation.IsAnyOf(Normal, Restricted) && ccode.NodeRepresentation != VeryRestricted);
                }
                return false;
            });

            return;
        }

        status = false;
    }

    /// <summary>
    /// Jeg som innbygger skal se dokumentreferanser/dokumenter til den som jeg har representasjonsforhold for
    /// </summary>
    public static void CitizenShouldSeePowerOfAttorneyDocumentReferences(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters businessLogic, out bool status)
    {
        if (
            businessLogic.Subject?.Code != businessLogic.Resource?.Code &&
            businessLogic.Purpose?.Code?.IsAnyOf(PWATRNY, SubjectOfCare_13) == true &&
            businessLogic.Acp == Constants.Oid.Saml.Acp.RepresentAnotherCitizen &&
            (businessLogic.SubjectAge is > 12 and < 16) == false
        )
        {
            status = true;
            registryObjects = registryObjects.Where(ro =>
            {
                if (ro is ExtrinsicObjectType extrinsicObject)
                {
                    return extrinsicObject.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
                        .All(ccode => ccode.NodeRepresentation.IsAnyOf(Normal, Restricted) && ccode.NodeRepresentation != VeryRestricted);
                }
                return false;
            });

            return;
        }

        status = false;
    }

    /// <summary>
    /// Jeg som innbygger skal IKKE se dokumentreferanser/dokumenter til den som jeg IKKE har representasjonsforhold eller foreldreansvar for
    /// </summary>
    public static void CitizenShouldNotSeeNonPowerOfAttorneyDocumentReferences(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters businessLogic, out bool status)
    {
        if (
            businessLogic.Subject?.Code != businessLogic.Resource?.Code &&
            businessLogic.Acp == Constants.Oid.Saml.Acp.NullValue && 
            businessLogic.Purpose?.Code?.IsAnyOf(PATRQT,FAMRQT,PWATRNY,SubjectOfCare_13) == false
        )
        {
            status = true;
            registryObjects = registryObjects.Where(ro =>
            {
                return false;
            });

            return;
        }

        status = false;
    }

    /// <summary>
    /// Jeg som helsepersonell skal se alle mine egne dokumentreferanser; og ha tilgang til mine egne dokumenter
    /// </summary>
    public static void HealthcarePersonellShouldSeeOwnDocumentReferences(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters businessLogic, out bool status)
    {
        if (
            businessLogic.Subject?.Code == businessLogic.Resource?.Code &&
            businessLogic.Acp == Constants.Oid.Saml.Acp.NullValue && 
            businessLogic.Purpose?.Code?.IsAnyOf(TREAT,CAREMGT,ClinicalCare_1,Management_5) == true
        )
        {
            status = true;
            registryObjects = registryObjects.Where(ro =>
            {
                if (ro is ExtrinsicObjectType extrinsicObject)
                {
                    return extrinsicObject.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
                        .All(ccode => ccode.NodeRepresentation.IsAnyOf(Normal, Restricted) && ccode.NodeRepresentation != VeryRestricted);
                }
                return false;
            });

            return;
        }

        status = false;
    }

    /// <summary>
    /// Jeg som helsepersonell skal se alle dokumentreferanser/dokumenter for en pasient med relasjon til virksomheten som jeg representerer i en normal situasjon
    /// </summary>
    public static void HealthcarePersonellShouldSeeRelatedPatientDocumentReferences(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters businessLogic, out bool status)
    {
        if (
            businessLogic.Subject?.Code != businessLogic.Resource?.Code &&
            businessLogic.SubjectOrganization?.Code != null &&
            businessLogic.Purpose?.Code?.IsAnyOf(TREAT,CAREMGT,ClinicalCare_1,Management_5) == true
        )
        {
            status = true;
            registryObjects = registryObjects.Where(ro =>
            {
                if (ro is ExtrinsicObjectType extrinsicObject)
                {
                    return extrinsicObject.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
                        .All(ccode => ccode.NodeRepresentation.IsAnyOf(Normal, Restricted) && ccode.NodeRepresentation != VeryRestricted);
                }
                return false;
            });

            return;
        }

        status = false;
    }

    /// <summary>
    /// Jeg som helsepersonell skal se alle dokumentreferanser/dokumenter for en pasient med relasjon til virksomheten som jeg representerer i en akutt situasjon
    /// </summary>
    public static void HealthcarePersonellShouldSeeEmergencyRelatedPatientDocumentReferences(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters businessLogic, out bool status)
    {
        if (
            businessLogic.Subject?.Code != businessLogic.Resource?.Code &&
            businessLogic.SubjectOrganization?.Code != null &&
            businessLogic.Purpose?.Code?.IsAnyOf(ETREAT,EmergencyCare_2) == true
        )
        {
            status = true;
            registryObjects = registryObjects.Where(ro =>
            {
                if (ro is ExtrinsicObjectType extrinsicObject)
                {
                    return extrinsicObject.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
                        .All(ccode => ccode.NodeRepresentation.IsAnyOf(Normal, Restricted, VeryRestricted));
                }
                return false;
            });

            return;
        }

        status = false;
    }

    /// <summary>
    /// Jeg som helsepersonell som representerer en helsevirksomhet skal ikke se noen dokumentreferanser/dokumenter dersom det mangler viktige elementer som f.eks. korrekt angitt Purpose of Use
    /// </summary>
    public static void HealthcarePersonellWithMissingAttributesShouldNotSeeDocumentReferences(IEnumerable<IdentifiableType> registryObjects, BusinessLogicParameters businessLogic, out bool status)
    {
        if (
            businessLogic.Subject?.Code == null ||
            businessLogic.Resource?.Code == null ||
            businessLogic.SubjectOrganization?.Code == null 
        )
        {
            status = true;
            registryObjects = registryObjects.Where(ro =>
            {
                return false;
            });

            return;
        }

        status = false;
    }

}
