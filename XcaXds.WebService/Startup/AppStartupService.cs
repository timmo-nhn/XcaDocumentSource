
namespace XcaXds.WebService.Startup;

public class AppStartupService : IHostedService
{

    private readonly ILogger<AppStartupService> _logger;
    private readonly IHostEnvironment _env;
    private readonly IConfiguration _config;

    private readonly ApplicationConfig _appConfig;

    public AppStartupService(ILogger<AppStartupService> logger, IHostEnvironment env, IConfiguration config, ApplicationConfig appConfig)
    {
        _logger = logger;
        _env = env;
        _config = config;
        _appConfig = appConfig;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_env.IsProduction())
        {
            if (_appConfig.HomeCommunityId == "2.16.578.1.12.4.5.100.1")
            {
                _logger.LogCritical($"\n\n========  Fatal! Default HomeCommunityId in production =======\nDefault HomeCommunity Id {_appConfig.HomeCommunityId}! \nWhen deploying the application, please change this to an unique OID\n\n");
                throw new InvalidOperationException("Default HomeCommunityId used in production environment.");
            }

            if (_appConfig.RepositoryUniqueId == "2.16.578.1.12.4.5.100.1.2")
            {
                _logger.LogCritical($"\n\n========  Fatal! Default RepositoryUniqueId in production =======\nUsing default Repository Unique Id {_appConfig.RepositoryUniqueId}!\nWhen deploying the application, please change this to an unique OID\n\n");
                throw new InvalidOperationException("Default HomeCommunityId used in production environment.");
            }

            _logger.LogWarning($"\n\n========  Warning! Default HomeCommunityId =======\nUsing default HomeCommunity Id {_appConfig.HomeCommunityId}! \nWhen deploying the application, please change this to an unique OID\n\n");
        }

        _logger.LogInformation("Starting XcaDocumentSource...");

        if (_appConfig.HomeCommunityId == "2.16.578.1.12.4.5.100.1")
        {
            _logger.LogWarning($"\n\n========  Warning! Default HomeCommunityId =======\nUsing default HomeCommunity Id {_appConfig.HomeCommunityId}! \nWhen deploying the application, please change this to an unique OID\n\n");
        }

        if (_appConfig.RepositoryUniqueId == "2.16.578.1.12.4.5.100.1.2")
        {
            _logger.LogWarning($"\n\n========  Warning! Default RepositoryUniqueId =======\nUsing default Repository Unique Id {_appConfig.RepositoryUniqueId}!\nWhen deploying the application, please change this to an unique OID\n\n");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping XcaDocumentSource...");
        return Task.CompletedTask;
    }
}
