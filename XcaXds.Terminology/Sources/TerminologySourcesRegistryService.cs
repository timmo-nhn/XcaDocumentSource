using Microsoft.Extensions.Logging;
using XcaXds.Terminology.Models.Custom;
using XcaXds.Terminology.TerminologySources;
using XcaXds.Terminology.ValueSetMappers.Hl7;
using XcaXds.Terminology.ValueSetMappers.Norway;
using XcaXds.Terminology.ValueSetMappers.XcaDocumentSource;

namespace XcaXds.Terminology.Sources;

/// <summary>
/// This class defines a list of the sources of the Value sets that XcaDocumentSource will use
/// <para/>
/// Each source is defined by a code system name and a list of <see cref="TerminologySource{TMapper}"/> objects,
/// which include the source path and an implementation of the <see cref="ICodeSystemMapper"/>
/// that will be used to convert the content from the sourcePath to a ComprehensiveCodeSystem.
/// <para/>
/// The code systems can either be fetched from an API endpoint (<see cref="HttpTerminologySource"/>) or from a file (<see cref="FileTerminologySource"/>) or any other mechanism you can come up with :)
/// </summary>
public class TerminologySourcesRegistryService
{
    private readonly ILogger<TerminologySourcesRegistryService> _logger;
    private readonly ApplicationConfig _applicationConfig;
    private readonly FileTerminologySource _fileSource;
    private readonly HttpTerminologySource _httpSource;
    private readonly StringTerminologySource _stringSource;


    public TerminologySourcesRegistryService(
        ILogger<TerminologySourcesRegistryService> logger,
        ApplicationConfig applicationConfig,
        FileTerminologySource fileSource,
        HttpTerminologySource httpSource,
        StringTerminologySource stringSource)
    {
        _logger = logger;
        _applicationConfig = applicationConfig;
        _fileSource = fileSource;
        _httpSource = httpSource;
        _stringSource = stringSource;

        AddApplicationSpecificTerminologySources();
    }

    public List<TerminologySourceDefinition> GetAllDefinitions() => [.. GetTerminologySources_Norway(), .. GetTerminologySources_XdsHl7(), .. AddApplicationSpecificTerminologySources()];

    private List<TerminologySourceDefinition> AddApplicationSpecificTerminologySources() =>
    [
        new(CodeSystemNames.Hl7.Attachments,
        [
            new(_stringSource, _applicationConfig.HomeCommunityId, new StringBasedMapper(null, "https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-homeCommunityId")),
        ]),
    ];

    private List<TerminologySourceDefinition> GetTerminologySources_XdsHl7() =>
    [
        new(CodeSystemNames.Hl7.Attachments,
        [
            new(_fileSource, "Attachments.json", new FileBasedJsonMapper()),
        ]),

        new(CodeSystemNames.Xds.FormatCode,
        [
            new(_fileSource, "FormatCodes.json", new FileBasedJsonMapper()),
        ]),

        new(CodeSystemNames.Authentication.SamlAttributes,
        [
            new(_fileSource, "SamlAttributes.json", new FileBasedJsonMapper()),
        ]),

        new(CodeSystemNames.Authentication.PurposeOfUse,
        [
            new(_fileSource, "PurposeOfUse_Old.json", new FileBasedJsonMapper()),
            new (_httpSource, "https://terminology.hl7.org/2.1.0/CodeSystem-v3-ActReason.json", new Hl7FhirCodeSystemMapper())
        ]),

        new(CodeSystemNames.Other.OrganizationAssigningAuthorities,
        [
            new(_httpSource, "https://terminology.hl7.org/7.2.0/en/CodeSystem-organization-type.json", new Hl7FhirCodeSystemMapper()),
        ]),
    ];

    // Initial terminology implementation, for use in Norwegian eHealth
    private List<TerminologySourceDefinition> GetTerminologySources_Norway() =>
    [
        new(CodeSystemNames.Xds.FormatCode,
        [
            new(_fileSource, "No/FormatCodes.json", new FileBasedJsonMapper()),
        ]),

        new(CodeSystemNames.Xds.Gender,
        [
            new(_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/3101", new FinnKodeMapper()),

            // Example: Fallback to file based code system if running offline or external terminology service is unavailable
            // new("No/Genders.json", new FinnKodeMapper())
        ]),

        new(CodeSystemNames.Xds.ConfidentialityCode,
        [
            new (_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/9603", new FinnKodeMapper()),
            new (_httpSource, "https://terminology.hl7.org/7.1.0/en/CodeSystem-v3-Confidentiality.json", new Hl7FhirCodeSystemMapper("Confidentiality"))
        ]),

        new(CodeSystemNames.Xds.ClassCode,
        [
            new (_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/9602", new FinnKodeClassCodeMapper())
        ]),

        new(CodeSystemNames.Xds.TypeCode,
        [
            new (_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/9602", new FinnKodeTypeCodeMapper())
        ]),

        new(CodeSystemNames.Xds.EventCode,
        [
            new (_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/7210", new FinnKodeMapper())
        ]),

        new(CodeSystemNames.Xds.FacilityType,
        [
            new (_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/1303", new FinnKodeMapper()),
            new (_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/1305", new FinnKodeMapper())
        ]),

        new(CodeSystemNames.Xds.PracticeSettingCode,
        [
            new (_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/8651" ,new FinnKodeMapper()),
            new (_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/8653" ,new FinnKodeMapper()),
            new (_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/8654" ,new FinnKodeMapper()),
            new (_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/8655" ,new FinnKodeMapper()),
            new (_httpSource, "https://fat.kote.helsedirektoratet.no/api/code-systems/adm/codelist/8663" ,new FinnKodeMapper()),
        ]),

        new(CodeSystemNames.Other.OrganizationAssigningAuthorities,
        [
            new(_fileSource, "No/OrganizationAssigningAuthorities.json", new FileBasedJsonMapper()),
        ]),

        new(CodeSystemNames.Other.PersonAssigningAuthorities,
        [
            new(_fileSource, "No/PersonAssigningAuthorities.json", new FileBasedJsonMapper()),
        ]),

        new(CodeSystemNames.Other.PractitionerAssigningAuthorities,
        [
            new(_fileSource, "No/PractitionerAssigningAuthorities.json", new FileBasedJsonMapper()),
        ]),

        new(CodeSystemNames.Authentication.Acp,
        [
            new(_fileSource, "No/Acp.json", new FileBasedJsonMapper()),
        ]),

        new(CodeSystemNames.Authentication.Bppc,
        [
            new(_fileSource, "No/Bppc.json", new FileBasedJsonMapper()),
        ]),

        new(CodeSystemNames.Authentication.SamlAttributes,
        [
            new(_fileSource, "No/SamlAttributes_No.json", new FileBasedJsonMapper()),
        ])
    ];
}