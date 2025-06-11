using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;


namespace XcaXds.Source.Services.Custom;
public class OpenDipsTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenDipsTokenService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var client = _httpClientFactory.CreateClient();

        var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = "https://api.dips.no/dips.oauth/connect/token",
            ClientId = "postman",
            ClientSecret = "postman",
            Scope = "dips-fhir-r4"
        });

        if (tokenResponse.IsError)
            throw new Exception($"Token request failed: {tokenResponse.Error}");

        return tokenResponse.AccessToken;
    }
}
