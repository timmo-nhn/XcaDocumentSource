using XcaXds.Terminology.Mappers;
using XcaXds.Terminology.Models.Custom;
using XcaXds.Terminology.Services;
using XcaXds.Terminology.ValueSetMappers.Hl7;
using XcaXds.Terminology.ValueSetMappers.Norway;

namespace XcaXds.Terminology.Sources;

/// <summary>
/// This class defines the sources of the Value sets that XcaDocumentSource will use. (Mainly for validating incoming content before storing, aswell as for business logic)
/// <para/>
/// Each source is defined by a code system name and a list of <see cref="TerminologySource{TMapper}"/> objects,
/// which include the source path and an implementation of the <see cref="ICodeSystemMapper"/>
/// that will be used to convert the content from the sourcePath to a ComprehensiveCodeSystem.
/// <para/>
/// The code systems can either be fetched from an API endpoint (<see cref="HttpTerminologySource"/>) or from a file (<see cref="FileTerminologySource"/>) or any other mechanism you can come up with :)
/// </summary>
public static class TerminologySourcesService
{
    public static List<TerminologySourceDefinition> GetDefinitions() => Terminology_Norway;

    // Initial terminology implementation, for use in Norwegian eHealth
    public static List<TerminologySourceDefinition> Terminology_Norway =
    [
        new(TerminologyConstants.XdsCodeSystemNames.Gender,
        [
            new("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/3101", new FinnKodeMapper()),

            // Example: Fallback to file based code system if running offline or external terminology service is unavailable
            // new("OfflineCodeSystems/Genders.json", new FinnKodeMapper())
        ]),

        new(TerminologyConstants.XdsCodeSystemNames.ConfidentialityCode,
        [
            new ("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/9603", new FinnKodeMapper()),
            new ("https://terminology.hl7.org/7.1.0/en/CodeSystem-v3-Confidentiality.json", new Hl7FhirCodeSystemMapper())
        ]),

        new(TerminologyConstants.XdsCodeSystemNames.ClassCode,
        [
            new ("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/9602", new FinnKodeClassCodeMapper())
        ]),

        new(TerminologyConstants.XdsCodeSystemNames.TypeCode,
        [
            new ("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/9602", new FinnKodeTypeCodeMapper())
        ]),

        new(TerminologyConstants.XdsCodeSystemNames.EventCode,
        [
            new ("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/7210", new FinnKodeMapper())
        ]),

        new(TerminologyConstants.XdsCodeSystemNames.FacilityType,
        [
            new ("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/1303", new FinnKodeMapper()),
            new ("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/1305", new FinnKodeMapper())
        ]),
        new(TerminologyConstants.XdsCodeSystemNames.PracticeSettingCode,
        [
            new ("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/8651" ,new FinnKodeMapper()),
            new ("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/8653" ,new FinnKodeMapper()),
            new ("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/8654" ,new FinnKodeMapper()),
            new ("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/8655" ,new FinnKodeMapper()),
            new ("https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/8663" ,new FinnKodeMapper()),
        ]),
        new(TerminologyConstants.OtherCodeSystemNames.OrganizationAssigningAuthorities,
        [
            new("No/OrganizationAssigningAuthorities.json", new FileBasedJsonMapper()),
        ]),
        new(TerminologyConstants.AuthenticationCodeSystemNames.PurposeOfUse,
        [
            new("PurposeOfUse_Old.json", new FileBasedJsonMapper()),
        ]),
    ];
}