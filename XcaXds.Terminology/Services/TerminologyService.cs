using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using XcaXds.Terminology.Mappers;
using XcaXds.Terminology.Models.Custom;
using XcaXds.Terminology.Sources;
using XcaXds.Terminology.ValueSets;

namespace XcaXds.Terminology.Services;

public class TerminologyService : IHostedService
{
    private readonly ILogger<TerminologyService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public TerminologyService(ILogger<TerminologyService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting terminology service...");

        var genderCodeSystems = await FetchCodeSystem(TerminologySources.GenderCodeSystems);
        TerminologyValueSets.CodeSystems.Add("Gender", genderCodeSystems);

    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping terminology service...");
    }

    private async Task<List<ComprehensiveCodeSystem>> FetchCodeSystem(string[] codeSystemSources)
    {
        var codeSystems = new List<ComprehensiveCodeSystem>();

        var client = _httpClientFactory.CreateClient();

        foreach (var codeSystemSource in codeSystemSources)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, codeSystemSource);
            var response = await client.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();

            var codeSystem = JsonSerializer.Deserialize(

            codeSystems.Add());
        }
    }
}