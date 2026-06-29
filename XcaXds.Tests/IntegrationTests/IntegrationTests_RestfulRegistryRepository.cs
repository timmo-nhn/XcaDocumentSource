using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Shared;
using XcaXds.Shared.Enums;
using XcaXds.Shared.Extensions;
using XcaXds.Shared.Models.Custom;
using Xunit.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests.IntegrationTests;

public partial class IntegrationTests_RestfulRegistryRepository_CRUD : IntegrationTests_DefaultFixture, IClassFixture<WebApplicationFactory<WebService.Program>>
{
    public IntegrationTests_RestfulRegistryRepository_CRUD(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : base(factory, output)
    {
    }


    [Fact]
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

    [Fact]
    [Trait("Get", "Registry/Repository")]
    public async Task Get_Rest_DocumentReference_NoApiKey()
    {

        var documentEntries = (await EnsureRegistryAndRepositoryHasContent(patientIdentifier: PatientIdentifier.IdNumber)).AsRegistryObjectDtos().OfType<DocumentEntryDto>().ToArray();
        var randomEntry = documentEntries.PickRandom();

        _client.DefaultRequestHeaders.Remove("X-API-KEY");

        var url = QueryHelpers.AddQueryString("/api/rest/document-list", "id", randomEntry.SourcePatientInfo?.PatientId?.Id!);
        var firstResponse = await _client.GetAsync(url);

    }

    [Fact]
    [Trait("Get", "Registry/Repository")]
    public async Task Get_Rest_DocumentReference_Fhir()
    {
        var documentEntries = (await EnsureRegistryAndRepositoryHasContent(patientIdentifier: PatientIdentifier.IdNumber)).AsRegistryObjectDtos().OfType<DocumentEntryDto>().ToArray();
        var randomEntry = documentEntries.PickRandom();

        var url = QueryHelpers.AddQueryString("/api/rest/document-entry", 
        [ 
            new KeyValuePair<string,string>("id", randomEntry.Id!)!,
            new KeyValuePair<string,string>("returnType", RestfulDocumentEntryReturnType.Fhir.ToString())!,

        ]);

        var firstResponse = await _client.GetAsync(url);
        var content = await firstResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(Constants.MimeTypes.FhirJson, firstResponse.Content.Headers.ContentType?.MediaType);

        _output.WriteLine("Entry: " + content);
    }

    [Fact]
    [Trait("Delete", "Registry/Repository")]
    public async Task Delete_SpecificParameteres()
    {
        SetDocumentRegistryContent();

        var parameters = new List<KeyValuePair<string, string?>>
        {
            new("patientIdentifier", $"{PatientIdentifier.AssigningAuthority?.UniversalId}|{PatientIdentifier.IdNumber}"),
            new("securityLabel", "2.16.840.1.113883.5.25|V"),
            new("securityLabel", "2.16.578.1.12.4.1.1.9603|NORN_ANG"),
        };

        var url = QueryHelpers.AddQueryString("/api/rest/by-parameters", parameters);
        var firstResponse = await _client.DeleteAsync(url);

        var content = await firstResponse.Content.ReadAsStringAsync();
    }

    private List<RegistryObjectDto> SetDocumentRegistryContent()
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
        return documentEntries;
    }
}