using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Jwk;
using XcaXds.Shared;

namespace XcaXds.WebService.Services;

public class SigningCertificateFetcherService
{
    private readonly ILogger<SigningCertificateFetcherService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationConfig _applicationConfig;

    public SigningCertificateFetcherService(ILogger<SigningCertificateFetcherService> logger, IHttpClientFactory httpClientFactory, ApplicationConfig applicationConfig)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _applicationConfig = applicationConfig;
    }

    public async Task<string[]> GetSamlTokenSigningCertificatesFromExternalApis()
    {
        var client = _httpClientFactory.CreateClient();

        var urls = _applicationConfig.SigningCertificateUrls;

        var certificates = _applicationConfig.CertificatesRaw;

        _logger.LogDebug($"Found {urls.Length} URLs to get certificates from: " + _applicationConfig.SigningCertificateUrls);

        try
        {
            var newCertificates = new List<string>();

            foreach (var url in urls)
            {
                var certificateRaw = await GetToken(client, url);
                if (certificateRaw != null)
                    newCertificates.Add(certificateRaw);
            }

            return [.. newCertificates];
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error when fetching certificates, using fallback values defined in config variables\n" + ex.ToString());
            return certificates;
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
