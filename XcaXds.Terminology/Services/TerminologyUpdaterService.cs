using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XcaXds.Terminology.Models.Custom;
using XcaXds.Terminology.Sources;

namespace XcaXds.Terminology.Services;

public class TerminologyUpdaterService : IHostedService
{
    private readonly ILogger<TerminologyUpdaterService> _logger;
    private readonly TerminologySourceFactory _sourceFactory;
    private readonly TerminologyService _terminologyService;

    public TerminologyUpdaterService(
    ILogger<TerminologyUpdaterService> logger,
    TerminologySourceFactory sourceFactory,
    TerminologyService terminologyService)
    {
        _logger = logger;
        _sourceFactory = sourceFactory;
        _terminologyService = terminologyService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing terminology service...");

        var allCodeSystems = new Dictionary<string, ComprehensiveCodeSystem[]>();

        var terminologySources = TerminologySources.GetDefinitions();
        
        _logger.LogDebug("Found {Count} terminology source definitions", terminologySources.Count);

        foreach (var sources in terminologySources)
        {
            var codeSystems = new List<ComprehensiveCodeSystem>();
            foreach (var source in sources.TerminologySources)
            {
                var sourceHandler = _sourceFactory.GetSource(source.SourcePath);
                try
                {
                    var codeSystem = await sourceHandler.FetchAsync(source);

                    if (codeSystem == null) continue;
                    codeSystems.Add(codeSystem);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch terminology from {Source}", source);
                }
            }

            _terminologyService.AddCodeSystem(sources.Name, [.. codeSystems]);
            _terminologyService.GetCodeSystemByName(sources.Name);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping terminology service...");
        return Task.CompletedTask;
    }
}