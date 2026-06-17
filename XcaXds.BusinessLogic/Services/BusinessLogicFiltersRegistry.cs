using Hl7.Fhir.Model;
using XcaXds.BusinessLogic.Extensions;
using XcaXds.BusinessLogic.Models.Custom;
using XcaXds.BusinessLogic.Models.Custom.BusinessLogic;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared.Constants;
using XcaXds.Shared.Extensions;
using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology;
using XcaXds.Terminology.Services;
using static XcaXds.BusinessLogic.Services.ValuesToUseForBusinessLogic;

namespace XcaXds.BusinessLogic.Services;

internal static class ValuesToUseForBusinessLogic
{
    internal static string BTG = default!;
    internal static string COC = default!;
    internal static string CAREMGT = default!;
    internal static string ETREAT = default!;
    internal static string FAMRQT = default!;
    internal static string PATRQT = default!;
    internal static string PWATRNY = default!;
    internal static string TREAT = default!;

    internal static string ClinicalCare_1 = default!;
    internal static string EmergencyCare_2 = default!;
    internal static string Management_5 = default!;
    internal static string SubjectOfCare_13 = default!;

    internal static string Normal = default!;
    internal static string Restricted = default!;
    internal static string VeryRestricted = default!;

    internal static class Acp
    {
        internal static string NullValue = default!;
        internal static string RepresentCitizenUnder12 = default!;
        internal static string RepresentAnotherCitizen = default!;
        internal static string RepresentedUnableToConsent = default!;
        internal static string NotObligedToConsent = default!;
        internal static string ExcplicitConsent = default!;
        internal static string UnableToConsent = default!;
        internal static string ExceptionToConcent = default!;
        internal static string HasConsent = default!;
    }
}

public class BusinessLogicFiltersRegistry
{
    private readonly TerminologyService _terminologyService;

    public BusinessLogicFiltersRegistry(TerminologyService terminologyService)
    {
        _terminologyService = terminologyService;
        InitConstantValuesUsedForBusinessLogicFiltering();
        AllBusinessRules = GetAllBusinessRulesForFilteringDocumentList();
    }

    //public static readonly ComprehensiveCodeSystem VolvenDocumentTypes = typeof(Constants.CodeSystems.Volven.CategoryCode_9602).GetAsComprehensiveCodesystem();

    //public static readonly Dictionary<string, string> Hl7ConfCodeClass = ConstantsExtensions.GetAsDictionary(typeof(Constants.CodeSystems.Hl7.ConfidentialityCode));
    //public static readonly string? Hl7ConfCodeOid = Hl7ConfCodeClass.Where(kvp => string.Equals(kvp.Key, "System", StringComparison.InvariantCultureIgnoreCase)).Select(kvp => kvp.Value).FirstOrDefault() ?? string.Empty;
    //public static readonly CodedValue[]? Hl7ConfCodeValues = [.. Hl7ConfCodeClass.Where(kvp => !string.Equals(kvp.Key, "System", StringComparison.InvariantCultureIgnoreCase)).Select(kvp => new CodedValue() { Code = kvp.Value, CodeSystem = Hl7ConfCodeOid })];

    //public static readonly Dictionary<string, string> VolvenConfCodeClass = ConstantsExtensions.GetAsDictionary(typeof(Constants.CodeSystems.Volven.ConfidentialityCode_9603));
    //public static readonly string? VolvenConfCodeOid = VolvenConfCodeClass.Where(kvp => string.Equals(kvp.Key, "System", StringComparison.InvariantCultureIgnoreCase)).Select(kvp => kvp.Value).FirstOrDefault() ?? string.Empty;
    //public static readonly CodedValue[]? VolvenConfCodeValues = [.. VolvenConfCodeClass.Where(kvp => !string.Equals(kvp.Key, "System", StringComparison.InvariantCultureIgnoreCase)).Select(kvp => new CodedValue() { Code = kvp.Value, CodeSystem = VolvenConfCodeOid })];

    //public static readonly HashSet<(string Code, string CodeSystem)> AllConfidentialityCodes = [.. Hl7ConfCodeValues.Concat(VolvenConfCodeValues).Select(val => (val.Code!, val.CodeSystem!))];


    private void InitConstantValuesUsedForBusinessLogicFiltering()
    {
        var purposeOfUse = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Authentication.PurposeOfUse);
        var confidentialityCode = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Xds.ConfidentialityCode);
        var acp = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Authentication.Acp);

        BTG = purposeOfUse.GetByValueOid("BTG")?.Value!;
        COC = purposeOfUse.GetByValueOid("COC")?.Value!;
        CAREMGT = purposeOfUse.GetByValueOid("CAREMGT")?.Value!;
        ETREAT = purposeOfUse.GetByValueOid("ETREAT")?.Value!;
        FAMRQT = purposeOfUse.GetByValueOid("FAMRQT")?.Value!;
        PATRQT = purposeOfUse.GetByValueOid("PATRQT")?.Value!;
        PWATRNY = purposeOfUse.GetByValueOid("PWATRNY")?.Value!;
        TREAT = purposeOfUse.GetByValueOid("TREAT")?.Value!;
        ClinicalCare_1 = purposeOfUse.GetByValueOid("1")?.Value!;
        EmergencyCare_2 = purposeOfUse.GetByValueOid("2")?.Value!;
        Management_5 = purposeOfUse.GetByValueOid("5")?.Value!;
        SubjectOfCare_13 = purposeOfUse.GetByValueOid("13")?.Value!;

        Normal = confidentialityCode.GetByValueOid("N")?.Value!;
        Restricted = confidentialityCode.GetByValueOid("R")?.Value!;
        VeryRestricted = confidentialityCode.GetByValueOid("V")?.Value!;

        Acp.NullValue = acp.GetByName("NullValue")!;
        Acp.RepresentCitizenUnder12 = acp.GetByName("RepresentCitizenUnder12")!;
        Acp.RepresentAnotherCitizen = acp.GetByName("RepresentAnotherCitizen")!;
        Acp.RepresentedUnableToConsent = acp.GetByName("RepresentedUnableToConsent")!;
        Acp.NotObligedToConsent = acp.GetByName("NotObligedToConsent")!;
        Acp.ExcplicitConsent = acp.GetByName("ExcplicitConsent")!;
        Acp.UnableToConsent = acp.GetByName("UnableToConsent")!;
        Acp.ExceptionToConcent = acp.GetByName("ExceptionToConcent")!;
        Acp.HasConsent = acp.GetByName("HasConsent")!;
    }

    public string[] GetAllowedMimeTypes() => 
    [
        Constants.MimeTypes.Pdf,
        Constants.MimeTypes.Jpeg,
        Constants.MimeTypes.Png,
        Constants.MimeTypes.Tiff,
        Constants.MimeTypes.Gif,
        Constants.MimeTypes.Xml,
        Constants.MimeTypes.XmlReadable,
        Constants.MimeTypes.Text,
        Constants.MimeTypes.TextRtf,
    ];

    public HashSet<(string Code, string CodeSystem)?> GetCitizenObfuscationCodes()
    {
        var confidentialityCodeSystems = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Xds.ConfidentialityCode);

        return
        [
            _terminologyService.GetValueFromCodeSystem(confidentialityCodeSystems, "V").AsTuple(),
            _terminologyService.GetValueFromCodeSystem(confidentialityCodeSystems, "NORN_ANG").AsTuple(),
        ];
    }

    public HashSet<(string Code, string CodeSystem)?> GetHealthcarePersonellObfuscationCodes()
    {
        var confidentialityCodeSystems = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Xds.ConfidentialityCode);

        return
        [
            _terminologyService.GetValueFromCodeSystem(confidentialityCodeSystems, "NORS").AsTuple(),
        ];
    }

    public (string, string)[] GetCitizenConfidentialityCodesToObfuscate()
    {
        return GetCitizenObfuscationCodes().Select(c => (c?.Code, c?.CodeSystem)).ToArray()!;
    }

    public (string, string)[] GetHealthcarePersonellConfidentialityCodesToObfuscate()
    {
        return GetHealthcarePersonellObfuscationCodes().Select(c => (c?.Code, c?.CodeSystem)).ToArray()!;
    }

    public ComprehensiveCodeSystem[] GetAllowedOrganizationSystems()
    {
        var organizationSystems = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Other.OrganizationAssigningAuthorities);
        return [.. organizationSystems];
    }

    public ComprehensiveCodeSystem[] GetAllowedPractitionerSystems()
    {
        var practitionerSystems = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Other.PractitionerAssigningAuthorities);
        return [.. practitionerSystems];
    }

    public ComprehensiveCodeSystem[] GetAllowedPatientSystems()
    {
        var patientSystems = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Other.PersonAssigningAuthorities);
        return [.. patientSystems];
    }

    public ComprehensiveCodeSystem[] GetAllowedFacilityTypes()
    {
        var facilityTypes = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Xds.FacilityType);
        return [.. facilityTypes];
    }

    public ComprehensiveCodeSystem[] GetAllowedPracticeSettings()
    {
        var practiceSettings = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Xds.PracticeSettingCode);
        return [.. practiceSettings];
    }

    public ComprehensiveCodeSystem[] GetAllowedTypeCodes()
    {
        var typeCodes = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Xds.TypeCode);
        return [.. typeCodes];
    }

    public ComprehensiveCodeSystem[] GetAllowedClassCodes()
    {
        var classCodes = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Xds.ClassCode);
        return [.. classCodes];
    }

    public ComprehensiveCodeSystem[] GetAllowedConfidentialityCodes()
    {
        var confidentialityCodeSystems = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Xds.ConfidentialityCode);
        return [.. confidentialityCodeSystems];
    }

    public ComprehensiveCodeSystem[] GetAllowedFormatCodes()
    {
        var formatCodes = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Xds.FormatCode);
        return [.. formatCodes];
    }

    public ComprehensiveCodeSystem[] GetAllowedAttachments()
    {
        var attachments = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Hl7.Attachments);
        return [.. attachments];
    }

    public static bool IsMatchingMimeType(string? mimeTypeFromMagicByte, string? documentEntryMimeType)
    {
        // HAYO! If the Mime Type is XmlReadable it can be a document wrapped in CDA, the magic byte check will say its CDA,
        // but the actual document inside will be the Mime Type from the DocumentEntry...
        // Maybe do something to detect it in the future?
        if (mimeTypeFromMagicByte == Constants.MimeTypes.XmlReadable)
        {
            return true;
        }

        // Special handling for XML mimetypes, as there can be many valid XML mimetypes that are not explicitly listed in the allowed mimetypes,
        // but should still be considered valid if the document entry mimetype indicates it's an XML type
        // and the actual magic byte mimetype also indicates it's an XML type.
        if (documentEntryMimeType?.Contains("/xml") == true || documentEntryMimeType?.Contains("+xml") == true)
        {
            if (mimeTypeFromMagicByte?.Contains("/xml") == true || mimeTypeFromMagicByte?.Contains("+xml") == true)
            {
                return true;
            }
        }

        return mimeTypeFromMagicByte == documentEntryMimeType;
    }

    public Dictionary<string, BusinessRule<IdentifiableType>> AllBusinessRules { get; set; }

    public bool CitizenShouldSeeOwnDocumentReferences(BusinessLogicParameters logic)
    {
        var hasRequiredAttributes =
             logic.Resource != null &&
             logic.Subject != null &&
             logic.Purpose != null &&
             logic.Purpose.Code != null;


        if (hasRequiredAttributes) {
            return logic.Resource.Code == logic.Subject.Code &&
                logic.Resource.CodeSystem == logic.Subject.CodeSystem &&
                logic.Purpose.Code.IsAnyOf(PATRQT, SubjectOfCare_13) &&
                logic.Acp.NoUrn() == Acp.NullValue.NoUrn() &&
                logic.SubjectAge >= 18;
        }

        return false;
    }

    public Dictionary<string, BusinessRule<IdentifiableType>> GetAllBusinessRulesForFilteringDocumentList()
    {
        return new()
        {
            /// <summary>
            /// Jeg som innbygger (voksen) skal se alle mine egne dokumentreferanser; og ha tilgang til mine egne dokumenter
            /// </summary>
            {
                "CitizenShouldSeeOwnDocumentReferences", new()
                {
                    Condition = logic => CitizenShouldSeeOwnDocumentReferences(logic),

                    Filter = robjs =>
                        FilterByConfidentiality(
                            robjs,
                            allowedLevels: Strings(Normal, Restricted),
                            disallowedLevels: Strings(VeryRestricted))
                }
            },

            /// <summary>
            /// Jeg som innbygger (barn) med alder mellom 12-16 skal ikke ha tilgang til dokumentreferanser/dokumenter
            /// </summary>
            {
                "CitizenBetween12And16ShouldNotSeeDocumentReferences",new()
                {
                    Condition = logic =>
                        logic.Subject != null &&
                        logic.Resource != null &&
                        logic.Purpose != null &&
                        logic.Purpose.Code != null &&

                        logic.Subject.Code == logic.Resource.Code &&
                        logic.Purpose.Code.IsAnyOf(PATRQT, SubjectOfCare_13) &&
                        logic.SubjectAge.InRange(12, 16),

                    Filter = _ => DenyAll()
                }
            },

            /// <summary>
            /// Jeg som innbygger (ungdom) med alder mellom 16-18 skal ha tilgang til til deler av dokumentreferanser/dokumenter
            /// </summary>
            {
                "CitizenBetween16And18ShouldAccessPartsOfDocumentReferences", new()
                {

                    Condition = logic =>
                        logic.Subject != null &&
                        logic.Resource != null &&
                        logic.Purpose != null &&
                        logic.Purpose.Code != null &&

                        logic.Subject.Code == logic.Resource.Code &&
                        logic.Purpose.Code.IsAnyOf(PATRQT, SubjectOfCare_13) &&
                        logic.SubjectAge.InRange(16, 18),

                    Filter = robjs =>
                        FilterByConfidentiality(
                            robjs,
                            allowedLevels: Strings(Normal, Restricted),
                            disallowedLevels: Strings(VeryRestricted))
                }
            },

            /// <summary>
            /// Jeg som innbygger skal se dokumentreferanser/dokumenter for mine barn under 12 år
            /// </summary>
            {
                "CitizenShouldSeeChildrenBelow12DocumentReferences",new ()
                {
                    Condition = logic =>
                        logic.Subject != null &&
                        logic.Resource != null &&
                        logic.Purpose != null &&
                        logic.Purpose.Code != null &&

                        logic.Subject.Code != logic.Resource.Code &&
                        logic.Purpose.Code.IsAnyOf(FAMRQT, SubjectOfCare_13) &&
                        logic.Acp == Acp.RepresentCitizenUnder12 &&
                        logic.ResourceAge <= 12,

                    Filter = robjs =>
                        FilterByConfidentiality(
                            robjs,
                            allowedLevels: Strings(Normal, Restricted),
                            disallowedLevels: Strings(VeryRestricted))
                }
            },

            /// <summary>
            /// Jeg som innbygger skal se dokumentreferanser/dokumenter til den som jeg har representasjonsforhold for
            /// </summary>
            {
                "CitizenShouldSeePowerOfAttorneyDocumentReferences",new ()
                {
                    Condition = logic =>
                        logic.Subject != null &&
                        logic.Resource != null &&
                        logic.Subject.Code != null &&
                        logic.Resource.Code != null &&
                        logic.Acp != null &&
                        logic.Purpose != null &&
                        logic.Purpose.Code != null &&

                        logic.Subject.Code != logic.Resource.Code &&
                        logic.Purpose.Code.IsAnyOf(PWATRNY, SubjectOfCare_13) &&
                        logic.Acp.IsAnyOf(Acp.RepresentAnotherCitizen, Acp.RepresentedUnableToConsent) &&
                        !logic.SubjectAge.InRange(12, 16),

                    Filter = robjs =>
                        FilterByConfidentiality(
                            robjs,
                            allowedLevels: Strings(Normal, Restricted),
                            disallowedLevels: Strings(VeryRestricted))

                }
            },

            /// <summary>
            /// Jeg som innbygger skal IKKE se dokumentreferanser/dokumenter til den som jeg IKKE har representasjonsforhold eller foreldreansvar for
            /// </summary>
            {
                "CitizenShouldNotSeeNonPowerOfAttorneyDocumentReferences", new ()
                {
                    Condition = logic =>
                        logic.Subject != null &&
                        logic.Resource != null &&
                        logic.Purpose != null &&
                        logic.Purpose.Code != null &&

                        logic.Subject.Code != logic.Resource.Code &&
                        logic.Acp == Acp.NullValue &&
                        logic.Purpose.Code.IsAnyOf(PATRQT, FAMRQT, PWATRNY, SubjectOfCare_13),

                    Filter = _ => DenyAll()
                }
            },

            /// <summary>
            /// Jeg som innbygger skal IKKE se dokumentreferanser/dokumenter til mitt barn som er 13 år eller eldre
            /// </summary>
            {
                "CitizenShouldNotAccessDocumentsForPatientOver12", new()
                {
                    Condition = logic =>
                        logic.Subject != null &&
                        logic.Resource != null &&
                        logic.Purpose != null &&
                        logic.Purpose.Code != null &&

                        logic.Subject.Code != logic.Resource.Code &&
                        logic.Acp == Acp.RepresentCitizenUnder12 &&
                        logic.Purpose.Code.IsAnyOf(PATRQT, FAMRQT, PWATRNY, SubjectOfCare_13) &&
                        logic.ResourceAge >= 13,

                    Filter = _ => DenyAll()
                }
            },

            /// <summary>
            /// Jeg som helsepersonell skal se alle mine egne dokumentreferanser; og ha tilgang til mine egne dokumenter
            /// </summary>
            {
                "HealthcarePersonellShouldSeeOwnDocumentReferences", new()
                {
                    Condition = logic =>
                        logic.Subject != null &&
                        logic.Resource != null &&
                        logic.Purpose != null &&
                        logic.Purpose.Code != null &&

                        logic.Subject.Code == logic.Resource.Code &&
                        logic.Acp == Acp.NullValue &&
                        logic.Purpose.Code.IsAnyOf(TREAT, CAREMGT, ClinicalCare_1, Management_5),

                    Filter = robjs =>
                        FilterByConfidentiality(
                            robjs,
                            allowedLevels: Strings(Normal, Restricted),
                            disallowedLevels: Strings(VeryRestricted))
                }
            },

            /// <summary>
            /// Jeg som helsepersonell skal se alle dokumentreferanser/dokumenter for en pasient med relasjon til virksomheten som jeg representerer i en normal situasjon
            /// </summary>
            {
                "HealthcarePersonellShouldSeeRelatedPatientDocumentReferences", new()
                {
                    Condition = logic =>
                        logic.Subject != null &&
                        logic.Resource != null &&
                        logic.Purpose != null &&
                        logic.Purpose.Code != null &&
                        logic.Scope != null &&

                        logic.Subject.Code != logic.Resource.Code &&
                        logic.Scope.Contains("journaldokumenter_helsepersonell") &&
                        logic.Purpose.Code.IsAnyOf(TREAT, CAREMGT, ClinicalCare_1, Management_5),

                    Filter = robjs =>
                        FilterByConfidentiality(
                            robjs,
                            allowedLevels: Strings(Normal, Restricted, VeryRestricted))
                }
            },

            /// <summary>
            /// Jeg som helsepersonell skal se alle dokumentreferanser/dokumenter for en pasient med relasjon til virksomheten som jeg representerer i en akutt situasjon
            /// </summary>
            {
                "HealthcarePersonellShouldSeeEmergencyRelatedPatientDocumentReferences", new()
                {
                    Condition = logic =>
                        logic.Subject != null &&
                        logic.Resource != null &&
                        logic.Purpose != null &&
                        logic.Purpose.Code != null &&

                        logic.Subject.Code != logic.Resource.Code &&
                        logic.Purpose.Code.IsAnyOf(ETREAT, EmergencyCare_2),

                    Filter = robjs =>
                        FilterByConfidentiality(
                            robjs,
                            allowedLevels: Strings(Normal, Restricted, VeryRestricted))
                }
            },

            /// <summary>
            /// Jeg som helsepersonell som representerer en helsevirksomhet skal ikke se noen dokumentreferanser/dokumenter dersom det mangler viktige elementer som f.eks. korrekt angitt Purpose of Use
            /// </summary>
            {
                "HealthcarePersonellWithMissingAttributesShouldNotSeeDocumentReferences", new()
                {
                    Condition = logic => HealthcarePersonellHasMissingAttributes(logic),

                    Filter = _ => DenyAll()
                }
            },

            ///// <summary>
            ///// Filter according to Kjernejournalforskriften <para/>
            ///// <code>
            ///// Category | Dokumentgruppe               | Visning i KJ | Eksempler på dokumenter
            ///// ====================================================================================
            ///// A00-1    | Epikriser og sammenfatninger | Ubegrenset   | Epikriser etter innleggelse, poliklinikk m.m.
            ///// C00-1    | Prøvesvar, vev og vesker     | Siste 1 år   | Medisinks biokjemi, patologi m.m.
            ///// D00-1    | Organfunksjon                | Siste 5 år   | Ultralyd av hjerte, spirometri m.m.
            ///// E00-1    | Bildediagnostikk             | Siste 5 år   | Radiologi, ultralyd m.m.
            ///// I00-1    | Korrespondanse               | Siste 1 år   | Henvisninger
            ///// </code>
            ///// </summary>
            //{
            //    "HealthcarePersonellKjernejournalForskriften", new()
            //    {
            //        Condition = logic =>
            //            logic.Scope != null &&
            //            logic.Scope.Length > 0 &&

            //            logic.AppliesTo == AppliesTo.HealthcarePersonell &&
            //        // HAYO! KJ_SCOPE As of march 2026, PHR has not defined a specific scope for Kjernejournalforskriften,
            //        // For now, a bogus value of "kjernejournalforskriften" in the scope as an indicator that this filter should be applied.
            //            logic.Scope.Contains("kjernejournalforskriften"),

            //        Filter = robjs => FilterByKjernejournalForskriften(robjs)
            //    }
            //}
        };
    }

    private static bool HealthcarePersonellHasMissingAttributes(BusinessLogicParameters logic)
    {
        var hasRequiredObjects =
            logic.Subject != null &&
            logic.Resource != null &&
            logic.Purpose != null &&
            logic.SubjectOrganization != null;

        if (!hasRequiredObjects)
            return false;

        var subjectCodeMissing = logic.Subject!.Code == null;
        var resourceCodeMissing = logic.Resource!.Code == null;
        var subjectOrgCodeMissing = logic.SubjectOrganization!.Code == null;
        var purposeMissingOrInvalid =
            logic.Purpose!.Code == null ||
            !logic.Purpose.Code.IsAnyOf(
                TREAT, ETREAT, COC, BTG, PATRQT, FAMRQT, PWATRNY,
                ClinicalCare_1, EmergencyCare_2, Management_5, SubjectOfCare_13);

        return subjectCodeMissing || resourceCodeMissing || subjectOrgCodeMissing || purposeMissingOrInvalid;
    }



    public static IEnumerable<IdentifiableType> FilterByKjernejournalForskriften(IEnumerable<IdentifiableType> source)
    {
        var now = DateTimeOffset.Now;

        var oneYearAgo = now.AddYears(-1);
        var fiveYearsAgo = now.AddYears(-5);

        var sourceAsList = source.OfType<ExtrinsicObjectType>().ToList();

        var provesvarDocuments = GetVolvenDocumentsByCategory(sourceAsList, KjForskriftCategoryCodes.ProvesvarVevOgVaesker);
        var organDocuments = GetVolvenDocumentsByCategory(sourceAsList, KjForskriftCategoryCodes.Organfunksjon);
        var bildeDocuments = GetVolvenDocumentsByCategory(sourceAsList, KjForskriftCategoryCodes.BildediagnostikkOgAndreMedisinskeBilder);
        var korrespondanseDocuments = GetVolvenDocumentsByCategory(sourceAsList, KjForskriftCategoryCodes.Korrespondanse);

        var removeProvesvar = provesvarDocuments.Where(document => document.GetServiceStartTime() < oneYearAgo).ToList();
        var removeOrgan = organDocuments.Where(document => document.GetServiceStartTime() < fiveYearsAgo).ToList();
        var removeBilder = bildeDocuments.Where(document => document.GetServiceStartTime() < fiveYearsAgo).ToList();
        var removeKorrespondanse = korrespondanseDocuments.Where(document => document.GetServiceStartTime() < oneYearAgo).ToList();

        sourceAsList.RemoveAll(doc => removeProvesvar.Any(remove => remove.Id == doc.Id));
        sourceAsList.RemoveAll(doc => removeOrgan.Any(remove => remove.Id == doc.Id));
        sourceAsList.RemoveAll(doc => removeBilder.Any(remove => remove.Id == doc.Id));
        sourceAsList.RemoveAll(doc => removeKorrespondanse.Any(remove => remove.Id == doc.Id));
        return sourceAsList;
    }

    // HAYO! This is a a straightforward way to implement it, but principally speaking
    // it should really be factored out as a subset of the 9602 category codes in the terminology service
    private static class KjForskriftCategoryCodes
    {
        public const string System = "2.16.578.1.12.4.1.1.9602";
        public const string EpikriserOgSammenfatninger = "A00-1";
        public const string KontinuerligLopendeJournal = "B00-1";
        public const string ProvesvarVevOgVaesker = "C00-1";
        public const string Organfunksjon = "D00-1";
        public const string BildediagnostikkOgAndreMedisinskeBilder = "E00-1";
        public const string KurveObservasjonOgBehandling = "F00-1";
        public const string Korrespondanse = "I00-1";
        public const string AttesterMeldingOgErklaeringer = "J00-1";
        public const string TestOgScoring = "S00-1";
    }


    private static bool CodingMatches(CodedValue? coding, CodedValue searchCoding)
    {
        if (coding == null)
            return false;

        var systemMatches = searchCoding.CodeSystem == null || coding.CodeSystem == searchCoding.CodeSystem;
        var codeMatches = searchCoding.Code == null || coding.Code == searchCoding.Code;

        return systemMatches && codeMatches;
    }

    private static List<ExtrinsicObjectType> GetVolvenDocumentsByCategory(List<ExtrinsicObjectType> documents, string categoryCode)
    {
        var categories = documents
            .Where(document =>
                document.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ClassCode)
                .Select(RegistryMetadataTransformer.MapClassificationToCodedValue)
                .Any(coding => CodingMatches(coding, new CodedValue(categoryCode, KjForskriftCategoryCodes.System))))
            .ToList();

        return categories;
    }

    public static IEnumerable<IdentifiableType> FilterByConfidentiality(IEnumerable<IdentifiableType> source, string[] allowedLevels, string[]? disallowedLevels = null)
    {
        foreach (var registryObject in source)
        {
            if (registryObject is ExtrinsicObjectType extrinsicObject)
            {
                var classifications = extrinsicObject.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode);

                if (allowedLevels == null || allowedLevels.Length == 0 || classifications.Length == 0)
                {
                    yield break;
                }

                // Requirements:
                // - At least 1 classification must match any in allowedLevels
                // - All classifications must not have any in disallowedLevels
                // - Classifications can contain other codes not in allowedLevels or disallowedLevels
                var hasAllowed = classifications.Any(cc => cc?.NodeRepresentation != null && allowedLevels?.Contains(cc.NodeRepresentation) == true);
                var hasDisallowed = classifications.Any(cc => cc?.NodeRepresentation != null && disallowedLevels?.Contains(cc.NodeRepresentation) == true);

                if (hasAllowed && !hasDisallowed)
                {
                    yield return registryObject;
                }
            }
        }
    }

    private static IEnumerable<IdentifiableType> DenyAll() { return []; }


    /// <summary>
    /// Collection expressions are not directly supported in Expression Trees...
    /// </summary>
    public static string[] Strings(params string[] items)
    {
        return items;
    }
}