using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Services;
using XcaXds.Tests.Helpers;

namespace XcaXds.Tests;


public class IntegrationTests_XcaRespondingGateway : IClassFixture<WebApplicationFactory<WebService.Program>>
{
    private readonly HttpClient _client;
    private readonly RestfulRegistryRepositoryService _restfulRegistryService;
    private readonly PolicyRepositoryService _policyRepositoryService;

    public IntegrationTests_XcaRespondingGateway(WebApplicationFactory<WebService.Program> factory)
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

        var tempPolicyName = "IT_CrossGatewayQuery";

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
                Actions = ["ReadDocumentList"],
                Effect = "Permit",
            });
        }

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

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var roleAttrValueNode = crossGatewayQuery.SelectSingleNode(
            "//saml:Attribute[@Name='urn:oasis:names:tc:xspa:1.0:subject:role']/saml:AttributeValue", nsmgr
        );

        if (roleAttrValueNode != null)
        {
            var roleXmlString = Regex.Replace(HttpUtility.HtmlDecode(roleAttrValueNode.InnerText), @"\bxsi:\b", "");

            var roleDoc = new XmlDocument();
            roleDoc.LoadXml(roleXmlString);

            var roleNode = roleDoc.DocumentElement;
            if (roleNode != null)
            {
                // Set the role to something else
                roleNode.SetAttribute("code", "HIBB");

                var updatedRoleXml = roleNode.OuterXml;

                roleAttrValueNode.InnerText = roleNode.OuterXml;
            }
        }

        var secondResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService",
            new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml)
        );

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var firstResponseSoap = sxmls.DeserializeSoapMessage<SoapEnvelope>(firstResponse.Content.ReadAsStream());
        var secondResponseSoap = sxmls.DeserializeSoapMessage<SoapEnvelope>(secondResponse.Content.ReadAsStream());

        _policyRepositoryService.DeletePolicy(tempPolicyName);

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        Assert.Equal(1, secondResponseSoap?.Body?.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length);
        Assert.Equal(System.Net.HttpStatusCode.OK, secondResponse.StatusCode);
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

        var crossGatewayRetrieve = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti39-request.xml"))));

        var crossGatewayRetrieveMultipart = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti39-request-multipart.txt"))).Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");


        if (crossGatewayRetrieve == null || crossGatewayRetrieveMultipart == null)
        {
            Assert.Fail("Where did the integration test data go?!");
        }


        var nsmgr = new XmlNamespaceManager(crossGatewayRetrieve.NameTable);
        nsmgr.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        nsmgr.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");


        var securityNode = crossGatewayRetrieve.SelectSingleNode("//wsse:Security", nsmgr);

        var firstResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayRetrieve.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var mimeBoundary = Regex.Match(crossGatewayRetrieveMultipart, "--(?<boundary>MIMEBoundary_.*?)\n").Groups.Values.LastOrDefault()?.Value.Trim();

        var multipartRequestXop = new StringContent(crossGatewayRetrieveMultipart, Encoding.UTF8, Constants.MimeTypes.XopXml);
        var multipartBoundary = new NameValueHeaderValue("boundary", mimeBoundary);
        multipartRequestXop.Headers.ContentType?.Parameters.Add(multipartBoundary);

        var secondResponse = await _client.PostAsync("https://localhost:7176/XCA/services/RespondingGatewayService", multipartRequestXop);

        var multipartRequestMultipart = new StringContent(crossGatewayRetrieveMultipart, Encoding.UTF8, Constants.MimeTypes.MultipartRelated);
        var multipartBoundaryMultipart = new NameValueHeaderValue("boundary", mimeBoundary);
        multipartRequestMultipart.Headers.ContentType?.Parameters.Add(multipartBoundaryMultipart);

        var thirdResponse = await _client.PostAsync("https://localhost:7176/XCA/services/RespondingGatewayService", multipartRequestMultipart);
        
        
        var firstContent = await firstResponse.Content.ReadAsStringAsync();
        var secondContent = await secondResponse.Content.ReadAsStringAsync();
        var thirdContent = await thirdResponse.Content.ReadAsStringAsync();
    }
}
