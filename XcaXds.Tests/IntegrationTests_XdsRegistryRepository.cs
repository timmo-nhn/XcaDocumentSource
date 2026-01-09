using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Services;
using XcaXds.Tests.FakesAndDoubles;
using XcaXds.Tests.Helpers;
using XcaXds.WebService.Services;
using XcaXds.WebService.Startup;

namespace XcaXds.Tests;

public class IntegrationTests_XcaRespondingGatewayQueryRetrieve : IClassFixture<WebApplicationFactory<WebService.Program>>
{
    private readonly HttpClient _client;
    private readonly RestfulRegistryRepositoryService _restfulRegistryService;
    private readonly PolicyRepositoryService _policyRepositoryService;
    private readonly InMemoryPolicyRepository _policyRepository;
    private readonly InMemoryRegistry _registry;
    private readonly InMemoryRepository _repository;

    // The amount of registry objects to generate and evaluate against
    private readonly int RegistryItemCount = 10;

    public IntegrationTests_XcaRespondingGatewayQueryRetrieve(
        WebApplicationFactory<WebService.Program> factory)
    {
        using var scope = factory.Services.CreateScope();

        _policyRepository = new InMemoryPolicyRepository();

        _registry = new InMemoryRegistry();
        _repository = new InMemoryRepository();

        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<AppStartupService>();
                services.RemoveAll<AuditLogExporterService>();

                // Remove implementations defined in Program.cs (WebApplicationFactory<WebService.Program>) ...
                services.RemoveAll<IPolicyRepository>();
                services.RemoveAll<IRegistry>();
                services.RemoveAll<IRepository>();

                // ...so replace with the mock implementations
                services.AddSingleton<IPolicyRepository>(_policyRepository);
                services.AddSingleton<IRegistry>(_registry);
                services.AddSingleton<IRepository>(_repository);
            });
        });

        _client = customFactory.CreateClient();
        using var customScope = customFactory.Services.CreateScope();

        _restfulRegistryService = customScope.ServiceProvider.GetRequiredService<RestfulRegistryRepositoryService>();
        _policyRepositoryService = customScope.ServiceProvider.GetRequiredService<PolicyRepositoryService>();
    }

    [Fact]
    public async Task CrossGatewayQuery_KjToken()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        EnsureRegistryAndRepositoryHasContent(registryObjects: RegistryItemCount, patientIdentifier: "13116900216");

        var tempPolicyName = "IT_CrossGatewayQuery";

        _policyRepositoryService.AddPolicy(new PolicyDto()
        {
            AppliesTo = [Issuer.HelseId, Issuer.Helsenorge],
            Id = tempPolicyName,
            Rules =
            [[
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:role:code", Value = "LE;SP;PS" },
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem", Value = "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060" }
            ]],
            Actions = ["ReadDocumentList"],
            Effect = "Permit",
        });

        var iti38SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti38-request.xml")));

        var crossGatewayQuery = GetSoapEnvelopeWithKjernejournalSamlToken(iti38SoapEnvelope);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(firstResponse.Content.ReadAsStream());

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(RegistryItemCount, firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryObjectList.Length);
    }

    [Fact]
    public async Task CrossGatewayQuery_HnToken()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        EnsureRegistryAndRepositoryHasContent(registryObjects: RegistryItemCount, patientIdentifier: "13116900216");

        var tempPolicyName = "IT_CrossGatewayQuery";

        _policyRepositoryService.AddPolicy(new PolicyDto()
        {
            AppliesTo = [Issuer.HelseId, Issuer.Helsenorge],
            Id = tempPolicyName,
            Rules =
            [[
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:purposeofuse:code", Value = "13" },
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:purposeofuse:codeSystem", Value = "1.0.14265.1" }
            ]],
            Actions = ["ReadDocumentList"],
            Effect = "Permit",
        });

        var iti38SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti38-request.xml")));

        var crossGatewayQuery = GetSoapEnvelopeWithHelsenorgeSamlToken(iti38SoapEnvelope);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));


        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(await firstResponse.Content.ReadAsStringAsync());

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(RegistryItemCount, firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryObjectList.Length);
    }


    [Fact]
    public async Task CrossGatewayRetrieve_KjToken()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var registryContent = EnsureRegistryAndRepositoryHasContent(registryObjects: RegistryItemCount, patientIdentifier: "13116900216");

        var tempPolicyName = "IT_CrossGatewayRetrieve";

        _policyRepositoryService.AddPolicy(new PolicyDto()
        {
            AppliesTo = [Issuer.HelseId, Issuer.Helsenorge],
            Id = tempPolicyName,
            Rules =
            [[
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:role:code", Value = "LE;SP;PS" },
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem", Value = "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060" }
            ]],
            Actions = ["ReadDocuments"],
            Effect = "Permit",
        });

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

        var crossGatewayRetrieve = GetSoapEnvelopeWithKjernejournalSamlToken(iti39SoapEnvelope);

        var multipartContent = MultipartExtensions.ConvertRetrieveDocumentSetRequestToMultipartRequest(sxmls.DeserializeXmlString<SoapEnvelope>(crossGatewayRetrieve?.OuterXml), out _);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", multipartContent);

        var firstContent = await firstResponse.Content.ReadAsStringAsync();

        var retrieveDocumentSetResponse = await MultipartExtensions.ReadMultipartSoapMessage(firstResponse.Content.Headers.ContentType?.ToString(), firstContent);

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, retrieveDocumentSetResponse?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(RegistryItemCount, retrieveDocumentSetResponse?.Body?.RetrieveDocumentSetResponse?.DocumentResponse?.Length);
    }


    [Fact]
    public async Task CrossGatewayRetrieve_HnToken()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var registryContent = EnsureRegistryAndRepositoryHasContent(registryObjects: RegistryItemCount, patientIdentifier: "13116900216");

        var tempPolicyName = "IT_CrossGatewayRetrieve";

        _policyRepositoryService.AddPolicy(new PolicyDto()
        {
            AppliesTo = [Issuer.HelseId, Issuer.Helsenorge],
            Id = tempPolicyName,
            Rules =
            [[
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:purposeofuse:code", Value = "13" },
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:purposeofuse:codeSystem", Value = "1.0.14265.1" }
            ]],
            Actions = ["ReadDocuments"],
            Effect = "Permit",
        });

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

        var retrieveDocumentSetResponse = await MultipartExtensions.ReadMultipartSoapMessage(firstResponse.Content.Headers.ContentType?.ToString(), firstContent);

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, retrieveDocumentSetResponse?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
        Assert.Equal(RegistryItemCount, retrieveDocumentSetResponse?.Body?.RetrieveDocumentSetResponse?.DocumentResponse?.Length);
    }


    [Fact]
    public async Task CrossGatewayRetrieve_HnToken_Multipart_ShouldNotGetAccess()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var registryContent = EnsureRegistryAndRepositoryHasContent(registryObjects: RegistryItemCount, patientIdentifier: "13116900216");

        var tempPolicyName = "IT_CrossGatewayRetrieve";

        _policyRepositoryService.AddPolicy(new PolicyDto()
        {
            AppliesTo = [Issuer.HelseId, Issuer.Helsenorge],
            Id = tempPolicyName,
            Rules =
            [[
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:purposeofuse:code", Value = "somevalue" },
                new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:purposeofuse:codeSystem", Value = "1.0.14265.1" }
            ]],
            Actions = ["ReadDocumentList"],
            Effect = "Permit",
        });

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

        var retrieveDocumentSetResponse = new SoapEnvelope();

        if (firstResponse.Content.Headers.ContentType?.MediaType == Constants.MimeTypes.XopXml ||
            firstResponse.Content.Headers.ContentType?.MediaType == Constants.MimeTypes.MultipartRelated)
        {
            retrieveDocumentSetResponse = await MultipartExtensions.ReadMultipartSoapMessage(firstResponse.Content.Headers.ContentType?.ToString(), firstContent);
        }
        else
        {
            retrieveDocumentSetResponse = sxmls.DeserializeXmlString<SoapEnvelope>(firstContent);
        }

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(1, retrieveDocumentSetResponse?.Body?.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
    }


    private List<DocumentReferenceDto> EnsureRegistryAndRepositoryHasContent(int registryObjects = 10, string? patientIdentifier = null)
    {
        var metadata = TestHelpers.GenerateRegistryMetadata(registryObjects, patientIdentifier, true);

        _registry.WriteRegistry(metadata.AsRegistryObjectList());

        foreach (var document in metadata.Select(dto => dto.Document))
        {
            _repository.Write(document.DocumentId, document.Data);
        }

        return metadata;
    }

    private XmlDocument? GetSoapEnvelopeWithKjernejournalSamlToken(string iti39SoapEnvelope)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var kjSamlTokenString = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_KJ01")));

        var kjSamlToken = TestHelpers.LoadNewXmlDocument(kjSamlTokenString);
        var soapEnvelopeDocument = TestHelpers.LoadNewXmlDocument(iti39SoapEnvelope);

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


