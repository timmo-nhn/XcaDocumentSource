using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit.Abstractions;

namespace XcaXds.Tests.IntegrationTests;

public class IntegrationTests_AuthorizationEndpoints(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : IntegrationTests_DefaultFixture(factory, output), IClassFixture<WebApplicationFactory<WebService.Program>>
{
    //[Fact]
    public async Task TestSecureEndpoint()
    {
        var response = await _client.GetAsync("/secure");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        _client.DefaultRequestHeaders.Add("X-API-KEY", _apiKeyHolder.ApiKey);

        response = await _client.GetAsync("/secure");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
    }
}
