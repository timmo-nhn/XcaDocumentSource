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
        _logger.LogInformation("Starting terminology service...");

        // Example: Mix of HTTP and File sources

        var allCodeSystems = new List<ComprehensiveCodeSystem>();

        foreach (var sources in TerminologySources.CodeSystems)
        {
            foreach (var source in sources.Value)
            {
                var sourceHandler = _sourceFactory.GetSource(source);
                try
                {
                    var codeSystem = await sourceHandler.FetchAsync(source);
                    allCodeSystems.Add(codeSystem);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch terminology from {Source}", source);
                }
            }
        }

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping terminology service...");
        return Task.CompletedTask;
    }
}