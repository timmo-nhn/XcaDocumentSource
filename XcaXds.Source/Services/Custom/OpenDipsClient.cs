using Duende.IdentityModel.Client;

namespace XcaXds.Source.Services.Custom;

public class OpenDipsClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenDipsTokenService _tokenService;
    public OpenDipsClient(IHttpClientFactory httpClientFactory, OpenDipsTokenService tokenService)
    {
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
    }

    public async Task<string> CallApiAsync(string url)
    {
        var token = await _tokenService.GetAccessTokenAsync();

        var client = _httpClientFactory.CreateClient();
        
        client.SetBearerToken(token);
       
        var response = await client.GetAsync(url);
       
       
        return await response.Content.ReadAsStringAsync();
    }
}
