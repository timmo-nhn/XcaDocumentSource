using Microsoft.Extensions.Logging;
using XcaXds.Shared.Enums;
using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Sources;

namespace XcaXds.Terminology.Services;

public class TerminologyUpdaterService
{
    private readonly ILogger<TerminologyUpdaterService> _logger;
    private readonly TerminologyService _terminologyService;
    private readonly TerminologySourcesRegistryService _terminologySourcesRegistryService;

    public ServiceState ServiceStatus;

    public TerminologyUpdaterService(
    ILogger<TerminologyUpdaterService> logger,
    TerminologyService terminologyService,
    TerminologySourcesRegistryService terminologySourcesRegistryService)
    {
        _logger = logger;
        _terminologyService = terminologyService;
        _terminologySourcesRegistryService = terminologySourcesRegistryService;
    }

    public async Task InitializeTerminologyServiceAsync(CancellationToken cancellationToken)
    {
        var allCodeSystems = new Dictionary<string, ComprehensiveCodeSystem[]>();

        var terminologySources = _terminologySourcesRegistryService.GetAllDefinitions();

        _logger.LogDebug($"Found {terminologySources.Count} terminology source definitions");

        foreach (var sources in terminologySources)
        {
            var codeSystems = new List<ComprehensiveCodeSystem>();
            foreach (var source in sources.TerminologySources)
            {
                var sourceHandler = source.Source;

                try
                {
                    _logger.LogInformation($"Fetching terminology from {source.SourcePath} using handler {sourceHandler.GetType().Name}");
                    var codeSystem = await sourceHandler.FetchAsync(source);

                    if (codeSystem == null) continue;

                    _logger.LogInformation($"Successfully fetched terminology from {source.SourcePath}. CodeSystem {sources.Name}, Values: {codeSystem.Values?.Length ?? 0}");

                    codeSystems.Add(codeSystem);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to fetch terminology from {source.SourcePath}, handler {sourceHandler.GetType().Name}");
                    ServiceStatus = ServiceState.Crashed;
                    throw;
                }
            }

            _terminologyService.AddOrUpdateCodeSystem(sources.Name, [.. codeSystems]);
        }
        ServiceStatus = ServiceState.Ready;
    }
}