using Microsoft.AspNetCore.Mvc.Testing;
using XcaXds.BusinessLogic.Models.Custom;
using XcaXds.BusinessLogic.Services;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared.Enums;
using Xunit.Abstractions;
using static XcaXds.Tests.TestConstants.CodeSystems.OtherIsoDerived.PurposeOfUse;

using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests;

public class UnitTests_BusinessLogic_ObfuscateDocuments(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : IntegrationTests_DefaultFixture(factory, output), IClassFixture<WebApplicationFactory<WebService.Program>>
{
    public List<IdentifiableType> DocumentReferences { get; private set; } = [];


    [Fact]
    public async Task HealthcarePersonell_TREAT_ShouldPartiallyObfuscate()
    {
        SetupDocumentReferencesWithConfidentialityCodes();

        var businessLogic = new BusinessLogicParameters()
        {
            AppliesTo = AppliesTo.Kjernejournal,
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = "TREAT", CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            SubjectOrganization = new() { Code = "Norsk Helsenett" },
            Scope = ["journaldokumenter_helsepersonell"],
            Subject = new("subject", "code"),
            Resource = new("resource", "code"),
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var entries).ToList();

        Assert.Equal(1, entries.Count);
    }

    [Fact]
    public async Task HealthcarePersonell_ETREAT_ShouldNOTObfuscate()
    {
        SetupDocumentReferencesWithConfidentialityCodes();

        var businessLogic = new BusinessLogicParameters()
        {
            AppliesTo = AppliesTo.Kjernejournal,
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = "ETREAT", CodeSystem = TestConstants.CodeSystems.Hl7.PurposeOfUse.System },
            SubjectOrganization = new() { Code = "Norsk Helsenett" },
            Subject = new("subject","code"),
            Resource = new("resource","code"),
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var entries).ToList();

        Assert.Equal(1, entries.Count);
    }

    [Fact]
    public async Task Citizen_NormalQuery_ShouldPartiallyObfuscate()
    {
        SetupDocumentReferencesWithConfidentialityCodes();

        var businessLogic = new BusinessLogicParameters()
        {
            AppliesTo = AppliesTo.Helsenorge,
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = SubjectOfCare_13, CodeSystem = TestConstants.CodeSystems.OtherIsoDerived.PurposeOfUse.System },
            SubjectOrganization = new() { Code = "Norsk Helsenett" },
            Subject = new("subject","system"),
            Resource = new("resource", "system"),
            SubjectAge = 21
        };

        var initialCount = DocumentReferences.Count;
        
        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var entries).ToList();

        Assert.Single(entries);
    }

    [Fact]
    public async Task Unknown_NormalQuery_ShouldFullyObfuscate()
    {
        SetupDocumentReferencesWithConfidentialityCodes();

        var businessLogic = new BusinessLogicParameters()
        {
            AppliesTo = AppliesTo.Unknown,
            Acp = TestConstants.Acp.NullValue,
            Purpose = new() { Code = "invalid code", CodeSystem = "invalid system" },
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = _documentListFiltererService.FilterRegistryObjectListBasedOnBusinessLogic(DocumentReferences, businessLogic, out var entries).ToList();

        Assert.Equal(0, entries.Count);
    }

    private void SetupDocumentReferencesWithConfidentialityCodes()
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
                .. _businessLogicFiltersRegistry.GetHealthcarePersonellConfidentialityCodesToObfuscate()
               .Select(p => new CodedValue() { CodeSystem = p.Item2, Code = p.Item1 }),
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
                .. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate()
                .Select(p => new CodedValue() { CodeSystem = p.Item2, Code = p.Item1 }),
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
