using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Serializers;
using XcaXds.Source.Services;

namespace XcaXds.Tests;


public class IntegrationTests_XcaRespondingGateway : IClassFixture<WebApplicationFactory<WebService.Program>>
{
    private readonly HttpClient _client;
    private readonly RestfulRegistryRepositoryService _restfulRegistryService;

    public IntegrationTests_XcaRespondingGateway(WebApplicationFactory<WebService.Program> factory)
    {
        _client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        _restfulRegistryService = scope.ServiceProvider.GetRequiredService<RestfulRegistryRepositoryService>();
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

        var iti38 = integrationTestFiles.FirstOrDefault(f => f.Contains("iti38"));

        if (iti38 == null)
        {
            Assert.Fail("Where did the integration test data go?!");
        }

        var crossGatewayQuery = new XmlDocument();
        var xmlContent = File.ReadAllText(iti38);
        crossGatewayQuery.LoadXml(xmlContent);

        var nsmgr = new XmlNamespaceManager(crossGatewayQuery.NameTable);
        nsmgr.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");

        var roleAttrValueNode = crossGatewayQuery.SelectSingleNode(
            "//saml:Attribute[@Name='urn:oasis:names:tc:xspa:1.0:subject:role']/saml:AttributeValue",
            nsmgr
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
            }
        }

        var soapEnvelope = new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml);

        var response = await _client.PostAsync("/XCA/services/RespondingGatewayService", soapEnvelope);
        var responseBody = await response.Content.ReadAsStringAsync();
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var soapObject = sxmls.DeserializeSoapMessage<SoapEnvelope>(responseBody);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
