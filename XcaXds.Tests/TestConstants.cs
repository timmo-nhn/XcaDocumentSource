namespace XcaXds.Tests;

public static class TestConstants
{
    public static class PurposeOfUse
    {
        public const string Normal = "N";
        public const string Restricted = "R";
        public const string VeryRestricted = "V";
    }

    public static class SamlAttributes
    {
        // --- XSPA core subject attributes ---
        public const string SubjectId = "urn:oasis:names:tc:xspa:1.0:subject:subject-id";
        public const string Organization = "urn:oasis:names:tc:xspa:1.0:subject:organization";
        public const string OrganizationId = "urn:oasis:names:tc:xspa:1.0:subject:organization-id";
        public const string ChildOrganization = "urn:oasis:names:tc:xspa:1.0:subject:child-organization";
        public const string Role = "urn:oasis:names:tc:xspa:1.0:subject:role";
        public const string PurposeOfUse = "urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse";
        public const string PurposeOfUse_Helsenorge = "urn:oasis:names:tc:xspa:1.0:subject:purposeofuse";
        public const string Npi = "urn:oasis:names:tc:xspa:2.0:subject:npi";

        // --- IHE / XUA / XCA / BPPC ---
        public const string HomeCommunityIdXca = "urn:ihe:iti:xca:2010:homeCommunityId";
        public const string ProviderIdentifier = "urn:ihe:iti:xua:2017:subject:provider-identifier";
        public const string BppcDocId = "urn:ihe:iti:bppc:2007:docid";
        public const string XuaAcp = "urn:ihe:iti:xua:2012:acp";

        // --- XACML attributes ---
        public const string ResourceId10 = "urn:oasis:names:tc:xacml:1.0:resource:resource-id";
        public const string ResourceId20 = "urn:oasis:names:tc:xacml:2.0:resource:resource-id";
        public const string SubjectRole20 = "urn:oasis:names:tc:xacml:2.0:subject:role";
        public const string ActionPurpose20 = "urn:oasis:names:tc:xacml:2.0:action:purpose";

        // --- eHelse-specific attributes ---
        public const string EhelseHomeCommunityId = "urn:no:ehelse:saml:1.0:subject:homeCommunityId";
        public const string EhelseSecurityLevel = "urn:no:ehelse:saml:1.0:subject:SecurityLevel";
        public const string EhelseScope = "urn:no:ehelse:saml:1.0:subject:Scope";
        public const string EhelseClientId = "urn:no:ehelse:saml:1.0:subject:client_id";
        public const string EhelseAuthenticationMethod = "urn:no:ehelse:saml:1.0:subject:Authentication_method";
        public const string EhelseHealthcareService = "urn:no:ehelse:saml:1.1:subject:healthcareservice";

        // --- NHN Trust Framework extensions ---
        public const string TrustChildOrgName = "urn:nhn:trust-framework:1.0:ext:subject:child-organization-name";
        public const string TrustResourceChildOrg = "urn:nhn:trust-framework:1.0:ext:resource:child-organization";
        public const string TrustResourceChildOrgId = "urn:nhn:trust-framework:1.0:ext:resource:child-organization-id";
        public const string TrustHealthcareService = "urn:nhn:trust-framework:1.0:ext:care-relationship:healthcare-service";
        public const string TrustPurposeOfUseDetails = "urn:nhn:trust-framework:1.0:ext:care-relationship:purpose-of-use-details";
        public const string TrustDecisionRef = "urn:nhn:trust-framework:1.0:ext:care-relationship:decision-ref";

        // --- Generic / misc ---
        public const string SamlSubjectId = "urn:oasis:names:tc:SAML:attribute:subject-id";
    }
}