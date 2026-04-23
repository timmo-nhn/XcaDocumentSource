namespace XcaXds.Tests;

public class IntegrationTests_ClientCertificates : IClassFixture<IntegrationTests_MtlsFixture>
{
    private readonly HttpClient _client;

    public IntegrationTests_ClientCertificates(IntegrationTests_MtlsFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task TestSecureEndpoint()
    {
        var response = await _client.GetAsync("/secure");

        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
    }
}
