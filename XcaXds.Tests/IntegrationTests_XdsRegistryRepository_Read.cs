using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Xml;
using XcaXds.Commons.Commons;
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

        RegistryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

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

        EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

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

        RegistryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

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

        RegistryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

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

        var registryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

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

        var registryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

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

        var registryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

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
    }




    //[Fact]
    public async Task Async_CrossGatewayQuery()
    {
        var receivedRequests = new List<string>();

        var replyToUrl = "/XCA/services/RespondingGatewayService/replyto";

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        await SetupRegistryRepositoryAndPolicy();

        var crossGatewayRetrieve = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti38-request_async.xml"))));


        var kjSamlToken = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_KJ01"))));


        if (crossGatewayRetrieve == null || crossGatewayRetrieve == null)
        {
            Assert.Fail("Where did the integration test data go?!");
        }

        var nsmgr = new XmlNamespaceManager(crossGatewayRetrieve.NameTable);
        nsmgr.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        nsmgr.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
        nsmgr.AddNamespace("a", "http://www.w3.org/2005/08/addressing");
        nsmgr.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

        var securityNode = crossGatewayRetrieve.SelectSingleNode("//wsse:Security", nsmgr);

        var replyToNode = crossGatewayRetrieve.SelectSingleNode("//s:Header/a:ReplyTo/a:Address", nsmgr);

        if (replyToNode != null)
        {
            replyToNode.InnerText = replyToUrl;
        }

        if (securityNode != null)
        {
            var importedKjToken = crossGatewayRetrieve.ImportNode(kjSamlToken.DocumentElement, true);

            securityNode.AppendChild(importedKjToken);
        }

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayRetrieve.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var firstContent = await firstResponse.Content.ReadAsStringAsync();
    }


    //[Fact]
    public async Task Async_CrossGatewayRetrieve()
    {
        var receivedRequests = new List<string>();

        var replyToUrl = "XCA/services/RespondingGatewayService/replyto";


        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        await SetupRegistryRepositoryAndPolicy();

        var crossGatewayRetrieve = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti39-request_async.xml"))));
        var kjSamlToken = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_KJ01"))));

        if (crossGatewayRetrieve == null)
        {
            Assert.Fail("Where did the integration test data go?!");
        }

        var nsmgr = new XmlNamespaceManager(crossGatewayRetrieve.NameTable);
        nsmgr.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        nsmgr.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
        nsmgr.AddNamespace("a", "http://www.w3.org/2005/08/addressing");
        nsmgr.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

        var securityNode = crossGatewayRetrieve.SelectSingleNode("//wsse:Security", nsmgr);

        var replyToNode = crossGatewayRetrieve.SelectSingleNode("//s:Header/a:ReplyTo/a:Address", nsmgr);

        if (replyToNode != null)
        {
            replyToNode.InnerText = replyToUrl;
        }

        if (securityNode != null)
        {
            var importedKjToken = crossGatewayRetrieve.ImportNode(kjSamlToken.DocumentElement, true);

            securityNode.AppendChild(importedKjToken);
        }

        var firstResponse = await _client.PostAsync("XCA/services/RespondingGatewayService", new StringContent(crossGatewayRetrieve.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        Assert.Equal(System.Net.HttpStatusCode.Accepted, firstResponse.StatusCode);

        var firstContent = await firstResponse.Content.ReadAsStringAsync();
    }

    private List<DocumentReferenceDto> EnsureRegistryAndRepositoryHasContent(int registryObjectsCount = 10, string? patientIdentifier = null)
    {
        var metadata = TestHelpers.GenerateRegistryMetadata(registryObjectsCount, patientIdentifier, true);
        _registryWrapper.UpdateDocumentRegistryContentWithDtos(metadata.AsRegistryObjectList());

        foreach (var document in metadata.Select(dto => dto.Document))
        {
            _repository.Write(document.DocumentId, document.Data);
        }

        return metadata;
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