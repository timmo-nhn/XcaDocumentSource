using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.BusinessLogic;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Tests.Helpers;
using Xunit.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests;

#pragma warning disable CS8604, CS8601, CS8602 // Possible null reference argument.

public partial class IntegrationTests_XcaXdsRegistryRepository_CRUD : IntegrationTests_DefaultFixture, IClassFixture<WebApplicationFactory<WebService.Program>>
{
    public IntegrationTests_XcaXdsRegistryRepository_CRUD(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : base(factory, output)
    {
    }

    [Fact]
    [Trait("Read", "DocumentList")]
    public async Task XGQ_CrossGatewayQuery_Kjernejournal()
    {
        _policyRepositoryService.DeleteAllPolicies();
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

        var iti38SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti38-request.xml")));

        var crossGatewayQuery = GetSoapEnvelopeWithKjernejournalSamlToken(iti38SoapEnvelope);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(firstResponse.Content.ReadAsStream());

        var responseContent = await firstResponse.Content.ReadAsStringAsync();
        var count = firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();

        var excpectedRegistryObjects = RegistryContent.Select(dr => dr.DocumentEntry).Where(rc => !rc.ConfidentialityCode.Any(ccode => BusinessLogicFilters.HealthcarePersonellConfidentialityCodesToObfuscate.Contains((ccode.Code, ccode.CodeSystem)))).ToArray();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(excpectedRegistryObjects.Length, firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryObjectList.Length ?? 0);

        Thread.Sleep(500); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Fetched {count} entries");

        // Cleanup
        await NukeRegistryRepository(); 
        _policyRepositoryService.DeleteAllPolicies();
    }

    [Fact]
    [Trait("Read", "DocumentList")]
    public async Task XGQ_CrossGatewayQuery_Helsenorge()
    {
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

        await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var iti38SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti38-request.xml")));

        var crossGatewayQuery = GetSoapEnvelopeWithHelsenorgeSamlToken(iti38SoapEnvelope);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayQuery?.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(await firstResponse.Content.ReadAsStringAsync());

        var count = firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();

        var excpectedRegistryObjects = RegistryContent.Where(rc => !rc.DocumentEntry.ConfidentialityCode.Any(ccode => BusinessLogicFilters.CitizenConfidentialityCodesToObfuscate.Contains((ccode.Code, ccode.CodeSystem)))).ToArray();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(0, firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        Thread.Sleep(500); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Fetched {count} entries");

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
    }


    [Fact]
    [Trait("Read", "Documents")]
    public async Task XGR_CrossGatewayRetrieve_Multipart_Kjernejournal()
    {
        _policyRepositoryService.DeleteAllPolicies();
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

        var iti39SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti39-request.xml")));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var iti39Request = sxmls.DeserializeXmlString<SoapEnvelope>(iti39SoapEnvelope);

        iti39Request.Body.RetrieveDocumentSetRequest?.DocumentRequest = RegistryContent
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

        var excpectedDocumentCount = RegistryContent.Count(rc => !rc.DocumentEntry.ConfidentialityCode.Any(ccode => BusinessLogicFilters.HealthcarePersonellConfidentialityCodesToObfuscate.Contains((ccode.Code, ccode.CodeSystem))));

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, retrieveDocumentSetResponse?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(excpectedDocumentCount, retrieveDocumentSetResponse?.Body?.RetrieveDocumentSetResponse?.DocumentResponse?.Length);

        Thread.Sleep(500); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Documents retrieved: {retrieveDocumentSetResponse?.Body?.RetrieveDocumentSetResponse?.DocumentResponse.Length}");

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
    }


    [Fact]
    [Trait("Read", "Documents")]
    public async Task XGR_CrossGatewayRetrieve_Multipart_Helsenorge()
    {
        _policyRepositoryService.DeleteAllPolicies();
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

        var iti39SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti39-request.xml")));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var iti39Request = sxmls.DeserializeXmlString<SoapEnvelope>(iti39SoapEnvelope);

        iti39Request.Body.RetrieveDocumentSetRequest?.DocumentRequest = RegistryContent
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

        var excpectedDocumentCount = RegistryContent.Count(rc => !rc.DocumentEntry.ConfidentialityCode.Any(ccode => BusinessLogicFilters.CitizenConfidentialityCodesToObfuscate.Contains((ccode.Code, ccode.CodeSystem))));

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, retrieveDocumentSetResponse?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(excpectedDocumentCount, retrieveDocumentSetResponse?.Body?.RetrieveDocumentSetResponse?.DocumentResponse?.Length);

        Thread.Sleep(500); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Documents retrieved: {retrieveDocumentSetResponse?.Body?.RetrieveDocumentSetResponse?.DocumentResponse}");

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
    }


    [Fact]
    [Trait("Read", "Documents")]
    public async Task XGR_CrossGatewayRetrieve_Multipart_Helsenorge_ShouldNotGetAccess()
    {
        _policyRepositoryService.DeleteAllPolicies();
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

        var iti39SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti39-request.xml")));

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

        Assert.Equal(Constants.MimeTypes.MultipartRelated, firstResponse.Content.Headers.ContentType?.MediaType);

        var retrieveDocumentSetResponse = await MultipartExtensions.ReadMultipartSoapMessage(firstResponse.Content.Headers.ContentType?.ToString(), firstContent);

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(1, retrieveDocumentSetResponse?.Body?.RetrieveDocumentSetResponse?.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
    }


    [Fact]
    [Trait("Read", "Documents")]
    public async Task XGR_CrossGatewayRetrieve_Helsenorge()
    {
        _policyRepositoryService.DeleteAllPolicies();
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

        var iti39SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti39-request.xml")));

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


        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, retrieveDocumentSetResponse?.Body?.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        Thread.Sleep(500); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Documents retrieved: {retrieveDocumentSetResponse?.Body?.RetrieveDocumentSetResponse?.DocumentResponse}");

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
    }


    [Fact]
    [Trait("Read", "Documents")]
    public async Task XGR_CrossGatewayRetrieve_Helsenorge_ShouldNotGetAccess()
    {
        _policyRepositoryService.DeleteAllPolicies();
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

        var iti39SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti39-request.xml")));

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


        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(1, retrieveDocumentSetResponse?.Body?.RetrieveDocumentSetResponse?.RegistryResponse.RegistryErrorList?.RegistryError?.Length ?? 0);

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
    }

    [Fact]
    [Trait("Upload", "Modify Registry/Repository")]
    public async Task PNR_UploadDocuments_RandomAmount()
    {
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

        await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        Assert.Equal(RegistryItemCount, _registry.ReadRegistry().OfType<DocumentEntryDto>().Count());

        var metadata = TestHelpers.GenerateRegistryMetadata(RegistryItemCount, PatientIdentifier.IdNumber, true).PickRandom(Random.Shared.Next(1, RegistryItemCount)).ToArray();
        var registryObjects = metadata.SelectMany(dedto => RegistryMetadataTransformer.TransformDocumentReferenceDtoToRegistryObjects(dedto)).ToArray();
        var documents = metadata.Select(dedto => new DocumentType { Id = dedto.Document.DocumentId, Value = dedto.Document.Data }).ToArray();

        var iti41SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti41-request.xml"))));

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. registryObjects];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = documents;

        var itemsToUploadCount = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterPnR = RegistryItemCount + itemsToUploadCount;

        var iti41RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti41SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti41RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body?.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        Assert.Equal(expectedCountAfterPnR, _registry.ReadRegistry().OfType<DocumentEntryDto>().Count());
       
        Thread.Sleep(500); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Registry count before test run: {RegistryItemCount}\nUploaded: {itemsToUploadCount} entries.\nRegistry count: {_registry.ReadRegistry().Count()}");

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
    }


    [Fact]
    [Trait("Update", "Modify Registry/Repository")]
    public async Task PNR_UpdateRegistryRepository_Deprecate_RandomAmount()
    {
        _policyRepositoryService.DeleteAllPolicies();
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

        var registryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var amountOfItemsToReplace = Random.Shared.Next(1, RegistryItemCount);

        var randomDocumentEntriesToDeprecate = registryContent.PickRandom(amountOfItemsToReplace).ToArray();
        var newDocumentEntries = TestHelpers.GenerateRegistryMetadata(amountOfItemsToReplace, PatientIdentifier.IdNumber, true);

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


        var submitObjectsUpdate = RegistryMetadataTransformer.
            TransformRegistryObjectDtosToRegistryObjects([.. assocDtos, .. newDocumentEntries.Select(dto => dto.DocumentEntry), .. newDocumentEntries.Select(dto => dto.Association), .. newDocumentEntries.Select(dto => dto.SubmissionSet)]);
        var documentUpdate = newDocumentEntries.Select(nde => new DocumentType { Id = nde.Document.DocumentId, Value = nde.Document.Data }).ToArray();

        var iti41SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti41-request.xml"))));

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. submitObjectsUpdate];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = [.. documentUpdate];

        var deprecateAssociations = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList
            .OfType<AssociationType>()
            .Where(robj => docEntIds.Any(id => id == robj.TargetObject)).ToArray();

        var itemsToUploadCount = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterPnrUpdate = RegistryItemCount + itemsToUploadCount;

        var iti41RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti41SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti41RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);

        var registryContentbale = _registry.ReadRegistry().OfType<DocumentEntryDto>();

        var deprecatedDocuments = _registry.ReadRegistry().OfType<DocumentEntryDto>().ToArray().Where(ro => ro.AvailabilityStatus == Constants.Xds.StatusValues.Deprecated).ToArray();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(expectedCountAfterPnrUpdate, _registry.ReadRegistry().OfType<DocumentEntryDto>().Count());
        //Assert.Equal(expectedCountAfterPnrUpdate, _repository.().Count);
        Assert.Equal(randomDocumentEntriesToDeprecate.Length, deprecatedDocuments.Length);
        
        Thread.Sleep(5000); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Registry count before test run: {RegistryItemCount}\nUpdated: {itemsToUploadCount} entries.\nRegistry count: {_registry.ReadRegistry().Count()}");

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
    }

    [Fact]
    [Trait("Upload", "Add to Registry")]
    public async Task RDS_UploadRegistry_AddMetadata()
    {
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

        await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var metadata = TestHelpers.GenerateRegistryMetadata(RegistryItemCount, PatientIdentifier.IdNumber, true).PickRandom(Random.Shared.Next(1, RegistryItemCount)).ToArray();
        var registryObjects = metadata.SelectMany(dedto => RegistryMetadataTransformer.TransformDocumentReferenceDtoToRegistryObjects(dedto)).ToArray();
        var documents = metadata.Select(dedto => new DocumentType { Id = dedto.Document.DocumentId, Value = dedto.Document.Data }).ToArray();


        var iti42SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti42-request.xml"))));

        iti42SoapRequestObject.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. registryObjects];

        var itemsToUploadCount = iti42SoapRequestObject.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterRds = RegistryItemCount + itemsToUploadCount;

        var iti42RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti42SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(iti42RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(firstResponse.Content.ReadAsStream());

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(expectedCountAfterRds, _registry.ReadRegistry().OfType<DocumentEntryDto>().Count());
        //Assert.Equal(RegistryItemCount, _repository.DocumentRepository.Count);
      
        Thread.Sleep(500); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Registry count before test run: {RegistryItemCount}\nUploaded: {itemsToUploadCount} entries.\nRegistry count: {_registry.ReadRegistry().Count()}");

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
    }

    [Fact]
    [Trait("Delete", "Modify Registry")]
    public async Task RMD_RemoveDocumentsAndMetadata_RandomAmount()
    {
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

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var iti18AdhocQuery = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti18-request.xml"))));
        var iti43RetrieveDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti43-request.xml"))));
        var iti62DeleteObjectsRequest = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti62-request.xml"))));
        var iti86DeleteDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti86-request.xml"))));

        var amountOfItemsToReplace = Random.Shared.Next(1, RegistryItemCount);

        // Step -1: Pick random DocumentEntries to remove
        var documentEntryToRemove = RegistryContent.PickRandom(amountOfItemsToReplace).Select(rc => rc.DocumentEntry).ToArray();

        // Step 0: Check if Registry and Repository content is present
        Assert.Equal(RegistryItemCount, _registry.ReadRegistry().OfType<DocumentEntryDto>().Count());

        // Step 1: Get the unique id for the DocumentEntry in the Registry...
        var iti18RmdRequest = new SoapEnvelope();
        iti18RmdRequest = iti18AdhocQuery; // Reusing this variable saves 0,000000124805 µg of CO2

        iti18RmdRequest.Body.AdhocQueryRequest?.AdhocQuery.Slot = [new SlotType()
        {
            Name = Constants.Xds.QueryParameters.Associations.Uuid,
            ValueList = new() { Value = [.. documentEntryToRemove.Select(docent => docent?.Id)] }
        }];

        iti18RmdRequest.Body.AdhocQueryRequest?.AdhocQuery.Id = Constants.Xds.StoredQueries.GetAssociations;

        var iti18RmdRequestSoapString = sxmls.SerializeSoapMessageToXmlString(iti18RmdRequest).Content;
        var iti18RmdRequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti18RmdRequestSoapString);

        var iti18RmdRequestResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(iti18RmdRequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));
        Assert.Equal(System.Net.HttpStatusCode.OK, iti18RmdRequestResponse.StatusCode);

        var iti18RmdRequestResponseContent = await iti18RmdRequestResponse.Content.ReadAsStringAsync();

        var iti18RmdResponseSoapObject = sxmls.DeserializeXmlString<SoapEnvelope>(iti18RmdRequestResponseContent);


        Assert.Empty(iti18RmdResponseSoapObject?.Body?.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);

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
        Assert.Equal(Constants.Xds.ResponseStatusTypes.Success, iti62ResponseSoapObject.Body.RegistryResponse?.Status);

        Assert.Equal(RegistryItemCount - documentEntriesToRemove.Count, _registry.ReadRegistry().OfType<DocumentEntryDto>().Count());


        // Step 3: Use the DocumentUniqueId in the DocumentEntry to remove the Document
        iti86DeleteDocumentSet.Body.RemoveDocumentsRequest?.DocumentRequest = documentEntryToRemove.SelectMany(docEnt => new[]{

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

        Assert.Equal(System.Net.HttpStatusCode.OK, iti86RequestResponse.StatusCode);
        Assert.Equal(Constants.Xds.ResponseStatusTypes.Success, iti62ResponseSoapObject.Body.RegistryResponse?.Status);
        //Assert.Equal(RegistryItemCount - documentEntriesToRemove.Count, _repository.DocumentRepository.Count);

        _output.WriteLine($"Registry count before test run: {RegistryItemCount}\nRemoved: {documentEntriesToRemove.Count} entries.\nRegistry count: {_registry.ReadRegistry().Count()}");

        // Cleanup
        await NukeRegistryRepository();
        _policyRepositoryService.DeleteAllPolicies();
    }


    [Fact]
    [Trait("Read", "Read Registry/Repository")]
    public async Task ALL_PutWrongRequestsForActions()
    {
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

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var iti18AdhocQuery = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti18-request.xml"))));
        var iti43RetrieveDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti43-request.xml"))));
        var iti62DeleteObjects = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti62-request.xml"))));
        var iti86DeleteDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti86-request.xml"))));

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
            new Dude { Request = iti18AdhocQuery, Endpoint = "/Registry/services/RegistryService"},
            new Dude { Request = iti62DeleteObjects, Endpoint = "/Registry/services/RegistryService"},
            new Dude { Request = iti86DeleteDocumentSet, Endpoint = "/Repository/services/RepositoryService"},
            new Dude { Request = iti43RetrieveDocumentSet, Endpoint = "/Repository/services/RepositoryService"}
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


    private async Task<List<DocumentReferenceDto>> EnsureRegistryAndRepositoryHasContent(int registryObjectsCount = 10, string? patientIdentifier = null)
    {
        await NukeRegistryRepository();

        var metadata = TestHelpers.GenerateRegistryMetadata(registryObjectsCount, patientIdentifier, true);
        _registryWrapper.UpdateDocumentRegistryContentWithDtos(metadata.AsRegistryObjectList());

        foreach (var document in metadata.Select(dto => dto.Document))
        {
            _repository.Write(document.DocumentId, document.Data);
        }

        return metadata;
    }

    private async Task NukeRegistryRepository()
    {
        var getNukeKey = await _client.GetAsync("api/get-nuke-key");

        var nukeResponse = JsonDocument.Parse(await getNukeKey.Content.ReadAsStringAsync());
        var nukeKey = nukeResponse.RootElement.GetProperty("nukeKey").GetString();

        var nuked = await _client.DeleteAsync($"/api/nuke?nukeKey={nukeKey}");

        Assert.Empty(_registry.ReadRegistry());
    }

    private XmlDocument? GetSoapEnvelopeWithKjernejournalSamlToken(string soapEnvelope)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var kjSamlTokenString = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_KJ01")));

        var kjSamlToken = TestHelpers.LoadNewXmlDocument(kjSamlTokenString);
        var soapEnvelopeDocument = TestHelpers.LoadNewXmlDocument(soapEnvelope);

        return GetSoapEnvelopeWithSamlToken(soapEnvelopeDocument, kjSamlToken);
    }

    private XmlDocument? GetSoapEnvelopeWithHelsenorgeSamlToken(string iti39SoapEnvelope)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var kjSamlToken = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_HN01"))));
        var soapEnvelopeDocument = TestHelpers.LoadNewXmlDocument(iti39SoapEnvelope);

        return GetSoapEnvelopeWithSamlToken(soapEnvelopeDocument, kjSamlToken);
    }

    private XmlDocument? GetSoapEnvelopeWithSamlToken(XmlDocument? soapEnvelopeDocument, XmlDocument? kjSamlToken)
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
    internal async Task SetupRegistryRepositoryAndPolicy()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var patients = _restfulRegistryService.GetPatientIdentifiersInRegistry();

        // Ensure the registry has stuff to work with
        if (patients?.Count == 0)
        {
            var testData = new StringContent(
                File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("TestDataRegistryObjects.json"))),
                Encoding.UTF8,
                Constants.MimeTypes.Json
                );

            var testDataGenerationResponse = await _client.PostAsync("/api/generate-test-data", testData);
        }

        var tempPolicyName = "IT_CrossGateway";

        _policyRepository.AddPolicy(new PolicyDto()
        {
            Id = tempPolicyName,
            Rules =
            [[
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:role:code", Value = "LE;SP;PS" },
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem", Value = "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060" }
            ]],
            Actions = ["ReadDocuments", "ReadDocumentList"],
            Effect = "Permit",
        });
    }
}

internal class Dude
{
    public SoapEnvelope? Request { get; set; }
    public string? Endpoint { get; set; }
}
#pragma warning restore CS8604, CS8601, CS8602 // Possible null reference argument.