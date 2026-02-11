namespace XcaXds.Commons.Models.Custom;

public class SamlClaimValues
{
    public string? NameId { get; set; }
    public string? SubjectId { get; set; }
    public string? Organization { get; set; }
    public string? OrganizationCodeSystem { get; set; }
    public string? OrganizationAuthority { get; set; }
    public string? OrganizationId { get; set; }
    public string? ChildOrganization { get; set; }
    public string? ChildOrganizationCodeSystem { get; set; }
    public string? ChildOrganizationAuthority { get; set; }
    public string? ChildOrganizationName { get; set; }
    public string? Facility { get; set; }
    public string? FacilityCodeSystem { get; set; }
    public string? FacilityName { get; set; }
    public string? FacilityAuthority { get; set; }
    public string? RoleCodeSystem { get; set; }
    public string? RoleCodeSystemName { get; set; }
    public string? RoleCode { get; set; }
    public string? RoleCodeName { get; set; }
    public string? HomeCommunityId { get; set; }
    public string? Npi { get; set; }
    public string? ProviderIdentifier { get; set; }
    public string? PurposeOfUseCode { get; set; }
    public string? PurposeOfUseDescription { get; set; } = default!;
    public string? PurposeOfUseCodeSystem { get; set; } = default!;
    public string? PurposeOfUseDetailsCode { get; set; }
    public string? PurposeOfUseDetailsDescription { get; set; } = default!;
    public string? PurposeOfUseAuthorityName { get; set; } = default!;
    public string? PurposeOfUseDetailsCodeSystem { get; set; } = default!;
    public string? ResourceId { get; set; }
    public string? SecurityLevel { get; set; }
    public string? Scope { get; set; }
    public string? ClientId { get; set; }
    public string? AuthenticationMethod { get; set; }
    public bool IsFastlege { get; set; }
    public string? PatientChildOrganization { get; set; }
    public string? PatientChildOrganizationName { get; set; }
    public string? PatientChildOrganizationAuthority { get; set; }
    public string? PatientChildOrganizationCodeSystem { get; set; }
    public string? PatientFacility { get; set; }
    public string? PatientFacilityAuthority { get; set; }
    public string? PatientFacilityName { get; set; }
    public string? PatientFacilityCodeSystem { get; set; }
    public string? HealthcareServiceCode { get; set; }
    public string? HealthcareServiceText { get; set; }
    public string? HealthcareServiceCodeSystem { get; set; }
    public string? HealthcareServiceAssigningAuthority { get; set; }
    public string? DecisionRefId { get; set; }
    public bool DecisionRefUserSelected { get; set; }
    public string? XuaAcp { get; set; }
    public string? BppcDocId { get; set; }

    public string? OrgnrParent { get; set; }
    public string? ClientName { get; set; }
    public string? Pid { get; set; }
    public string? HprNumber { get; set; }
    public string? Name { get; set; }
    public string? GivenName { get; set; }
    public string? MiddleName { get; set; }
    public string? FamilyName { get; set; }
}
