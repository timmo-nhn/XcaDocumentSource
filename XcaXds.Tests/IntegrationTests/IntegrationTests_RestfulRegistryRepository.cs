using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using XcaXds.BusinessLogic.Services;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Shared.Models.Custom;
using Xunit.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests.IntegrationTests;

public partial class IntegrationTests_RestfulRegistryRepository_CRUD : IntegrationTests_DefaultFixture, IClassFixture<WebApplicationFactory<WebService.Program>>
{
    public IntegrationTests_RestfulRegistryRepository_CRUD(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : base(factory, output)
    {
    }


    //[Fact]
    [Trait("Delete", "Registry/Repository")]
    public async Task Delete_OlderThanXMonths()
    {
        var days = Random.Shared.Next(30, 365);

        var documentEntries = (await EnsureRegistryAndRepositoryHasContent(patientIdentifier: PatientIdentifier.IdNumber)).AsRegistryObjectDtos().OfType<DocumentEntryDto>().ToArray();

        var oldDocumentEntries = documentEntries.Where(de => de.ServiceStopTime < DateTime.Now.AddDays(-days)).ToArray();

        var url = QueryHelpers.AddQueryString("/api/rest/delete-older-than", "days", string.Empty + days);
        var firstResponse = await _client.DeleteAsync(url);

        Assert.Equal(await _registry.ReadRegistry().OfType<DocumentEntryDto>().CountAsync(), documentEntries.Length - oldDocumentEntries.Length);
    }

    //[Fact]
    [Trait("Delete", "Registry/Repository")]
    public async Task Delete_SpecificParameteres()
    {
        SetDocumentRegistryContent();

        var parameters = new List<KeyValuePair<string, string?>>
        {
            new("patientIdentifier", "2.16.578.1.12.4.1.4.1|13116900216"),
            new("securityLabel", "2.16.840.1.113883.5.25|V"),
            new("securityLabel", "2.16.578.1.12.4.1.1.9603|NORN_ANG"),
        };

        var url = QueryHelpers.AddQueryString("/api/rest/by-parameters", parameters);
        var firstResponse = await _client.DeleteAsync(url);

        var content = await firstResponse.Content.ReadAsStringAsync();
    }

    private void SetDocumentRegistryContent()
    {
        var documentEntries = new List<RegistryObjectDto>
        {
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}
            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [new(TestConstants.CodeSystems.Hl7.ConfidentialityCode.Normal, TestConstants.CodeSystems.Hl7.ConfidentialityCode.System)],
                SourcePatientInfo = new(){PatientId = new(){Id = PatientIdentifier.IdNumber, System = PatientIdentifier.AssigningAuthority?.UniversalId}}

            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [new(TestConstants.CodeSystems.Hl7.ConfidentialityCode.Normal, TestConstants.CodeSystems.Hl7.ConfidentialityCode.System)],
                SourcePatientInfo = new(){PatientId = new(){Id = "AnotherPatient", System = "123.123.123"}}

            },
            new DocumentEntryDto()
            {
                ConfidentialityCode = [.. _businessLogicFiltersRegistry.GetCitizenConfidentialityCodesToObfuscate().Select(code => new CodedValue(code.Item1, code.Item2))],
                SourcePatientInfo = new(){PatientId = new(){Id = "AnotherPatient", System = "123.123.123"}}
            },
        };

        _registryWrapper.SetDocumentRegistryContentWithDtos(documentEntries);
    }
}