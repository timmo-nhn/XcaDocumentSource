using Microsoft.IdentityModel.Tokens.Saml;
using XcaXds.Commons.Codes;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom;

namespace XcaXds.Commons.Extensions;

public static class SamlTrustFrameworkClaimsMapper
{
    public static SamlClaimValues GetClaimValues(Dictionary<string, string> claims)
    {
        // https://github.com/NorskHelsenett/Tillitsrammeverk/blob/main/specs/informasjons_og_datamodell.md#631-attributter-koblet-mot-ihe-xds-og-xua-saml-profil-for-kjernejournal
        var tfClaims = claims.FirstOrDefault(clm => clm.Key == Constants.JwtSaml.TillitsrammeverkClaimType).Value.ToString();
        var tillitsrammeverkClaim = TillitsrammeverkParser.ParseFromClaim(tfClaims);

        var xuaClaim = claims.GetValueOrDefault("xua-acp");
        string bppcDocid = null;
        if (xuaClaim != null)
        {
            bppcDocid = "2.16.578.1.12.4.1.7.2.2.1";
        }

        return new SamlClaimValues
        {
            ResourceId = claims.GetValueOrDefault("resource:resource-id"),

            NameId = tillitsrammeverkClaim.Practitioner.Identifier.Id,
            SubjectId = tillitsrammeverkClaim.Practitioner.Identifier.Name,
            Organization = tillitsrammeverkClaim.Practitioner.LegalEntity.Name,
            OrganizationId = tillitsrammeverkClaim.Practitioner.LegalEntity.Id,
            OrganizationCodeSystem = tillitsrammeverkClaim.Practitioner.LegalEntity.System,
            OrganizationAuthority = tillitsrammeverkClaim.Practitioner.LegalEntity.Authority,
            ChildOrganization = tillitsrammeverkClaim.Practitioner.PointOfCare.Id,
            ChildOrganizationName = tillitsrammeverkClaim.Practitioner.PointOfCare.Name,
            ChildOrganizationAuthority = tillitsrammeverkClaim.Practitioner.PointOfCare.Authority,
            ChildOrganizationCodeSystem = tillitsrammeverkClaim.Practitioner.PointOfCare.System,
            Facility = tillitsrammeverkClaim.Practitioner.Department?.Id,
            FacilityAuthority = tillitsrammeverkClaim.Practitioner.Department?.Authority,
            FacilityCodeSystem = tillitsrammeverkClaim.Practitioner.Department?.System,
            FacilityName = tillitsrammeverkClaim.Practitioner.Department?.Name,
            RoleCodeSystem = tillitsrammeverkClaim.Practitioner.Authorization?.System,
            RoleCodeSystemName = CodeSystemNamingDevice.GetNameFromCodeOrDefault(tillitsrammeverkClaim.Practitioner.Authorization?.System),
            RoleCode = tillitsrammeverkClaim.Practitioner.Authorization?.Code,
            RoleCodeName = tillitsrammeverkClaim.Practitioner.Authorization?.Text,
            Npi = tillitsrammeverkClaim.Practitioner.HprNr?.Id ?? tillitsrammeverkClaim.Practitioner.Identifier.Id,
            ProviderIdentifier = tillitsrammeverkClaim.Practitioner.HprNr?.Id ?? tillitsrammeverkClaim.Practitioner.Identifier.Id,
            // Care relationship
            PurposeOfUseCode = tillitsrammeverkClaim.CareRelationship.PurposeOfUse.Code,
            PurposeOfUseDescription = tillitsrammeverkClaim.CareRelationship.PurposeOfUse.Text,
            PurposeOfUseCodeSystem = tillitsrammeverkClaim.CareRelationship.PurposeOfUse.System,
            PurposeOfUseDetailsCode = tillitsrammeverkClaim.CareRelationship.PurposeOfUseDetails?.Code,
            PurposeOfUseDetailsDescription = tillitsrammeverkClaim.CareRelationship.PurposeOfUseDetails?.Text,
            PurposeOfUseDetailsCodeSystem = tillitsrammeverkClaim.CareRelationship.PurposeOfUseDetails?.System,
            PurposeOfUseAuthorityName = tillitsrammeverkClaim.CareRelationship.PurposeOfUseDetails?.Assigner,
            HealthcareServiceCode = tillitsrammeverkClaim.CareRelationship.HealthcareService?.Code,
            HealthcareServiceText = tillitsrammeverkClaim.CareRelationship.HealthcareService?.Text,
            HealthcareServiceCodeSystem = tillitsrammeverkClaim.CareRelationship.HealthcareService?.System,
            HealthcareServiceAssigningAuthority = tillitsrammeverkClaim.CareRelationship.HealthcareService?.Assigner,
            DecisionRefId = tillitsrammeverkClaim.CareRelationship.DecisionRef.Id,
            DecisionRefUserSelected = tillitsrammeverkClaim.CareRelationship.DecisionRef.UserSelected,
            // Patient
            PatientChildOrganization = tillitsrammeverkClaim.Patients.SingleOrDefault()?.PointOfCare?.Id,
            PatientChildOrganizationName = tillitsrammeverkClaim.Patients.SingleOrDefault()?.PointOfCare?.Name,
            PatientChildOrganizationCodeSystem = tillitsrammeverkClaim.Patients.SingleOrDefault()?.PointOfCare?.System,
            PatientChildOrganizationAuthority = tillitsrammeverkClaim.Patients.SingleOrDefault()?.PointOfCare?.Authority,
            PatientFacility = tillitsrammeverkClaim.Patients.SingleOrDefault()?.Department?.Id,
            PatientFacilityName = tillitsrammeverkClaim.Patients.SingleOrDefault()?.Department?.Name,
            PatientFacilityCodeSystem = tillitsrammeverkClaim.Patients.SingleOrDefault()?.Department?.System,
            PatientFacilityAuthority = tillitsrammeverkClaim.Patients.SingleOrDefault()?.Department?.Authority,
            // Other claims
            HomeCommunityId = claims.GetValueOrDefault("homeCommunityId"),
            Scope = "journaldokumenter_helsepersonell",
            SecurityLevel = claims.GetValueOrDefault(Constants.JwtSaml.SecurityLevelClaimType),
            ClientId = claims.GetValueOrDefault("client_id"),
            AuthenticationMethod = claims.GetValueOrDefault("idp"),
            IsFastlege = claims.GetValueOrDefault(Constants.JwtSaml.FastlegeClaimType) == "true",
            XuaAcp = xuaClaim,
            BppcDocId = bppcDocid,
        };
    }
}