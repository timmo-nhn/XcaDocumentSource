using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using XcaXds.BusinessLogic.BusinessLogic;
using XcaXds.BusinessLogic.Models.Custom;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared.Enums;
using Xunit.Abstractions;
using static XcaXds.Tests.TestConstants.CodeSystems.Hl7.PurposeOfUse;
using static XcaXds.Tests.TestConstants.CodeSystems.OtherIsoDerived.PurposeOfUse;

namespace XcaXds.Tests.UnitTests;

public class UnitTests_BusinessLogic_UseCases : IntegrationTests_DefaultFixture, IClassFixture<WebApplicationFactory<WebService.Program>>
{
    private List<IdentifiableType> DocumentReferences = new();
    internal readonly ITestOutputHelper _output;

    public UnitTests_BusinessLogic_UseCases(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : base(factory, output)
    {
        _output = output;
    }


    [Fact]
    public async Task Citizen_1_ShouldOpenDocumentsOnThemself()
    {
        SetupTests();

        var patientId = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = SubjectOfCare_13, CodeSystem = TestConstants.CodeSystems.OtherIsoDerived.PurposeOfUse.System },
            Subject = new() { Code = patientId, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(patientId),
            Resource = new() { Code = patientId, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(patientId),
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
        var patientId12To16Years = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-13):yy}79740";

        var yearPart = DateTime.Now.AddYears(-13).Year.ToString().Substring(2, 2);

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = patientId12To16Years, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(patientId12To16Years),
            Resource = new() { Code = patientId12To16Years, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(patientId12To16Years),
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

        var patientId16To18Years = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-17):yy}79740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = patientId16To18Years, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(patientId16To18Years),
            Resource = new() { Code = patientId16To18Years, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(patientId16To18Years),
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

        var resourceBelow12Years = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-6):yy}79740";
        var subject = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.RepresentCitizenUnder12,
            Purpose = new() { Code = SubjectOfCare_13, CodeSystem = TestConstants.CodeSystems.OtherIsoDerived.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(subject),
            Resource = new() { Code = resourceBelow12Years, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(resourceBelow12Years),
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

        var resource = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.RepresentAnotherCitizen,
            Purpose = new() { Code = PWATRNY, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(resource),
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

        var resource = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(resource),
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

        var resource = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-13):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-42):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.RepresentCitizenUnder12,
            Purpose = new() { Code = SubjectOfCare_13, CodeSystem = TestConstants.CodeSystems.OtherIsoDerived.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(resource),
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

        var subject = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = TREAT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(subject),
            Resource = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(subject),
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

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = TREAT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(resource),
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

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = ETREAT, CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();


        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(3, DocumentReferences?.Count);
    }

    [Fact]
    public async Task HealthcarePersonell_10_IfMissingAttributesShouldNotAccessDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            AppliesTo = AppliesTo.Kjernejournal,
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = "FEILVERDI", CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(resource),
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

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            AppliesTo = AppliesTo.Helsenorge,
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = "FEILVERDI", CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = TestConstants.AssigningAuthority.Nin },
            SubjectAge = BusinessLogicExtensions.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = TestConstants.AssigningAuthority.Nin },
            ResourceAge = BusinessLogicExtensions.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var applied).ToList();


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

        DocumentReferences = RegistryMetadataTransformer.TransformDocumentReferenceDtoListToRegistryObjects([documentEntry1, documentEntry2, documentEntry3]).ToList();
    }
}