using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit.Abstractions;

namespace XcaXds.Tests;

public class IntegrationTests_AuthorizationEndpoints : IntegrationTests_DefaultFixture, IClassFixture<WebApplicationFactory<WebService.Program>>
{
    public IntegrationTests_AuthorizationEndpoints(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : base(factory, output)
    {
    }

    [Fact]
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
