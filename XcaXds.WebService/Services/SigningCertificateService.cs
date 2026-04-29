using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Jwk;

namespace XcaXds.WebService.Services;

public class SigningCertificateService
{
    private readonly ILogger<SigningCertificateService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationConfig _applicationConfig;

    public SigningCertificateService(ILogger<SigningCertificateService> logger, IHttpClientFactory httpClientFactory, ApplicationConfig applicationConfig)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _applicationConfig = applicationConfig;
    }

    public async Task OverrideSigningCertificatesFromExternalApis()
    {
        var client = _httpClientFactory.CreateClient();

        var helseIdResponse = await GetToken(client, _applicationConfig.HelseIdSigningCertUrl);

        if (!string.IsNullOrWhiteSpace(helseIdResponse))
        {
            _applicationConfig.HelseidCert = helseIdResponse;
        }

        var helsenorgeResponse = await GetToken(client, _applicationConfig.HelsenorgeSigningCertUrl);

        if (!string.IsNullOrWhiteSpace(helsenorgeResponse))
        {
            _applicationConfig.HelsenorgeCert = helsenorgeResponse;
        }
    }

    private async Task<string?> GetToken(HttpClient client, string jwkEndpointUrl)
    {
        var response = await client.GetAsync(jwkEndpointUrl);
        var content = await response.Content.ReadAsStringAsync();

        var jwk = JsonSerializer.Deserialize<Jwk>(content, Constants.JsonDefaultOptions.DefaultSettings);
        var helsenorgeKey = jwk?.Keys?.FirstOrDefault()?.X5C?.FirstOrDefault();
        return helsenorgeKey;
    }
}
