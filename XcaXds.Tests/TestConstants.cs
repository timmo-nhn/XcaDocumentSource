namespace XcaXds.Tests;

public static class TestConstants
{
    public static class AssigningAuthority
    {
        public const string Nin = "2.16.578.1.12.4.1.4.1";
        public const string TNin = "2.16.578.1.12.4.1.4.2";
        public const string ENin = "2.16.578.1.12.4.1.4.3";
        public const string GPIN = "2.16.578.1.12.4.1.4.4";
    }

    public static class Acp
    {
        public const string NullValue = "2.16.578.1.12.4.1.7.2.1.0";
        public const string RepresentCitizenUnder12 = "2.16.578.1.12.4.1.7.2.1.1";
        public const string RepresentAnotherCitizen = "2.16.578.1.12.4.1.7.2.1.2";
        public const string RepresentedUnableToConsent = "2.16.578.1.12.4.1.7.2.1.3";
        public const string NotObligedToConsent = "2.16.578.1.12.4.1.7.2.1.4";
        public const string ExcplicitConsent = "2.16.578.1.12.4.1.7.2.1.5";
        public const string UnableToConsent = "2.16.578.1.12.4.1.7.2.1.6";
        public const string ExceptionToConcent = "2.16.578.1.12.4.1.7.2.1.7";
        public const string HasConsent = "2.16.578.1.12.4.1.7.2.1.8";
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

    public static class CodeSystems
    {
        public static class Volven
        {
            public static class ConfidentialityCode_9603
            {
                public const string System = "2.16.578.1.12.4.1.1.9603";

                /// <summary> Normal</summary>
                public const string N = "N";
                /// <summary> Nektet, andre grunner</summary>
                public const string NORN_ANG = "NORN_ANG";
                /// <summary> Nektet, alle dokumenter</summary>
                public const string NORN_ALL = "NORN_ALL";
                /// <summary> Nektet, duplikat</summary>
                public const string NORN_DUP = "NORN_DUP";
                /// <summary> Nektet, eget ønske</summary>
                public const string NORN_EPO = "NORN_EPO";
                /// <summary> Nektet, fare for helsepersonell</summary>
                public const string NORN_FFH = "NORN_FFH";
                /// <summary> Nektet, fare for liv</summary>
                public const string NORN_FFL = "NORN_FFL";
                /// <summary> Nektet, foreldet</summary>
                public const string NORN_FOR = "NORN_FOR";
                /// <summary> Nektet, foreldreansvarlig</summary>
                public const string NORN_FORANS = "NORN_FORANS";
                /// <summary> Nektet, forsvarlig pasientbehandling</summary>
                public const string NORN_FPB = "NORN_FPB";
                /// <summary> Nektet, klart utilrådelig</summary>
                public const string NORN_KUT = "NORN_KUT";
                /// <summary> Nektet, ungdom</summary>
                public const string NORN_UNGDOM = "NORN_UNGDOM";
                /// <summary> Sperret</summary>
                public const string NORS = "NORS";
                /// <summary> Utsatt innsyn for innbygger</summary>
                public const string NORU = "NORU";
            }
        }

        public static class Hl7
        {
            public static class ConfidentialityCode
            {
                public const string System = "2.16.840.1.113883.5.25";
                public const string System_Alternate = "http://terminology.hl7.org/CodeSystem/v3-Confidentiality";

                /// <summary>low</summary>
                public const string Low = "L";
                /// <summary>moderate</summary>
                public const string Moderate = "M";
                /// <summary>normal</summary>
                public const string Normal = "N";
                /// <summary>restricted</summary>
                public const string Restricted = "R";
                /// <summary>unrestricted</summary>
                public const string Unrestricted = "U";
                /// <summary>veryrestricted</summary>
                public const string VeryRestricted = "V";
            }

            public static class Lifecycle
            {
                public const string IsoHealthRecordLifecycleEvent = "http://terminology.hl7.org/CodeSystem/iso-21089-lifecycle";
            }

            public static class AuditEventId
            {
                public const string System = "2.16.840.1.113883.4.642.3.462";
            }

            public static class PurposeOfUse
            {
                public const string System = "2.16.840.1.113883.1.11.20448";
                /// <summary>healthcare marketing</summary>
                public const string HMARKT = "HMARKT";
                /// <summary>healthcare operations</summary>
                public const string HOPERAT = "HOPERAT";
                /// <summary>care management</summary>
                public const string CAREMGT = "CAREMGT";
                /// <summary>donation</summary>
                public const string DONAT = "DONAT";
                /// <summary>fraud</summary>
                public const string FRAUD = "FRAUD";
                /// <summary>government</summary>
                public const string GOV = "GOV";
                /// <summary>health accreditation</summary>
                public const string HACCRED = "HACCRED";
                /// <summary>health compliance</summary>
                public const string HCOMPL = "HCOMPL";
                /// <summary>decedent</summary>
                public const string HDECD = "HDECD";
                /// <summary>directory</summary>
                public const string HDIRECT = "HDIRECT";
                /// <summary>healthcare delivery management</summary>
                public const string HDM = "HDM";
                /// <summary>legal</summary>
                public const string HLEGAL = "HLEGAL";
                /// <summary>health outcome measure</summary>
                public const string HOUTCOMS = "HOUTCOMS";
                /// <summary>health program reporting</summary>
                public const string HPRGRP = "HPRGRP";
                /// <summary>health quality improvement</summary>
                public const string HQUALIMP = "HQUALIMP";
                /// <summary>health system administration</summary>
                public const string HSYSADMIN = "HSYSADMIN";
                /// <summary>labeling</summary>
                public const string LABELING = "LABELING";
                /// <summary>metadata management</summary>
                public const string METAMGT = "METAMGT";
                /// <summary>member administration</summary>
                public const string MEMADMIN = "MEMADMIN";
                /// <summary>military command</summary>
                public const string MILCDM = "MILCDM";
                /// <summary>patient administration</summary>
                public const string PATADMIN = "PATADMIN";
                /// <summary>patient safety</summary>
                public const string PATSFTY = "PATSFTY";
                /// <summary>performance measure</summary>
                public const string PERFMSR = "PERFMSR";
                /// <summary>records management</summary>
                public const string RECORDMGT = "RECORDMGT";
                /// <summary>system development</summary>
                public const string SYSDEV = "SYSDEV";
                /// <summary>test health data</summary>
                public const string HTEST = "HTEST";
                /// <summary>training</summary>
                public const string TRAIN = "TRAIN";
                /// <summary>healthcare payment</summary>
                public const string HPAYMT = "HPAYMT";
                /// <summary>claim attachment</summary>
                public const string CLMATTCH = "CLMATTCH";
                /// <summary>coverage authorization</summary>
                public const string COVAUTH = "COVAUTH";
                /// <summary>coverage under policy or program</summary>
                public const string COVERAGE = "COVERAGE";
                /// <summary>eligibility determination</summary>
                public const string ELIGDTRM = "ELIGDTRM";
                /// <summary>eligibility verification</summary>
                public const string ELIGVER = "ELIGVER";
                /// <summary>enrollment</summary>
                public const string ENROLLM = "ENROLLM";
                /// <summary>military discharge</summary>
                public const string MILDCRG = "MILDCRG";
                /// <summary>remittance advice</summary>
                public const string REMITADV = "REMITADV";
                /// <summary>healthcare research</summary>
                public const string HRESCH = "HRESCH";
                /// <summary>biomedical research</summary>
                public const string BIORCH = "BIORCH";
                /// <summary>clinical trial research</summary>
                public const string CLINTRCH = "CLINTRCH";
                /// <summary>clinical trial research without patient care</summary>
                public const string CLINTRCHNPC = "CLINTRCHNPC";
                /// <summary>clinical trial research with patient care</summary>
                public const string CLINTRCHPC = "CLINTRCHPC";
                /// <summary>preclinical trial research</summary>
                public const string PRECLINTRCH = "PRECLINTRCH";
                /// <summary>disease specific healthcare research</summary>
                public const string DSRCH = "DSRCH";
                /// <summary>population origins or ancestry healthcare research</summary>
                public const string POARCH = "POARCH";
                /// <summary>translational healthcare research</summary>
                public const string TRANSRCH = "TRANSRCH";
                /// <summary>patient requested</summary>
                public const string PATRQT = "PATRQT";
                /// <summary>family requested</summary>
                public const string FAMRQT = "FAMRQT";
                /// <summary>power of attorney</summary>
                public const string PWATRNY = "PWATRNY";
                /// <summary>support network</summary>
                public const string SUPNWK = "SUPNWK";
                /// <summary>public health</summary>
                public const string PUBHLTH = "PUBHLTH";
                /// <summary>disaster</summary>
                public const string DISASTER = "DISASTER";
                /// <summary>threat</summary>
                public const string THREAT = "THREAT";
                /// <summary>treatment</summary>
                public const string TREAT = "TREAT";
                /// <summary>clinical trial</summary>
                public const string CLINTRL = "CLINTRL";
                /// <summary>coordination of care</summary>
                public const string COC = "COC";
                /// <summary>Emergency Treatment</summary>
                public const string ETREAT = "ETREAT";
                /// <summary>break the glass</summary>
                public const string BTG = "BTG";
                /// <summary>emergency room treatment</summary>
                public const string ERTREAT = "ERTREAT";
                /// <summary>population health</summary>
                public const string POPHLTH = "POPHLTH";
            }
        }

        public static class OtherIsoDerived
        {
            public static class PurposeOfUse
            {
                public const string System = "1.0.14265.1";
                public const string ClinicalCare_1 = "1";
                public const string EmergencyCare_2 = "2";
                public const string Management_5 = "5";
                public const string SubjectOfCare_13 = "13";
            }
        }
    }
}