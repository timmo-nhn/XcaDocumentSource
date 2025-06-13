using Duende.IdentityModel.Client;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using XcaXds.Commons.Models.Custom.AccessToken;

namespace XcaXds.OpenDipsRegistryRepository.Services;

public class OpenDipsClient : IFhirEndpointsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenDipsTokenService _tokenService;

    private FhirJsonParser _fhirJsonParser = new();

    public AccessToken Token { get; set; }

    public OpenDipsClient(IHttpClientFactory httpClientFactory, OpenDipsTokenService tokenService)
    {
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
    }

    public async Task<Resource> FetchFromFhirEndpointAsync(string url, string apiKey)
    {
        if (Token == null || !Token.IsValid())
        {
            Token = await _tokenService.GetAccessTokenAsync();
        }

        var client = _httpClientFactory.CreateClient();

        client.SetBearerToken(Token.Value);

        client.DefaultRequestHeaders.TryAddWithoutValidation("dips-subscription-key", apiKey);

        var response = await client.GetAsync(url);


        var stringContent = await response.Content.ReadAsStringAsync();
        var resource = _fhirJsonParser.Parse<Resource>(stringContent);
        return resource;
    }
}
