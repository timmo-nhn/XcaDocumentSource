using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using XcaXds.BusinessLogic.BusinessLogic;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Tests.FakesAndDoubles;
using XcaXds.Tests.Helpers;
using XcaXds.WebService;
using Xunit.Abstractions;
using static XcaXds.Commons.Commons.Constants.CodeSystems.Hl7.ConfidentialityCode;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests;

#pragma warning disable CS8604, CS8601, CS8602 // Possible null reference argument.

public class IntegrationTests_XcaXdsRegistryRepository_CRUD(
    WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : IntegrationTests_DefaultFixture(factory, output), IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    [Trait("Read", "DocumentList (Isolated Access Control)")]
    public async Task XGQ_CrossGatewayQuery_KjernejournalForskriften_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await XGQ_CrossGatewayQuery_KjernejournalForskriften();
    }

    [Fact]
    [Trait("Read", "DocumentList")]
    public async Task XGQ_CrossGatewayQuery_KjernejournalForskriften()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_kjforskriften_readdocumentlist",
            attributeId: Constants.Saml.Attribute.EhelseScope,

            // HAYO! KJ_SCOPE use a non-standard scope
            codeValue: "kjernejournalforskriften",
            action: "ReadDocumentList",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        // Explicitly add KjernejournalForskriften rule for this test
        BusinessLogicFilterer.AddRule(BusinessLogicFilters.HealthcarePersonellKjernejournalForskriften);

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var iti38SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-38_request.xml")));

        var crossGatewayQuery = GetSoapEnvelopeWithKjernejournalForskriftenSamlToken(iti38SoapEnvelope);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var firstContent = await firstResponse.Content.ReadAsStringAsync();
        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(firstContent);

        var responseContent = await firstResponse.Content.ReadAsStringAsync();
        var count = firstResponseSoap?.Body.AdhocQueryResponse?.RegistryObjectList?.OfType<ExtrinsicObjectType>()?.Count() ?? 0;

        var excpectedRegistryObjects = BusinessLogicFilters.FilterByKjernejournalForskriften(RegistryContent.AsRegistryObjectList()).ToArray();

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(excpectedRegistryObjects.Length, firstResponseSoap?.Body.AdhocQueryResponse?.RegistryObjectList?.Length ?? 0);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Fetched {count} entries\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }


    [Fact]
    [Trait("Read", "DocumentList (Isolated Access Control)")]
    public async Task XGQ_CrossGatewayQuery_Kjernejournal_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await XGQ_CrossGatewayQuery_Kjernejournal();
    }

    [Fact]
    [Trait("Read", "DocumentList")]
    public async Task XGQ_CrossGatewayQuery_Kjernejournal()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var iti38SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-38_request.xml")));

        var crossGatewayQuery = GetSoapEnvelopeWithKjernejournalSamlToken(iti38SoapEnvelope);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(firstResponse.Content.ReadAsStream());

        var responseContent = await firstResponse.Content.ReadAsStringAsync();
        var count = firstResponseSoap?.Body.AdhocQueryResponse?.RegistryObjectList?.OfType<ExtrinsicObjectType>()?.Count() ?? 0;

        var excpectedRegistryObjects = BusinessLogicFilters.FilterByConfidentiality(RegistryContent.AsRegistryObjectList(), [Normal, Restricted, VeryRestricted]).ToArray();

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(excpectedRegistryObjects.Length, firstResponseSoap?.Body.AdhocQueryResponse?.RegistryObjectList?.Length ?? 0);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Fetched {count} entries\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }

    [Fact]
    [Trait("Read", "DocumentList (Isolated Access Control)")]
    public async Task XGQ_CrossGatewayQuery_Helsenorge_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await XGQ_CrossGatewayQuery_Helsenorge();
    }

    [Fact]
    [Trait("Read", "DocumentList")]
    public async Task XGQ_CrossGatewayQuery_Helsenorge()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.PurposeOfUse_Helsenorge,
            codeValue: "13",
            codeSystemValue: "1.0.14265.1",
            action: "ReadDocumentList");

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(
            registryObjectsCount: RegistryItemCount
        // ,patientIdentifier: PatientIdentifier.IdNumber
        );

        var iti38SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-38_request.xml")));

        var crossGatewayQuery = GetSoapEnvelopeWithHelsenorgeSamlToken(iti38SoapEnvelope);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(await firstResponse.Content.ReadAsStringAsync());

        var count = firstResponseSoap?.Body.AdhocQueryResponse?.RegistryObjectList?.OfType<ExtrinsicObjectType>().Count() ?? 0;

        var excpectedRegistryObjects = RegistryContent.Where(rc => !rc.DocumentEntry.ConfidentialityCode.Any(ccode => BusinessLogicFilters.CitizenConfidentialityCodesToObfuscate.Contains((ccode.Code!, ccode.CodeSystem!)))).ToArray();

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(0, firstResponseSoap?.Body.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Fetched {count} entries\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }

    [Fact]
    [Trait("Read", "DocumentList")]
    public async Task XGQ_CrossGatewayQuery_Helsenorge_PerformanceTest()
    {
        // Override default with many more entries to simulate a very mature registry/repository.
        RegistryItemCount = 10000;

        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.PurposeOfUse_Helsenorge,
            codeValue: "13",
            codeSystemValue: "1.0.14265.1",
            action: "ReadDocumentList");

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(
            registryObjectsCount: RegistryItemCount
        // ,patientIdentifier: PatientIdentifier.IdNumber
        );

        var patientId = "UniqueId";
        var patientSystem = "1.2.3.4.5";


        _registryWrapper.UpdateDocumentRegistryContentWithDtos([
            new DocumentEntryDto()
            {
                AvailabilityStatus = Constants.Xds.StatusValues.Approved,
                ConfidentialityCode = [new("N", "2.16.578.1.12.4.1.1.9603")],
                SourcePatientInfo = new() { PatientId = new() { Id = patientId, System = patientSystem } },
            }
        ]);

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var iti38SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-38_request.xml")));
        var iti38SoapObject = sxmls.DeserializeXmlString<SoapEnvelope>(iti38SoapEnvelope);
        //iti38SoapObject.Body.AdhocQueryRequest.AdhocQuery.Slot
        //    .FirstOrDefault(s => s.Name == "$XDSDocumentEntryPatientId")?.ValueList?.Value = [$"{patientId}^^{patientSystem}"];
        iti38SoapEnvelope = sxmls.SerializeSoapMessageToXmlString(iti38SoapObject).Content;
        var crossGatewayQuery = GetSoapEnvelopeWithHelsenorgeSamlToken(iti38SoapEnvelope);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayQuery?.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(await firstResponse.Content.ReadAsStringAsync());

        var count = firstResponseSoap?.Body.AdhocQueryResponse?.RegistryObjectList?.OfType<ExtrinsicObjectType>().Count() ?? 0;

        var excpectedRegistryObjects = RegistryContent.Where(rc => !rc.DocumentEntry.ConfidentialityCode.Any(ccode => BusinessLogicFilters.CitizenConfidentialityCodesToObfuscate.Contains((ccode.Code!, ccode.CodeSystem!)))).ToArray();

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(0, firstResponseSoap?.Body.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Fetched {count} entries\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }


    [Fact]
    [Trait("Read", "Documents (Isolated Access Control)")]
    public async Task XGR_CrossGatewayRetrieve_Multipart_Kjernejournal_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await XGR_CrossGatewayRetrieve_Multipart_Kjernejournal();
    }

    [Fact]
    [Trait("Read", "Documents")]
    public async Task XGR_CrossGatewayRetrieve_Multipart_Kjernejournal()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayRetrieve",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocuments");

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var iti39SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-39_request.xml")));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var iti39Request = sxmls.DeserializeXmlString<SoapEnvelope>(iti39SoapEnvelope);


        iti39Request.Body.RetrieveDocumentSetRequest?.DocumentRequest = RegistryContent.Take(Random.Shared.Next(1, (RegistryItemCount / 80) + 1))
            .Select(rc => new DocumentRequestType()
            {
                DocumentUniqueId = rc?.DocumentEntry?.UniqueId,
                RepositoryUniqueId = rc?.DocumentEntry?.RepositoryUniqueId,
                HomeCommunityId = rc?.DocumentEntry?.HomeCommunityId,
            }).ToArray();


        iti39SoapEnvelope = sxmls.SerializeSoapMessageToXmlString(iti39Request).Content;

        var crossGatewayRetrieve = GetSoapEnvelopeWithKjernejournalSamlToken(iti39SoapEnvelope);

        var multipartContent = MultipartExtensions.ConvertRetrieveDocumentSetRequestToMultipartRequest(sxmls.DeserializeXmlString<SoapEnvelope>(crossGatewayRetrieve?.OuterXml), out _);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", multipartContent);

        var firstContent = await firstResponse.Content.ReadAsStringAsync();

        var retrieveDocumentSetResponse = await MultipartExtensions.ReadMultipartSoapMessage(firstResponse.Content.Headers.ContentType?.ToString(), firstContent);

        var excpectedDocumentCount = iti39Request.Body.RetrieveDocumentSetRequest?.DocumentRequest.Length;

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, retrieveDocumentSetResponse?.Body.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(excpectedDocumentCount, retrieveDocumentSetResponse?.Body.RetrieveDocumentSetResponse?.DocumentResponse?.Length ?? 0);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Documents retrieved: {retrieveDocumentSetResponse?.Body.RetrieveDocumentSetResponse?.DocumentResponse?.Length ?? 0}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }


    [Fact]
    [Trait("Read", "Documents (Isolated Access Control)")]
    public async Task XGR_CrossGatewayRetrieve_Multipart_Helsenorge_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await XGR_CrossGatewayRetrieve_Multipart_Helsenorge();
    }

    [Fact]
    [Trait("Read", "Documents")]
    public async Task XGR_CrossGatewayRetrieve_Multipart_Helsenorge()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayRetrieve",
            attributeId: Constants.Saml.Attribute.PurposeOfUse_Helsenorge,
            codeValue: "13",
            codeSystemValue: "1.0.14265.1",
            action: "ReadDocuments");

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var iti39SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-39_request.xml")));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var iti39Request = sxmls.DeserializeXmlString<SoapEnvelope>(iti39SoapEnvelope);

        iti39Request.Body.RetrieveDocumentSetRequest?.DocumentRequest = RegistryContent.Take(Random.Shared.Next(1, (RegistryItemCount / 80) + 1))
            .Select(rc => new DocumentRequestType()
            {
                DocumentUniqueId = rc?.DocumentEntry?.UniqueId,
                RepositoryUniqueId = rc?.DocumentEntry?.RepositoryUniqueId,
                HomeCommunityId = rc?.DocumentEntry?.HomeCommunityId,
            }).ToArray();

        iti39SoapEnvelope = sxmls.SerializeSoapMessageToXmlString(iti39Request).Content;

        var crossGatewayRetrieve = GetSoapEnvelopeWithHelsenorgeSamlToken(iti39SoapEnvelope);

        var multipartContent = MultipartExtensions.ConvertRetrieveDocumentSetRequestToMultipartRequest(sxmls.DeserializeXmlString<SoapEnvelope>(crossGatewayRetrieve?.OuterXml), out _);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", multipartContent);

        var firstContent = await firstResponse.Content.ReadAsStringAsync();

        var retrieveDocumentSetResponse = await MultipartExtensions.ReadMultipartSoapMessage(firstResponse.Content.Headers.ContentType?.ToString(), firstContent);

        var excpectedDocumentCount = iti39Request.Body.RetrieveDocumentSetRequest?.DocumentRequest.Length;

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, retrieveDocumentSetResponse?.Body.RetrieveDocumentSetResponse?.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(excpectedDocumentCount, retrieveDocumentSetResponse?.Body.RetrieveDocumentSetResponse?.DocumentResponse?.Length ?? 0);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Documents retrieved: {retrieveDocumentSetResponse?.Body.RetrieveDocumentSetResponse?.DocumentResponse?.Length ?? 0}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }


    [Fact]
    [Trait("Read", "Documents (Isolated Access Control)")]
    public async Task XGR_CrossGatewayRetrieve_Multipart_Helsenorge_ShouldNotGetAccess_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await XGR_CrossGatewayRetrieve_Multipart_Helsenorge_ShouldNotGetAccess();
    }

    [Fact]
    [Trait("Read", "Documents")]
    public async Task XGR_CrossGatewayRetrieve_Multipart_Helsenorge_ShouldNotGetAccess()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayRetrieve",
            attributeId: Constants.Saml.Attribute.PurposeOfUse_Helsenorge,
            codeValue: "13",
            codeSystemValue: "1.0.14265.1",
            action: "ReadDocumentList");

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var registryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var iti39SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-39_request.xml")));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var iti39Request = sxmls.DeserializeXmlString<SoapEnvelope>(iti39SoapEnvelope);

        iti39Request.Body.RetrieveDocumentSetRequest?.DocumentRequest = registryContent
            .Select(rc => new DocumentRequestType()
            {
                DocumentUniqueId = rc?.DocumentEntry?.UniqueId,
                RepositoryUniqueId = rc?.DocumentEntry?.RepositoryUniqueId,
                HomeCommunityId = rc?.DocumentEntry?.HomeCommunityId,
            }).ToArray();


        iti39SoapEnvelope = sxmls.SerializeSoapMessageToXmlString(iti39Request).Content;

        var crossGatewayRetrieve = GetSoapEnvelopeWithHelsenorgeSamlToken(iti39SoapEnvelope);

        var multipartContent = MultipartExtensions.ConvertRetrieveDocumentSetRequestToMultipartRequest(sxmls.DeserializeXmlString<SoapEnvelope>(crossGatewayRetrieve?.OuterXml), out _);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", multipartContent);

        var firstContent = await firstResponse.Content.ReadAsStringAsync();

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(Constants.MimeTypes.MultipartRelated, firstResponse.Content.Headers.ContentType?.MediaType);

        var retrieveDocumentSetResponse = await MultipartExtensions.ReadMultipartSoapMessage(firstResponse.Content.Headers.ContentType?.ToString(), firstContent);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.True((retrieveDocumentSetResponse?.Body.RetrieveDocumentSetResponse?.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0) > 0);

        await WaitForUserAccessEntryToBeExported();

        _output.WriteLine($"Documents retrieved: {retrieveDocumentSetResponse?.Body.RetrieveDocumentSetResponse?.DocumentResponse?.Length ?? 0}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }


    [Fact]
    [Trait("Read", "Documents (Isolated Access Control)")]
    public async Task XGR_CrossGatewayRetrieve_Helsenorge_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await XGQ_CrossGatewayQuery_Helsenorge();
    }

    [Fact]
    [Trait("Read", "Documents")]
    public async Task XGR_CrossGatewayRetrieve_Helsenorge()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayRetrieve",
            attributeId: Constants.Saml.Attribute.PurposeOfUse_Helsenorge,
            codeValue: "13",
            codeSystemValue: "1.0.14265.1",
            action: "ReadDocuments");

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var registryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var iti39SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-39_request.xml")));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var iti39Request = sxmls.DeserializeXmlString<SoapEnvelope>(iti39SoapEnvelope);

        iti39Request.Body.RetrieveDocumentSetRequest?.DocumentRequest = registryContent
            .Select(rc => new DocumentRequestType()
            {
                DocumentUniqueId = rc?.DocumentEntry?.UniqueId,
                RepositoryUniqueId = rc?.DocumentEntry?.RepositoryUniqueId,
                HomeCommunityId = rc?.DocumentEntry?.HomeCommunityId,
            }).ToArray();


        iti39SoapEnvelope = sxmls.SerializeSoapMessageToXmlString(iti39Request).Content;

        var crossGatewayRetrieve = GetSoapEnvelopeWithHelsenorgeSamlToken(iti39SoapEnvelope);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayRetrieve.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var firstContent = await firstResponse.Content.ReadAsStringAsync();

        var retrieveDocumentSetResponse = new SoapEnvelope();

        retrieveDocumentSetResponse = sxmls.DeserializeXmlString<SoapEnvelope>(firstContent);

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, retrieveDocumentSetResponse?.Body.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Documents retrieved: {retrieveDocumentSetResponse?.Body.RetrieveDocumentSetResponse?.DocumentResponse?.Length ?? 0}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }


    [Fact]
    [Trait("Read", "Documents")]
    public async Task XGR_CrossGatewayRetrieve_Helsenorge_ShouldNotGetAccess_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await XGR_CrossGatewayRetrieve_Helsenorge_ShouldNotGetAccess();
    }

    [Fact]
    [Trait("Read", "Documents")]
    public async Task XGR_CrossGatewayRetrieve_Helsenorge_ShouldNotGetAccess()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayRetrieve",
            attributeId: Constants.Saml.Attribute.PurposeOfUse_Helsenorge,
            codeValue: "somevalue",
            codeSystemValue: "1.0.14265.1",
            action: "ReadDocuments");

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var registryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var iti39SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-39_request.xml")));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var iti39Request = sxmls.DeserializeXmlString<SoapEnvelope>(iti39SoapEnvelope);

        iti39Request.Body.RetrieveDocumentSetRequest?.DocumentRequest = registryContent
            .Select(rc => new DocumentRequestType()
            {
                DocumentUniqueId = rc?.DocumentEntry?.UniqueId,
                RepositoryUniqueId = rc?.DocumentEntry?.RepositoryUniqueId,
                HomeCommunityId = rc?.DocumentEntry?.HomeCommunityId,
            }).ToArray();

        iti39SoapEnvelope = sxmls.SerializeSoapMessageToXmlString(iti39Request).Content;

        var crossGatewayRetrieve = GetSoapEnvelopeWithHelsenorgeSamlToken(iti39SoapEnvelope);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayRetrieve.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var firstContent = await firstResponse.Content.ReadAsStringAsync();

        var retrieveDocumentSetResponse = sxmls.DeserializeXmlString<SoapEnvelope>(firstContent);

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(1, retrieveDocumentSetResponse?.Body.RetrieveDocumentSetResponse?.RegistryResponse.RegistryErrorList?.RegistryError?.Length ?? 0);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Documents retrieved: {retrieveDocumentSetResponse?.Body.RetrieveDocumentSetResponse?.DocumentResponse?.Length ?? 0}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }


    [Fact]
    [Trait("Upload", "Modify Registry/Repository")]
    public async Task PNR_UploadDocuments_RandomAmount_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await PNR_UploadDocuments_RandomAmount();
    }

    [Fact]
    [Trait("Upload", "Modify Registry/Repository")]
    public async Task PNR_UploadDocuments_RandomAmount()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Create");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        Assert.Equal(RegistryItemCount, await _registry.ReadRegistry().OfType<DocumentEntryDto>().CountAsync());

        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(RegistryItemCount, PatientIdentifier.IdNumber, true).PickRandom(Random.Shared.Next(1, RegistryItemCount)).ToArray();
        var registryObjects = metadata.SelectMany(dedto => RegistryMetadataTransformer.TransformDocumentReferenceDtoToRegistryObjects(dedto)).ToArray();
        var documents = metadata.Select(dedto => new DocumentType { Id = dedto.Document.DocumentId, Value = dedto.Document.Data }).ToArray();

        var iti41SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-41_request.xml"))));

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. registryObjects];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = documents;

        var itemsToUploadCount = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterPnR = RegistryItemCount + itemsToUploadCount;

        var iti41RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti41SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti41RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);
        var registryCountAfterPnr = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>().Count();

        var randomDocument = _repository.Read(documents.PickRandom().Id);
        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        Assert.Equal(expectedCountAfterPnR, registryCountAfterPnr);

        Assert.NotNull(randomDocument);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Registry count before test run: {RegistryItemCount}\nUploaded: {itemsToUploadCount} entries.\nRegistry count: {registryCountAfterPnr}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }

    [Fact]
    [Trait("Upload", "Modify Registry/Repository (Concurrent read/writes)")]
    public async Task PNR_RDS_ConcurrentReadWrites()
    {
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_UploadDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Create");

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_DeleteDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Delete");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var countFirst = RegistryContent.AsRegistryObjectDtos().Count();

        Assert.Equal(RegistryItemCount, await _registry.ReadRegistry().OfType<DocumentEntryDto>().CountAsync());

        var randomAmountOfSoapMessages = GenerateRandomSoapEnvelopesThatInteractWithRegistryRepository(10, out var generatedDeletedEntries);

        var tasks = new List<Task<HttpResponseMessage>>();

        var expectedCountAfterPnR = generatedDeletedEntries.generatedEntries.Count + RegistryContent.AsRegistryObjectDtos().Count() - generatedDeletedEntries.deletedEntries.Count;

        foreach (var (message, path) in randomAmountOfSoapMessages)
        {
            tasks.Add(_client.PostAsync(path, new StringContent(message.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml)));
        }

        var result = await Task.WhenAll(tasks);

        foreach (var response in result)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, firstResponseSoap?.Body.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        }

        //var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);
        var registryCountAfterPnr = _registryWrapper.GetDocumentRegistryContentAsDtos().Count();
        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();


        Assert.Equal(expectedCountAfterPnR, registryCountAfterPnr);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Registry count before test run: {countFirst}\nUploaded: {generatedDeletedEntries.generatedEntries.Count} entries.\nDeleted: {generatedDeletedEntries.deletedEntries.Count}\nRegistry count: {registryCountAfterPnr}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }

    [Fact]
    [Trait("Upload", "Modify Registry/Repository")]
    public async Task PNR_UploadDocuments_TooLongFields()
    {
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_UploadDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Create");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);
        var countFirst = RegistryContent.AsRegistryObjectDtos().Count();

        Assert.Equal(RegistryItemCount, await _registry.ReadRegistry().OfType<DocumentEntryDto>().CountAsync());

        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(RegistryItemCount, PatientIdentifier.IdNumber, true).PickRandom();

        var iti41SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-41_request.xml"))));

        metadata.DocumentEntry.Author.FirstOrDefault().Department.OrganizationName = "Lang tekst som overgår 256 bokstaver æøåøæøåøæøåøLang tekst som overgår 256 bokstaver æøåøæøåøæøåøLang tekst som overgår 256 bokstaver æøåøæøåøæøåøLang tekst som overgår 256 bokstaver æøåøæøåøæøåøLang tekst som overgår 256 bokstaver æøåøæøåøæøåøLang tekst som overgår 256 bokstaver æøåøæøåøæøåø";

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. RegistryMetadataTransformer.TransformDocumentReferenceDtoListToRegistryObjects([metadata.DocumentEntry, metadata.SubmissionSet, metadata.Association])];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = [new() { Id = metadata.Document.DocumentId, Value = metadata.Document.Data }];

        var itemsToUploadCount = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterPnR = RegistryItemCount; // No change should happen due to invalid content

        var iti41RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti41SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti41RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);
        var registryCountAfterPnr = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>().Count();
        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.NotEmpty(firstResponseSoap?.Body.RegistryResponse?.RegistryErrorList?.RegistryError);

        Assert.Equal(expectedCountAfterPnR, registryCountAfterPnr);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Registry count before test run: {countFirst}\nUploaded: {itemsToUploadCount} entries.\nRegistry count: {registryCountAfterPnr}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }

    [Fact]
    [Trait("Upload", "Modify Registry/Repository")]
    public async Task PNR_UploadDocuments_InvalidValidation()
    {
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_UploadDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Create");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);
        var countFirst = RegistryContent.AsRegistryObjectDtos().Count();

        Assert.Equal(RegistryItemCount, await _registry.ReadRegistry().OfType<DocumentEntryDto>().CountAsync());

        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(RegistryItemCount, PatientIdentifier.IdNumber, true).PickRandom();

        var iti41SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-41_request.xml"))));

        metadata.DocumentEntry.MimeType = null;
        metadata.DocumentEntry.Title = "<script>alert('bø!');</script>";

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. RegistryMetadataTransformer.TransformDocumentReferenceDtoListToRegistryObjects([metadata.DocumentEntry, metadata.SubmissionSet, metadata.Association])];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = [new() { Id = metadata.Document.DocumentId, Value = metadata.Document.Data }];

        var itemsToUploadCount = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterPnR = RegistryItemCount; // No change should happen due to invalid content

        var iti41RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti41SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti41RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);
        var registryCountAfterPnr = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>().Count();
        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.True(0 < (firstResponseSoap?.Body.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0));

        Assert.Equal(expectedCountAfterPnR, registryCountAfterPnr);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Registry count before test run: {countFirst}\nUploaded: {itemsToUploadCount} entries.\nRegistry count: {registryCountAfterPnr}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }

    [Fact]
    [Trait("Upload", "Modify Registry/Repository")]
    public async Task PNR_UploadDocuments_RandomMimeType_ContainsInvalids_RandomAmount()
    {
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Create");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var testdataDocuments = JsonSerializer.Deserialize<FileDude[]>(File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("Documents"))));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);
        var countFirst = RegistryContent.AsRegistryObjectDtos().Count();

        Assert.Equal(RegistryItemCount, await _registry.ReadRegistry().OfType<DocumentEntryDto>().CountAsync());

        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(RegistryItemCount, PatientIdentifier.IdNumber, true).PickRandom(Random.Shared.Next(1, RegistryItemCount)).ToArray();
        var registryObjects = metadata.AsRegistryObjectDtos();
        var documents = metadata.Select(dedto => new DocumentType { Id = dedto.Document.DocumentId, Value = dedto.Document.Data }).ToArray();

        int unsupportedMimeTypeCount = 0;

        // Replace documents with TestDataDocuments
        foreach (var document in documents)
        {
            var randomFile = testdataDocuments.PickRandom();

            document.Value = randomFile.Data;
            registryObjects.OfType<DocumentEntryDto>()?.FirstOrDefault(ro => ro.UniqueId == document.Id)?.MimeType = randomFile.MimeType;
            if (randomFile.MimeType.IsAnyOf(BusinessLogicFilters.AllowedMimeTypes) == false)
            {
                unsupportedMimeTypeCount++;
            }
        }

        var iti41SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-41_request.xml"))));

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. RegistryMetadataTransformer.TransformRegistryObjectDtosToRegistryObjects(registryObjects)];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = documents;

        var itemsToUploadCount = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();

        var expectedCountAfterPnR = RegistryItemCount; // Nothing should be updated

        var iti41RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti41SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti41RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);
        var registryCountAfterPnr = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>().Count();

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(unsupportedMimeTypeCount, firstResponseSoap?.Body.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        Assert.Equal(expectedCountAfterPnR, registryCountAfterPnr);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Registry count before test run: {countFirst}\nUploaded: {itemsToUploadCount} entries.\nRegistry count: {registryCountAfterPnr}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }

    [Fact]
    [Trait("Update", "Modify Registry/Repository")]
    public async Task PNR_UpdateRegistryRepository_Deprecate_RandomAmount_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await PNR_UpdateRegistryRepository_Deprecate_RandomAmount();
    }

    [Fact]
    [Trait("Update", "Modify Registry/Repository")]
    public async Task PNR_UpdateRegistryRepository_Deprecate_RandomAmount()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Update");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);
        var countFirst = RegistryContent.AsRegistryObjectDtos().Count();

        var amountOfItemsToReplace = Random.Shared.Next(1, RegistryItemCount);

        var randomDocumentEntriesToDeprecate = RegistryContent.PickRandom(amountOfItemsToReplace).ToArray();
        var newDocumentEntries = TestHelpers.GenerateComprehensiveRegistryMetadata(amountOfItemsToReplace, PatientIdentifier.IdNumber, true);

        var assocDtos = newDocumentEntries
            .Zip(randomDocumentEntriesToDeprecate, (nuDocEnt, rndDocEntToDprct) => new AssociationDto
            {
                SourceObject = nuDocEnt.DocumentEntry?.Id,
                TargetObject = rndDocEntToDprct.DocumentEntry?.Id,
                AssociationType = Constants.Xds.AssociationType.Replace
            }).ToArray();


        var assocIds = assocDtos.Select(ass => ass.TargetObject).ToArray();
        var docEntIds = randomDocumentEntriesToDeprecate.Select(ass => ass.DocumentEntry?.Id).ToArray();

        var targets = assocDtos.Select(a => a.TargetObject).ToHashSet();

        Assert.All(randomDocumentEntriesToDeprecate, d => Assert.Contains(d.DocumentEntry?.Id, targets));


        var submitObjectsUpdate = RegistryMetadataTransformer.TransformRegistryObjectDtosToRegistryObjects([.. assocDtos, .. newDocumentEntries.Select(dto => dto.DocumentEntry), .. newDocumentEntries.Select(dto => dto.Association), .. newDocumentEntries.Select(dto => dto.SubmissionSet)]).ToArray();

        var documentUpdate = newDocumentEntries.Select(nde => new DocumentType { Id = nde.Document.DocumentId, Value = nde.Document.Data }).ToArray();

        var iti41SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-41_request.xml"))));

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. submitObjectsUpdate];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = [.. documentUpdate];

        var deprecateAssociations = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList
            .OfType<AssociationType>()
            .Where(robj => docEntIds.Any(id => id == robj.TargetObject)).ToArray();

        var registryContentBeforePnR = _registry.ReadRegistry();
        var actualRegistryCountBeforePnR = await registryContentBeforePnR.CountAsync();

        var itemsToUploadCount = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.Length;
        var expectedCountAfterPnrUpdate = actualRegistryCountBeforePnR + itemsToUploadCount;

        var iti41RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti41SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti41RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);


        var deprecatedDocuments = (await _registry.ReadRegistry().OfType<DocumentEntryDto>().ToArrayAsync()).Where(ro => ro.AvailabilityStatus == Constants.Xds.StatusValues.Deprecated).ToArray();

        var registryContentAfterPnR = _registry.ReadRegistry();
        var actualRegistryCountAfterPnR = await registryContentAfterPnR.CountAsync();

        var randomDocument = _repository.Read(documentUpdate.PickRandom().Id);

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();


        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(expectedCountAfterPnrUpdate, actualRegistryCountAfterPnR);

        Assert.Equal(randomDocumentEntriesToDeprecate.Length, deprecatedDocuments.Length);

        Assert.NotNull(randomDocument);

        Thread.Sleep(1500); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Registry count before test run: {countFirst}\nUpdated: {itemsToUploadCount} entries.\nRegistry count: {actualRegistryCountAfterPnR}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }

    [Fact]
    [Trait("Upload", "Add to Registry")]
    public async Task RDS_UploadRegistry_AddMetadata_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await RDS_UploadRegistry_AddMetadata();
    }

    [Fact]
    [Trait("Upload", "Add to Registry")]
    public async Task RDS_UploadRegistry_AddMetadata()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Create");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);
        var registryContent = _registry.ReadRegistry();
        var countFirst = await registryContent.CountAsync();

        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(RegistryItemCount, PatientIdentifier.IdNumber, true).PickRandom(Random.Shared.Next(1, RegistryItemCount)).ToArray();
        var registryObjects = metadata.SelectMany(dedto => RegistryMetadataTransformer.TransformDocumentReferenceDtoToRegistryObjects(dedto)).ToArray();
        var documents = metadata.Select(dedto => new DocumentType { Id = dedto.Document.DocumentId, Value = dedto.Document.Data }).ToArray();


        var iti42SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-42_request.xml"))));

        iti42SoapRequestObject.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. registryObjects];

        var itemsToUploadCount = iti42SoapRequestObject.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.Length;
        var expectedCountAfterRds = countFirst + itemsToUploadCount;

        var iti42RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti42SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(iti42RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(firstResponse.Content.ReadAsStream());

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        registryContent = _registry.ReadRegistry();
        var registryCount = await registryContent.CountAsync();

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(expectedCountAfterRds, registryCount);
        //Assert.Equal(RegistryItemCount, _repository.DocumentRepository.Count);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Registry count before test run: {countFirst}\nUploaded: {itemsToUploadCount} entries.\nRegistry count: {registryCount}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }


    [Fact]
    [Trait("Delete", "Modify Registry")]
    public async Task RMD_RemoveDocumentsAndMetadata_RandomAmount_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await RMD_RemoveDocumentsAndMetadata_RandomAmount();
    }

    [Fact]
    [Trait("Delete", "Modify Registry")]
    public async Task RMD_RemoveDocumentsAndMetadata_RandomAmount()
    {
        await NukeRegistryRepository();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_RemoveDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Delete");

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_QueryDocumentList",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_QueryDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocuments");

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);
        var countFirst = RegistryContent.AsRegistryObjectDtos().Count();

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var iti18AdhocQuery = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-18_request.xml"))));
        var iti43RetrieveDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-43_request.xml"))));
        var iti62DeleteObjectsRequest = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-62_request.xml"))));
        var iti86DeleteDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-86_request.xml"))));

        var amountOfItemsToReplace = Random.Shared.Next(1, RegistryItemCount);

        // Step -1: Pick random DocumentEntries to remove
        var documentEntryToRemove = RegistryContent.PickRandom(amountOfItemsToReplace).Select(rc => rc.DocumentEntry).ToArray();

        // Step 0: Check if Registry and Repository content is present
        Assert.Equal(RegistryItemCount, await _registry.ReadRegistry().OfType<DocumentEntryDto>().CountAsync());

        // Step 1: Get the unique id for the DocumentEntry in the Registry...
        var iti18RmdRequest = new SoapEnvelope();
        iti18RmdRequest = iti18AdhocQuery; // Reusing this variable saves 0,000000124805 µg of CO2

        iti18RmdRequest.Body.AdhocQueryRequest?.AdhocQuery.Slot =
        [
            new SlotType()
            {
                Name = Constants.Xds.QueryParameters.Associations.Uuid,
                ValueList = new() { Value = [.. documentEntryToRemove.Select(docent => docent?.Id)] }
            }
        ];

        iti18RmdRequest.Body.AdhocQueryRequest?.AdhocQuery.Id = Constants.Xds.StoredQueries.GetAssociations;

        var iti18RmdRequestSoapString = sxmls.SerializeSoapMessageToXmlString(iti18RmdRequest).Content;
        var iti18RmdRequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti18RmdRequestSoapString);

        var iti18RmdRequestResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(iti18RmdRequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var iti18RmdRequestResponseContent = await iti18RmdRequestResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, iti18RmdRequestResponse.StatusCode);

        var iti18RmdResponseSoapObject = sxmls.DeserializeXmlString<SoapEnvelope>(iti18RmdRequestResponseContent);


        Assert.Empty(iti18RmdResponseSoapObject?.Body.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);

        var documentEntriesToRemove = new HashSet<string>(documentEntryToRemove?.Select(de => de.Id));
        var amountOfEntitiesToRemove = documentEntriesToRemove.Count;
        var rmdAssociation = iti18RmdResponseSoapObject?.Body.AdhocQueryResponse?.RegistryObjectList.OfType<AssociationType>().Where(assoc => documentEntriesToRemove.Contains(assoc.TargetObject)).ToArray();


        // Step 2: Use the identifiers in the Association to remove the metadata from the Registry...
        Assert.NotNull(rmdAssociation);
        iti62DeleteObjectsRequest.Body.RemoveObjectsRequest?.ObjectRefList?.ObjectRef = rmdAssociation.SelectMany(assoc => new IdentifiableType[]
        {
            new ObjectRefType() { Id = assoc.Id },
            new ObjectRefType() { Id = assoc.SourceObject },
            new ObjectRefType() { Id = assoc.TargetObject },
        }).ToArray();

        var iti62RequestString = sxmls.SerializeSoapMessageToXmlString(iti62DeleteObjectsRequest).Content;

        var iti62RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti62RequestString);

        var iti62RequestResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(iti62RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));
        Assert.Equal(System.Net.HttpStatusCode.OK, iti62RequestResponse.StatusCode);

        var iti62ResponseContent = await iti62RequestResponse.Content.ReadAsStringAsync();

        var iti62ResponseSoapObject = sxmls.DeserializeXmlString<SoapEnvelope>(iti62ResponseContent);
        Assert.Null(iti62ResponseSoapObject.Body.Fault);

        Assert.Equal(Constants.Xds.ResponseStatusTypes.Success, iti62ResponseSoapObject.Body.RegistryResponse?.Status);

        Assert.Equal(RegistryItemCount - documentEntriesToRemove.Count, await _registry.ReadRegistry().OfType<DocumentEntryDto>().CountAsync());


        // Step 3: Use the DocumentUniqueId in the DocumentEntry to remove the Document
        iti86DeleteDocumentSet.Body.RemoveDocumentsRequest?.DocumentRequest = documentEntryToRemove.SelectMany(docEnt => new[]
        {
            new DocumentRequestType()
            {
                DocumentUniqueId = docEnt?.UniqueId,
                HomeCommunityId = docEnt?.HomeCommunityId,
                RepositoryUniqueId = docEnt?.RepositoryUniqueId
            }
        }).ToArray();

        iti86DeleteDocumentSet.SetAction(Constants.Xds.OperationContract.Iti86Action);

        var iti86RequestString = sxmls.SerializeSoapMessageToXmlString(iti86DeleteDocumentSet).Content;

        var iti86RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti86RequestString);
        var iti86RequestResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti86RequestXmlDoc?.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var iti86RequestResponseContent = await iti86RequestResponse.Content.ReadAsStringAsync();

        var iti86ResponseSoapObject = sxmls.DeserializeXmlString<SoapEnvelope>(iti86RequestResponseContent);

        var registryCount = await _registry.ReadRegistry().OfType<DocumentEntryDto>().CountAsync();

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();

        Assert.Equal(System.Net.HttpStatusCode.OK, iti86RequestResponse.StatusCode);
        Assert.Equal(Constants.Xds.ResponseStatusTypes.Success, iti62ResponseSoapObject.Body.RegistryResponse?.Status);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine($"Registry count before test run: {countFirst}\nRemoved: {documentEntriesToRemove.Count} entries.\nRegistry count: {registryCount}\nExported AtnaLog: {_atnaLogExportedChecker.AtnaMessageString}\nUser Access Entry: {MockStatisticsProcessorService.UserAccessEntryJson}");
    }


    [Fact]
    [Trait("Read", "Read Registry/Repository")]
    public async Task ALL_PutWrongRequestsForActions()
    {
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_RemoveDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Delete");

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_QueryDocumentList",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_QueryDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocuments");

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);
        var countFirst = RegistryContent.AsRegistryObjectDtos().Count();

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var iti18AdhocQuery = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-18_request.xml"))));
        var iti43RetrieveDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-43_request.xml"))));
        var iti62DeleteObjects = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-62_request.xml"))));
        var iti86DeleteDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-86_request.xml"))));

        iti18AdhocQuery.Body.RetrieveDocumentSetRequest = iti43RetrieveDocumentSet.Body.RetrieveDocumentSetRequest;
        iti43RetrieveDocumentSet.Body.AdhocQueryRequest = iti18AdhocQuery.Body.AdhocQueryRequest;
        iti18AdhocQuery.Body.AdhocQueryRequest = null;
        iti43RetrieveDocumentSet.Body.RetrieveDocumentSetRequest = null;

        iti62DeleteObjects.Body.RemoveDocumentsRequest = iti86DeleteDocumentSet.Body.RemoveDocumentsRequest;
        iti86DeleteDocumentSet.Body.RemoveObjectsRequest = iti62DeleteObjects.Body.RemoveObjectsRequest;
        iti86DeleteDocumentSet.Body.RemoveDocumentsRequest = null;
        iti62DeleteObjects.Body.RemoveObjectsRequest = null;

        var requests = new List<Dude>
        {
            new() { Request = iti18AdhocQuery, Endpoint = "/Registry/services/RegistryService" },
            new() { Request = iti62DeleteObjects, Endpoint = "/Registry/services/RegistryService" },
            new() { Request = iti86DeleteDocumentSet, Endpoint = "/Repository/services/RepositoryService" },
            new() { Request = iti43RetrieveDocumentSet, Endpoint = "/Repository/services/RepositoryService" }
        };

        foreach (var request in requests)
        {
            var soapRequestString = sxmls.SerializeSoapMessageToXmlString(request.Request).Content;
            var soapRequestResponse = await _client.PostAsync(request.Endpoint,
                new StringContent(GetSoapEnvelopeWithKjernejournalSamlToken(soapRequestString)?.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

            var responseEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(await soapRequestResponse.Content.ReadAsStringAsync());
            Assert.NotNull(responseEnvelope.Body.Fault);
        }

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
    }

    private static XmlDocument? GetSoapEnvelopeWithKjernejournalForskriftenSamlToken(string soapEnvelope)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var kjSamlTokenString = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_KJ01")));

        var doc = XDocument.Parse(kjSamlTokenString);

        XNamespace saml = "urn:oasis:names:tc:SAML:2.0:assertion";

        var attribute = doc.Descendants(saml + "Attribute")
            .FirstOrDefault(a => (string?)a.Attribute("Name") == Constants.Saml.Attribute.EhelseScope);

        if (attribute != null)
        {
            var valueElement = attribute.Element(saml + "AttributeValue");
            
            // HAYO! KJ_SCOPE
            valueElement?.Value = "kjernejournalforskriften";
        }

        var soapEnvelopeDocument = TestHelpers.LoadNewXmlDocument(soapEnvelope);
        var kjSamlToken = TestHelpers.LoadNewXmlDocument(doc.ToString());

        return GetSoapEnvelopeWithSamlToken(soapEnvelopeDocument, kjSamlToken);
    }

    private static XmlDocument? GetSoapEnvelopeWithKjernejournalSamlToken(SoapEnvelope soapEnvelope)
    {
        var sxmls = new SoapXmlSerializer();
        return GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(soapEnvelope).Content);
    }

    private static XmlDocument? GetSoapEnvelopeWithKjernejournalSamlToken(string soapEnvelope)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var kjSamlTokenString = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_KJ01")));

        var kjSamlToken = TestHelpers.LoadNewXmlDocument(kjSamlTokenString);
        var soapEnvelopeDocument = TestHelpers.LoadNewXmlDocument(soapEnvelope);

        return GetSoapEnvelopeWithSamlToken(soapEnvelopeDocument, kjSamlToken);
    }

    private static XmlDocument? GetSoapEnvelopeWithHelsenorgeSamlToken(string soapEnvelope)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var kjSamlToken = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_HN01"))));
        var soapEnvelopeDocument = TestHelpers.LoadNewXmlDocument(soapEnvelope);

        return GetSoapEnvelopeWithSamlToken(soapEnvelopeDocument, kjSamlToken);
    }

    private static XmlDocument? GetSoapEnvelopeWithSamlToken(XmlDocument? soapEnvelopeDocument, XmlDocument? kjSamlToken)
    {
        var nsmgr = new XmlNamespaceManager(soapEnvelopeDocument.NameTable);
        nsmgr.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        nsmgr.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

        var securityNode = soapEnvelopeDocument.SelectSingleNode("//wsse:Security", nsmgr);

        if (securityNode != null)
        {
            var importedKjToken = soapEnvelopeDocument.ImportNode(kjSamlToken.DocumentElement, true);

            securityNode.AppendChild(importedKjToken);
        }

        return soapEnvelopeDocument;
    }

    private (XmlDocument, string)[] GenerateRandomSoapEnvelopesThatInteractWithRegistryRepository(int amount, out (List<IdentifiableType> generatedEntries, List<string> deletedEntries) generatedDeletedEntries)
    {
        generatedDeletedEntries = ([], []);

        var xmlDocuments = new List<(XmlDocument, string)>();

        for (var i = 0; i < amount; i++)
        {
            switch (Random.Shared.Next(1, 4))
            {
                case 1:
                    var iti41Request = GetRandomIti41Message(out var generatedEntries);
                    generatedDeletedEntries.generatedEntries.AddRange(generatedEntries);
                    var iti41RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti41Request);
                    xmlDocuments.Add((iti41RequestXmlDoc!, "/Repository/services/RepositoryService"));
                    break;

                case 2:
                    var iti62Request = GetRandomIti62Message(out var deletedEntries);
                    generatedDeletedEntries.deletedEntries.AddRange(deletedEntries);
                    var iti62RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti62Request);
                    xmlDocuments.Add((iti62RequestXmlDoc!, "/Registry/services/RegistryService"));

                    break;

                case 3:
                    var iti38Request = GetRandomIti38Request();
                    var iti38RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti38Request);
                    xmlDocuments.Add((iti38RequestXmlDoc!, "/XCA/services/RespondingGatewayService"));
                    break;

                default:
                    break;
            }
        }

        return [.. xmlDocuments];
    }

    private static SoapEnvelope? GetRandomIti38Request()
    {
        var iti38SoapRequestObject = GetSoapEnvelopeFromIntegrationTestFiles("IT_iti-38_request.xml");
        return iti38SoapRequestObject;
    }

    private SoapEnvelope GetRandomIti62Message(out List<string> deletedEntries)
    {
        deletedEntries = [];

        var randomEntries = RegistryContent.PickRandom(Random.Shared.Next(1, 10));

        var iti62SoapRequestObject = GetSoapEnvelopeFromIntegrationTestFiles("iti-62");
        var entriesToDelete = randomEntries.Select(re => new ObjectRefType() { Id = re.DocumentEntry.Id }).ToArray();
        deletedEntries.AddRange(entriesToDelete.Select(or => or.Id).OfType<string>().ToArray());
        iti62SoapRequestObject.Body.RemoveObjectsRequest.ObjectRefList.ObjectRef = entriesToDelete;

        return iti62SoapRequestObject;
    }

    private SoapEnvelope GetRandomIti41Message(out List<IdentifiableType> generatedEntries)
    {
        generatedEntries = [];
        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(RegistryItemCount, PatientIdentifier.IdNumber, true).PickRandom(Random.Shared.Next(1, ((RegistryItemCount + 1) / 10 + 1))).ToArray();
        var registryObjects = metadata.SelectMany(dedto => RegistryMetadataTransformer.TransformDocumentReferenceDtoToRegistryObjects(dedto)).ToArray();
        var documents = metadata.Select(dedto => new DocumentType { Id = dedto.Document.DocumentId, Value = dedto.Document.Data }).ToArray();

        var iti41SoapRequestObject = GetSoapEnvelopeFromIntegrationTestFiles("iti-41");
        generatedEntries.AddRange(registryObjects);

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. registryObjects];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = documents;

        return iti41SoapRequestObject;
    }

    private static SoapEnvelope? GetSoapEnvelopeFromIntegrationTestFiles(string actionName)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));
        var sxmls = new SoapXmlSerializer();

        var soapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains(actionName))));
        return soapRequestObject;
    }
}

internal class FileDude
{
    public string? MimeType { get; set; }
    public byte[]? Data { get; set; }
}

internal class Dude
{
    public SoapEnvelope? Request { get; set; }
    public string? Endpoint { get; set; }
}
#pragma warning restore CS8604, CS8601, CS8602 // Possible null reference argument.