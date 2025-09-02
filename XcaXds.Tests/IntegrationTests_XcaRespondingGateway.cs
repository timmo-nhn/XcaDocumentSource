using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using XcaXds.Commons.Commons;
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
        if (policySet.Policies?.Count == 0)
        {

        }

        var crossGatewayQuery = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("iti38"))));
        var kjSamlToken = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_KJ01"))));
        var hnSamlToken = TestHelpers.LoadNewXmlDocument(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_HN01"))));


        if (crossGatewayQuery == null || kjSamlToken == null || hnSamlToken == null)
        {
            Assert.Fail("Where did the integration test data go?!");
        }

        var soapEnvelope = new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml);

        var response = await _client.PostAsync("/XCA/services/RespondingGatewayService", soapEnvelope);
        var responseBody = await response.Content.ReadAsStringAsync();
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var soapObject = sxmls.DeserializeSoapMessage<SoapEnvelope>(responseBody);



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
                roleNode.SetAttribute("code", "PS");

                var updatedRoleXml = roleNode.OuterXml;

                roleAttrValueNode.InnerText = roleNode.OuterXml.Replace("<", "&lt;").Replace(">", "&gt;");
            }
        }


        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    private void CrossGatewayQuery_NodeInserted(object sender, XmlNodeChangedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
