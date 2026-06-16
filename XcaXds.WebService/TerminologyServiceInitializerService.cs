using XcaXds.Terminology.Services;

namespace XcaXds.WebService;

public class TerminologyServiceInitializerService : IHostedService
{
    private readonly ILogger<TerminologyServiceInitializerService> _logger;
    private readonly TerminologyUpdaterService _terminologyUpdaterService;

    public TerminologyServiceInitializerService(ILogger<TerminologyServiceInitializerService> logger, TerminologyUpdaterService terminologyUpdaterService)
    {
        _logger = logger;
        _terminologyUpdaterService = terminologyUpdaterService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing terminology service...");
        _terminologyUpdaterService.InitializeTerminologyServiceAsync(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping terminology service...");
        return Task.CompletedTask;
    }
}