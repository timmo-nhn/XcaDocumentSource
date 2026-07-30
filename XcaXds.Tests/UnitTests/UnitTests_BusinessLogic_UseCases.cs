using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using XcaXds.BusinessLogic.Models.Custom;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared.Enums;
using static XcaXds.Tests.TestConstants.CodeSystems.Hl7.PurposeOfUse;
using static XcaXds.Tests.TestConstants.CodeSystems.OtherIsoDerived.PurposeOfUse;

namespace XcaXds.Tests.UnitTests;

public class UnitTests_BusinessLogic_UseCases : IntegrationTests_DefaultFixture, IClassFixture<WebApplicationFactory<WebService.Program>>
{
    private List<IdentifiableType> DocumentReferences = new();

    public UnitTests_BusinessLogic_UseCases(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : base(factory, output) { }

    [Fact]
    public async Task Citizen_1_ShouldOpenDocumentsOnThemself()
    {
        SetupTests();

        var patientId = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = SubjectOfCare_13, CodeSystem = TestConstants.CodeSystems.OtherIsoDerived.PurposeOfUse.System },
            Subject = new() { Code = patientId, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(patientId)?.GetAgeFromPatientId(patientId) ?? 0,
            Resource = new() { Code = patientId, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(patientId)?.GetAgeFromPatientId(patientId) ?? 0,
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(2, DocumentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_2_12To16_ShouldGetEmptyDocumentList()
    {
        SetupTests();
        var patientId12To16Years = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-13):yy}79740";

        var yearPart = DateTime.UtcNow.AddYears(-13).Year.ToString().Substring(2, 2);

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = patientId12To16Years, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(patientId12To16Years)?.GetAgeFromPatientId(patientId12To16Years) ?? 0,
            Resource = new() { Code = patientId12To16Years, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(patientId12To16Years)?.GetAgeFromPatientId(patientId12To16Years) ?? 0,
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Empty(DocumentReferences ?? []);
    }

    [Fact]
    public async Task Citizen_3_16To18_ShouldAccessPartsOfDocumentList()
    {
        SetupTests();

        var patientId16To18Years = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-17):yy}79740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = patientId16To18Years, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(patientId16To18Years)?.GetAgeFromPatientId(patientId16To18Years) ?? 0,
            Resource = new() { Code = patientId16To18Years, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(patientId16To18Years)?.GetAgeFromPatientId(patientId16To18Years) ?? 0,
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(2, DocumentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_4_ShouldAccessChildrenBelow12DocumentList()
    {
        SetupTests();

        var resourceBelow12Years = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-6):yy}79740";
        var subject = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.RepresentCitizenUnder12,
            Purpose = new() { Code = SubjectOfCare_13, CodeSystem = TestConstants.CodeSystems.OtherIsoDerived.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(subject)?.GetAgeFromPatientId(subject) ?? 0,
            Resource = new() { Code = resourceBelow12Years, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(resourceBelow12Years)?.GetAgeFromPatientId(resourceBelow12Years) ?? 0,
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();


        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(2, DocumentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_5_ShouldAccessPowerOfAttorneyDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-70):yy}39740";
        var subject = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.RepresentAnotherCitizen,
            Purpose = new() { Code = PWATRNY, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(subject)?.GetAgeFromPatientId(subject) ?? 0,
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(resource)?.GetAgeFromPatientId(resource) ?? 0,
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();


        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(2, DocumentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_6_ShouldNotAccessNonPowerOfAttorneyDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-70):yy}39740";
        var subject = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(subject)?.GetAgeFromPatientId(subject) ?? 0,
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(resource)?.GetAgeFromPatientId(resource) ?? 0,
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();


        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Empty(DocumentReferences ?? []);
    }

    [Fact]
    public async Task Citizen_7_ShouldNotAccessDocumentsForPatientOver12()
    {
        SetupTests();

        var resource = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-13):yy}39740";
        var subject = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-42):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.RepresentCitizenUnder12,
            Purpose = new() { Code = SubjectOfCare_13, CodeSystem = TestConstants.CodeSystems.OtherIsoDerived.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(subject)?.GetAgeFromPatientId(subject) ?? 0,
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(resource)?.GetAgeFromPatientId(resource) ?? 0,
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();


        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Empty(DocumentReferences ?? []);
    }

    [Fact]
    public async Task HealthcarePersonell_7_ShouldAccessTheirOwnDocumentList()
    {
        SetupTests();

        var subject = $"{DateTime.UtcNow.AddDays(-1):dd}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = TREAT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(subject)?.GetAgeFromPatientId(subject) ?? 0,
            Resource = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(subject)?.GetAgeFromPatientId(subject) ?? 0,
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();


        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(2, DocumentReferences?.Count);
    }

    [Fact]
    public async Task HealthcarePersonell_8_ShouldAccessPatientsDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.UtcNow.AddDays(-1).Day}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-70):yy}39740";
        var subject = $"{DateTime.UtcNow.AddDays(-1).Day}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = TREAT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(subject)?.GetAgeFromPatientId(subject) ?? 0,
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(resource)?.GetAgeFromPatientId(resource) ?? 0,
            Scope = ["journaldokumenter_helsepersonell"],
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();


        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(3, DocumentReferences?.Count);
    }

    [Fact]
    public async Task HealthcarePersonell_9_EmergencyShouldAccessPatientsDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.UtcNow.AddDays(-1).Day}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-70):yy}39740";
        var subject = $"{DateTime.UtcNow.AddDays(-1).Day}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = ETREAT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(subject)?.GetAgeFromPatientId(subject) ?? 0,
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(resource)?.GetAgeFromPatientId(resource) ?? 0,
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = [.. _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied)];


        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(3, DocumentReferences?.Count);
    }

    [Fact]
    public async Task HealthcarePersonell_10_IfMissingAttributesShouldNotAccessDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.UtcNow.AddDays(-1).Day}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-70):yy}39740";
        var subject = $"{DateTime.UtcNow.AddDays(-1).Day}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            AppliesTo = AppliesTo.HelseId,
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = "FEILVERDI", CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(subject)?.GetAgeFromPatientId(subject) ?? 0,
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(resource)?.GetAgeFromPatientId(resource) ?? 0,
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();


        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Empty(DocumentReferences ?? []);
    }

    [Fact]
    public async Task HealthcarePersonell_Custom01()
    {
        SetupTests();

        var resource = $"{DateTime.UtcNow.AddDays(-1).Day}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-70):yy}39740";
        var subject = $"{DateTime.UtcNow.AddDays(-1).Day}{DateTime.UtcNow:MM}{DateTime.UtcNow.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            AppliesTo = AppliesTo.Helsenorge,
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = "FEILVERDI", CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = _ninParserFactory.CreateNinParser(subject)?.GetAgeFromPatientId(subject) ?? 0,
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = _ninParserFactory.CreateNinParser(resource)?.GetAgeFromPatientId(resource) ?? 0,
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = [.. _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied)];


        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Empty(DocumentReferences ?? []);
    }

    private void SetupTests()
    {
        var documentEntry1 = new DocumentEntryDto()
        {
            ConfidentialityCode =
            [
                new()
                {
                    CodeSystem = TestConstants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = TestConstants.CodeSystems.Hl7.ConfidentialityCode.Normal
                },
                new()
                {
                    CodeSystem = TestConstants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = "othercodethatshouldntaffectlogic"
                }
            ],
        };

        var documentEntry2 = new DocumentEntryDto()
        {
            ConfidentialityCode =
            [
                new()
                {
                    CodeSystem = TestConstants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = TestConstants.CodeSystems.Hl7.ConfidentialityCode.Normal
                },
                new()
                {
                    CodeSystem = TestConstants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = TestConstants.CodeSystems.Hl7.ConfidentialityCode.Restricted
                },
                new()
                {
                    CodeSystem = TestConstants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = "othercodethatshouldntaffectlogic"
                }

            ],
        };

        var documentEntry3 = new DocumentEntryDto()
        {
            ConfidentialityCode =
            [
                new()
                {
                    CodeSystem = TestConstants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = TestConstants.CodeSystems.Hl7.ConfidentialityCode.Normal
                },
                new()
                {
                    CodeSystem = TestConstants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = TestConstants.CodeSystems.Hl7.ConfidentialityCode.Restricted
                },
                new()
                {
                    CodeSystem = TestConstants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = TestConstants.CodeSystems.Hl7.ConfidentialityCode.VeryRestricted
                },
                new()
                {
                    CodeSystem = TestConstants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = "othercodethatshouldntaffectlogic"
                }

            ],
        };

        DocumentReferences = [.. RegistryMetadataTransformerService.TransformDocumentReferenceDtoListToRegistryObjectsStateless([documentEntry1, documentEntry2, documentEntry3])];
    }
}