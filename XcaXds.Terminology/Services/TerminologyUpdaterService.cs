using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XcaXds.Shared.Models.Custom;
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

        var terminologySources = TerminologySourcesRegistry.GetDefinitions();

        _logger.LogDebug($"Found {terminologySources.Count} terminology source definitions");

        foreach (var sources in terminologySources)
        {
            var codeSystems = new List<ComprehensiveCodeSystem>();
            foreach (var source in sources.TerminologySources)
            {
                var sourceHandler = _sourceFactory.GetSource(source.SourcePath);
                
                try
                {
                    _logger.LogInformation($"Fetching terminology from {source.SourcePath} using handler {sourceHandler.GetType().Name}");
                    var codeSystem = await sourceHandler.FetchAsync(source);

                    if (codeSystem == null) continue;
                    
                    _logger.LogInformation($"Successfully fetched terminology from {source.SourcePath}. CodeSystem {codeSystem.Name}, Values: {codeSystem.Values?.Length ?? 0}");

                    codeSystems.Add(codeSystem);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to fetch terminology from {source.SourcePath}, handler {sourceHandler.GetType().Name}");
                }
            }

            _terminologyService.AddCodeSystem(sources.Name, [.. codeSystems]);
            _terminologyService.GetCodeSystemByKey(sources.Name);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping terminology service...");
        return Task.CompletedTask;
    }
}