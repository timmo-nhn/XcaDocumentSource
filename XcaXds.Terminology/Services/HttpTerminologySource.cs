using System.Text.Json;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.Services;

public class HttpTerminologySource : ITerminologySource
{
    private readonly IHttpClientFactory _clientFactory;

    public HttpTerminologySource(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<ComprehensiveCodeSystem> FetchAsync(string sourceIdentifier)
    {
        var client = _clientFactory.CreateClient();
        var response = await client.GetAsync(sourceIdentifier);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ComprehensiveCodeSystem>(content);
    }
}