using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace XcaXds.IntegrationTests;


public class XcaRespondingGateway : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public XcaRespondingGateway(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/healthz");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
