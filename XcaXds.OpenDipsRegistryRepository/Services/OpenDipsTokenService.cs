using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using XcaXds.Commons.Models.Custom.AccessToken;

namespace XcaXds.OpenDipsRegistryRepository.Services;

public class OpenDipsTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenDipsTokenService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AccessToken?> GetAccessTokenAsync()
    {
        var client = _httpClientFactory.CreateClient();

        var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = "https://api.dips.no/dips.oauth/connect/token",
            ClientId = "opendips-newman",
            ClientSecret = "opendips-newman",
            Scope = "dips-fhir-r4"
        });

        if (tokenResponse.IsError)
            throw new Exception($"Token request failed: {tokenResponse.Error}");

        return new() 
        {
            IssuedAt = DateTime.UtcNow,
            Duration = tokenResponse.ExpiresIn,
            Value = tokenResponse.AccessToken
        };
    }
}
