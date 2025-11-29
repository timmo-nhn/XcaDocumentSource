using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Services;
using XcaXds.Tests.Helpers;

namespace XcaXds.Tests;


public class IntegrationTests_XcaRespondingGatewayQueryRetrieve : IClassFixture<WebApplicationFactory<WebService.Program>>
{
    private readonly HttpClient _client;
    private readonly RestfulRegistryRepositoryService _restfulRegistryService;
    private readonly PolicyRepositoryService _policyRepositoryService;

    public IntegrationTests_XcaRespondingGatewayQueryRetrieve(WebApplicationFactory<WebService.Program> factory)
    {
        _client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        _restfulRegistryService = scope.ServiceProvider.GetRequiredService<RestfulRegistryRepositoryService>();
        _policyRepositoryService = scope.ServiceProvider.GetRequiredService<PolicyRepositoryService>();
    }

    [Fact]
    public async Task CrossGatewayQuery()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var crossGatewayQuery = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti38-request.xml"))));
        var kjSamlToken = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_KJ01"))));
        var hnSamlToken = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_HN01"))));


        if (crossGatewayQuery == null || kjSamlToken == null || hnSamlToken == null)
        {
            Assert.Fail("Where did the integration test data go?!");
        }


        var nsmgr = new XmlNamespaceManager(crossGatewayQuery.NameTable);
        nsmgr.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        nsmgr.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");


        var securityNode = crossGatewayQuery.SelectSingleNode("//wsse:Security", nsmgr);

        if (securityNode != null)
        {
            var importedKjToken = crossGatewayQuery.ImportNode(kjSamlToken.DocumentElement, true);
            var importedSamlToken = crossGatewayQuery.ImportNode(hnSamlToken.DocumentElement, true);

            securityNode.AppendChild(importedKjToken);
        }

        var firstResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));


        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(firstResponse.Content.ReadAsStream());

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
    }


    [Fact]
    public async Task CrossGatewayRetrieve()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

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

        // Ensure policies are set up correctly
        var policySet = _policyRepositoryService.GetPoliciesAsPolicySetDto();

        var tempPolicyName = "IT_CrossGatewayRetrieve";

        if (policySet.Policies?.Count == 0)
        {
            _policyRepositoryService.AddPolicy(new PolicyDto()
            {
                Id = tempPolicyName,
                Rules =
                [[
                    new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:role:code", Value = "LE;SP;PS" },
                    new() { AttributeId = "urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem", Value = "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060" }
                ]],
                Actions = ["ReadDocuments"],
                Effect = "Permit",
            });
        }

        var retrieveDocumentSet = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti38-request.xml"))));


        if (retrieveDocumentSet == null || retrieveDocumentSet == null)
        {
            Assert.Fail("Where did the integration test data go?!");
        }

        var nsmgr = new XmlNamespaceManager(retrieveDocumentSet.NameTable);
        nsmgr.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        nsmgr.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");



        var securityNode = retrieveDocumentSet.SelectSingleNode("//wsse:Security", nsmgr);

        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(retrieveDocumentSet.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var firstContent = await firstResponse.Content.ReadAsStringAsync();
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

        // Ensure policies are set up correctly
        var policySet = _policyRepositoryService.GetPoliciesAsPolicySetDto();

        var tempPolicyName = "IT_CrossGateway";

        if (policySet.Policies?.Count == 0)
        {
            _policyRepositoryService.AddPolicy(new PolicyDto()
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
}