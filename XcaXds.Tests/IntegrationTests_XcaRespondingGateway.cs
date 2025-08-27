using System.Text;
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
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", "IntegrationTests"));
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

        var iti38 = testDataFiles.FirstOrDefault(f => f.Contains("iti38"));

        if (iti38 == null)
        {
            Assert.Fail("Where did the test data go?!");
        }

        var soapEnvelope = new StringContent(File.ReadAllText(iti38), Encoding.UTF8, Constants.MimeTypes.SoapXml);
        var response = await _client.PostAsync("/XCA/services/RespondingGatewayService", soapEnvelope);
        var responseBody = await response.Content.ReadAsStringAsync();
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var soapObject = sxmls.DeserializeSoapMessage<SoapEnvelope>(responseBody);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
