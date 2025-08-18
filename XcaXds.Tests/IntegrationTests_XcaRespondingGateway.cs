using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;

namespace XcaXds.Tests;


public class IntegrationTests_XcaRespondingGateway : IClassFixture<WebApplicationFactory<WebService.Program>>
{
    private readonly HttpClient _client;

    public IntegrationTests_XcaRespondingGateway(WebApplicationFactory<WebService.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CrossGatewayQuery()
    {
        var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData"));

        var iti38 = testDataFiles.FirstOrDefault(f => f.Contains("iti38"));

        var soapEnvelope = new StringContent(File.ReadAllText(iti38), Encoding.UTF8, "application/soap+xml");

        var response = await _client.PostAsync("/XCA/services/RespondingGatewayService", soapEnvelope);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
