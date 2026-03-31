using Hl7.Fhir.Model;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.BusinessLogic;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;
using static XcaXds.Commons.Commons.Constants.CodeSystems.Hl7.ConfidentialityCode;
using static XcaXds.Commons.Commons.Constants.CodeSystems.Hl7.PurposeOfUse;
using static XcaXds.Commons.Commons.Constants.CodeSystems.OtherIsoDerived.PurposeOfUse;
using static XcaXds.Commons.Commons.Constants.CodeSystems.Volven.ConfidentialityCode_9603;

namespace XcaXds.Commons.DataManipulators.BusinessLogic;

public static class BusinessLogicFilters
{
    public static readonly ComprehensiveCodeSystem VolvenDocumentTypes = typeof(Constants.CodeSystems.Volven.CategoryCode_9602).GetAsComprehensiveCodesystem();

    public static readonly Dictionary<string, string> Hl7ConfCodeClass = ConstantsExtensions.GetAsDictionary(typeof(Constants.CodeSystems.Hl7.ConfidentialityCode));
    public static readonly string? Hl7ConfCodeOid = Hl7ConfCodeClass.Where(kvp => string.Equals(kvp.Key, "System", StringComparison.InvariantCultureIgnoreCase)).Select(kvp => kvp.Value).FirstOrDefault() ?? string.Empty;
    public static readonly CodedValue[]? Hl7ConfCodeValues = [.. Hl7ConfCodeClass.Where(kvp => !string.Equals(kvp.Key, "System", StringComparison.InvariantCultureIgnoreCase)).Select(kvp => new CodedValue() { Code = kvp.Value, CodeSystem = Hl7ConfCodeOid })];

    public static readonly Dictionary<string, string> VolvenConfCodeClass = ConstantsExtensions.GetAsDictionary(typeof(Constants.CodeSystems.Volven.ConfidentialityCode_9603));
    public static readonly string? VolvenConfCodeOid = VolvenConfCodeClass.Where(kvp => string.Equals(kvp.Key, "System", StringComparison.InvariantCultureIgnoreCase)).Select(kvp => kvp.Value).FirstOrDefault() ?? string.Empty;
    public static readonly CodedValue[]? VolvenConfCodeValues = [.. VolvenConfCodeClass.Where(kvp => !string.Equals(kvp.Key, "System", StringComparison.InvariantCultureIgnoreCase)).Select(kvp => new CodedValue() { Code = kvp.Value, CodeSystem = VolvenConfCodeOid })];

    public static readonly HashSet<(string Code, string CodeSystem)> AllConfidentialityCodes = [.. Hl7ConfCodeValues.Concat(VolvenConfCodeValues).Select(val => (val.Code!, val.CodeSystem!))];

    private static readonly HashSet<(string Code, string CodeSystem)> CitizenObfuscationCodes =
    [
        (VeryRestricted, Constants.CodeSystems.Hl7.ConfidentialityCode.System),
        (NORN_ANG, Constants.CodeSystems.Volven.ConfidentialityCode_9603.System)
    ];

    private static readonly HashSet<(string Code, string CodeSystem)> HealthcarePersonellObfuscationCodes =
    [
        (NORS, Constants.CodeSystems.Volven.ConfidentialityCode_9603.System)
    ];

    public static readonly List<(string, string)> CitizenConfidentialityCodesToObfuscate = [.. AllConfidentialityCodes.Where(CitizenObfuscationCodes.Contains)];
    public static readonly List<(string, string)> HealthcarePersonellConfidentialityCodesToObfuscate = [.. AllConfidentialityCodes.Where(HealthcarePersonellObfuscationCodes.Contains)];

    /// <summary>
    /// Jeg som innbygger (voksen) skal se alle mine egne dokumentreferanser; og ha tilgang til mine egne dokumenter
    /// </summary>
    public static BusinessRule<IdentifiableType> CitizenShouldSeeOwnDocumentReferences { get; set; } = new()
    {
        Name = nameof(CitizenShouldSeeOwnDocumentReferences),

        Condition = logic =>
            logic.Resource != null &&
            logic.Subject != null &&
            logic.QueriedSubject != null &&
            logic.Purpose != null &&
            logic.Purpose.Code != null &&

            logic.Resource.Code == logic.Subject.Code &&
            logic.Resource.CodeSystem == logic.Subject.CodeSystem &&
            logic.QueriedSubject.Code == logic.Subject.Code &&
            logic.QueriedSubject.CodeSystem == logic.Subject.CodeSystem &&

            logic.Purpose.Code.IsAnyOf(PATRQT, SubjectOfCare_13) &&
            logic.Acp == Constants.Oid.Saml.Acp.NullValue &&
            logic.SubjectAge >= 18,

        Filter = robjs =>
            FilterByConfidentiality(
                robjs,
                allowedLevels: Strings(Normal, Restricted),
                disallowedLevels: Strings(VeryRestricted))
    };


    /// <summary>
    /// Jeg som innbygger (barn) med alder mellom 12-16 skal ikke ha tilgang til dokumentreferanser/dokumenter
    /// </summary>
    public static BusinessRule<IdentifiableType> CitizenBetween12And16ShouldNotSeeDocumentReferences { get; set; } = new()
    {
        Name = nameof(CitizenBetween12And16ShouldNotSeeDocumentReferences),

        Condition = logic =>
            logic.Subject != null &&
            logic.Resource != null &&
            logic.Purpose != null &&
            logic.Purpose.Code != null &&

            logic.Subject.Code == logic.Resource.Code &&
            logic.Purpose.Code.IsAnyOf(PATRQT, SubjectOfCare_13) &&
            logic.SubjectAge.InRange(12, 16),

        Filter = _ => DenyAll()
    };

    /// <summary>
    /// Jeg som innbygger (ungdom) med alder mellom 16-18 skal ha tilgang til til deler av dokumentreferanser/dokumenter
    /// </summary>
    public static BusinessRule<IdentifiableType> CitizenBetween16And18ShouldAccesPartsOfDocumentReferences = new()
    {
        Name = nameof(CitizenBetween16And18ShouldAccesPartsOfDocumentReferences),

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
    };

    /// <summary>
    /// Jeg som innbygger skal se dokumentreferanser/dokumenter for mine barn under 12 år
    /// </summary>
    public static BusinessRule<IdentifiableType> CitizenShouldSeeChildrenBelow12DocumentReferences { get; set; } = new()
    {
        Name = nameof(CitizenShouldSeeChildrenBelow12DocumentReferences),

        Condition = logic =>
            logic.Subject != null &&
            logic.Resource != null &&
            logic.Purpose != null &&
            logic.Purpose.Code != null &&

            logic.Subject.Code != logic.Resource.Code &&
            logic.Purpose.Code.IsAnyOf(FAMRQT, SubjectOfCare_13) &&
            logic.Acp == Constants.Oid.Saml.Acp.RepresentCitizenUnder12 &&
            logic.SubjectAge < 12,

        Filter = robjs =>
            FilterByConfidentiality(
                robjs,
                allowedLevels: Strings(Normal, Restricted),
                disallowedLevels: Strings(VeryRestricted))
    };

    /// <summary>
    /// Jeg som innbygger skal se dokumentreferanser/dokumenter til den som jeg har representasjonsforhold for
    /// </summary>
    public static BusinessRule<IdentifiableType> CitizenShouldSeePowerOfAttorneyDocumentReferences { get; set; } = new()
    {
        Name = nameof(CitizenShouldSeePowerOfAttorneyDocumentReferences),

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
            logic.Acp.IsAnyOf(Constants.Oid.Saml.Acp.RepresentAnotherCitizen, Constants.Oid.Saml.Acp.RepresentedUnableToConsent) &&
            !logic.SubjectAge.InRange(12, 16),

        Filter = robjs =>
            FilterByConfidentiality(
                robjs,
                allowedLevels: Strings(Normal, Restricted),
                disallowedLevels: Strings(VeryRestricted))

    };

    /// <summary>
    /// Jeg som innbygger skal IKKE se dokumentreferanser/dokumenter til den som jeg IKKE har representasjonsforhold eller foreldreansvar for
    /// </summary>
    public static BusinessRule<IdentifiableType> CitizenShouldNotSeeNonPowerOfAttorneyDocumentReferences { get; set; } = new()
    {
        Name = nameof(CitizenShouldNotSeeNonPowerOfAttorneyDocumentReferences),

        Condition = logic =>
            logic.Subject != null &&
            logic.Resource != null &&
            logic.Purpose != null &&
            logic.Purpose.Code != null &&

            logic.Subject.Code != logic.Resource.Code &&
            logic.Acp == Constants.Oid.Saml.Acp.NullValue &&
            logic.Purpose.Code.IsAnyOf(PATRQT, FAMRQT, PWATRNY, SubjectOfCare_13),

        Filter = _ => DenyAll()
    };

    /// <summary>
    /// Jeg som helsepersonell skal se alle mine egne dokumentreferanser; og ha tilgang til mine egne dokumenter
    /// </summary>
    public static BusinessRule<IdentifiableType> HealthcarePersonellShouldSeeOwnDocumentReferences { get; set; } = new()
    {
        Name = nameof(HealthcarePersonellShouldSeeOwnDocumentReferences),

        Condition = logic =>
            logic.Subject != null &&
            logic.Resource != null &&
            logic.Purpose != null &&
            logic.Purpose.Code != null &&

            logic.Subject.Code == logic.Resource.Code &&
            logic.Acp == Constants.Oid.Saml.Acp.NullValue &&
            logic.Purpose.Code.IsAnyOf(TREAT, CAREMGT, ClinicalCare_1, Management_5),

        Filter = robjs =>
            FilterByConfidentiality(
                robjs,
                allowedLevels: Strings(Normal, Restricted),
                disallowedLevels: Strings(VeryRestricted))
    };

    /// <summary>
    /// Jeg som helsepersonell skal se alle dokumentreferanser/dokumenter for en pasient med relasjon til virksomheten som jeg representerer i en normal situasjon
    /// </summary>
    public static BusinessRule<IdentifiableType> HealthcarePersonellShouldSeeRelatedPatientDocumentReferences { get; set; } = new()
    {
        Name = nameof(HealthcarePersonellShouldSeeRelatedPatientDocumentReferences),

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
    };

    /// <summary>
    /// Jeg som helsepersonell skal se alle dokumentreferanser/dokumenter for en pasient med relasjon til virksomheten som jeg representerer i en akutt situasjon
    /// </summary>
    public static BusinessRule<IdentifiableType> HealthcarePersonellShouldSeeEmergencyRelatedPatientDocumentReferences { get; set; } = new()
    {
        Name = nameof(HealthcarePersonellShouldSeeEmergencyRelatedPatientDocumentReferences),

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
    };

    /// <summary>
    /// Jeg som helsepersonell som representerer en helsevirksomhet skal ikke se noen dokumentreferanser/dokumenter dersom det mangler viktige elementer som f.eks. korrekt angitt Purpose of Use
    /// </summary>
    public static BusinessRule<IdentifiableType> HealthcarePersonellWithMissingAttributesShouldNotSeeDocumentReferences { get; set; } = new()
    {
        Name = nameof(HealthcarePersonellWithMissingAttributesShouldNotSeeDocumentReferences),
        Condition = logic =>
            logic.Subject != null &&
            logic.Resource != null &&
            logic.Purpose != null &&
            logic.SubjectOrganization != null &&

            logic.Subject.Code == null ||
            logic.Resource!.Code == null ||
            logic.SubjectOrganization!.Code == null ||
            !logic.Purpose!.Code!.IsAnyOf(TREAT, ETREAT, COC, BTG, PATRQT, FAMRQT, PWATRNY, ClinicalCare_1, EmergencyCare_2, Management_5, SubjectOfCare_13),

        Filter = _ => DenyAll()
    };

    /// <summary>
    /// Filter according to Kjernejournalforskriften <para/>
    /// <code>
    /// Category | Dokumentgruppe               | Visning i KJ | Eksempler på dokumenter
    /// ====================================================================================
    /// A00-1    | Epikriser og sammenfatninger | Ubegrenset   | Epikriser etter innleggelse, poliklinikk m.m.
    /// C00-1    | Prøvesvar, vev og vesker     | Siste 1 år   | Medisinks biokjemi, patologi m.m.
    /// D00-1    | Organfunksjon                | Siste 5 år   | Ultralyd av hjerte, spirometri m.m.
    /// E00-1    | Bildediagnostikk             | Siste 5 år   | Radiologi, ultralyd m.m.
    /// I00-1    | Korrespondanse               | Siste 1 år   | Henvisninger
    /// </code>
    /// </summary>
    public static BusinessRule<IdentifiableType> HealthcarePersonellKjernejournalForskriften { get; set; } = new()
    {
        Name = nameof(HealthcarePersonellKjernejournalForskriften),

        Condition = logic =>
            logic.Scope != null &&
            logic.Scope.Length > 0 &&

            logic.Issuer == Issuer.HelseId &&
        // HAYO! KJ_SCOPE As of march 2026, PHR has not defined a specific scope for Kjernejournalforskriften,
        // For now, a bogus value of "kjernejournalforskriften" in the scope as an indicator that this filter should be applied.
            logic.Scope.Contains("kjernejournalforskriften"),

        Filter = robjs => FilterByKjernejournalForskriften(robjs)
    };


    public static IEnumerable<IdentifiableType> FilterByKjernejournalForskriften(IEnumerable<IdentifiableType> source)
    {
        var now = DateTimeOffset.Now;

        var oneYearAgo = now.AddYears(-1);
        var fiveYearsAgo = now.AddYears(-5);

        var sourceAsList = source.OfType<ExtrinsicObjectType>().ToList();

        var provesvarDocuments = GetVolvenDocumentsByCategory(sourceAsList, XcaDokumentKategoriKoder.ProvesvarVevOgVaesker);
        var organDocuments = GetVolvenDocumentsByCategory(sourceAsList, XcaDokumentKategoriKoder.Organfunksjon);
        var bildeDocuments = GetVolvenDocumentsByCategory(sourceAsList, XcaDokumentKategoriKoder.BildediagnostikkOgAndreMedisinskeBilder);
        var korrespondanseDocuments = GetVolvenDocumentsByCategory(sourceAsList, XcaDokumentKategoriKoder.Korrespondanse);

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
                .Any(coding => CodingMatches(coding, new CodedValue(categoryCode, VolvenDocumentTypes.System))))
            .ToList();

        return categories;
    }

    public static class XcaDokumentKategoriKoder
    {
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

    public static IEnumerable<IdentifiableType> FilterByConfidentiality(IEnumerable<IdentifiableType> source, string[] allowedLevels, string[]? disallowedLevels = null)
    {
        foreach (var registryObject in source)
        {
            if (registryObject is ExtrinsicObjectType extrinsicObject)
            {
                var classifications = extrinsicObject.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode);

                if (allowedLevels == null || allowedLevels.Length == 0)
                {
                    yield break;
                }

                // Requirements:
                // - At least 1 classification must match any in allowedLevels
                // - All classifications must not have any in disallowedLevels
                // - Classifications can contain other codes not in allowedLevels or disallowedLevels
                var hasAllowed = classifications.Any(cc => cc?.NodeRepresentation != null && allowedLevels.Contains(cc.NodeRepresentation));
                var hasDisallowed = classifications.Any(cc => cc?.NodeRepresentation != null && (disallowedLevels ?? []).Contains(cc.NodeRepresentation));

                if (hasAllowed && !hasDisallowed)
                {
                    yield return registryObject;
                }
            }
        }
    }

    private static IEnumerable<IdentifiableType> DenyAll() { return []; }

    public static bool InRange(this int input, int lower, int upper)
    {
        return input >= lower && input <= upper;
    }

    /// <summary>
    /// Collection expressions are not directly supported in Expression Trees...
    /// </summary>
    public static string[] Strings(params string[] items)
    {
        return items;
    }

}
