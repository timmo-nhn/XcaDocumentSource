using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces.Statistics;
using XcaXds.Commons.Models.Custom.Statistics;

namespace XcaXds.WebService.Services.Statistics;

public class StatisticsProcessorService : BackgroundService
{
    private readonly ILogger<StatisticsProcessorService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly StatisticsTransformerService _statisticsTransformerService;
    private readonly IStatisticsQueue _statisticsQueue;

    public StatisticsProcessorService(ILogger<StatisticsProcessorService> logger, ApplicationConfig appConfig, StatisticsTransformerService statisticsTransformerService, IStatisticsQueue statisticsQueue)
    {
        _logger = logger;
        _appConfig = appConfig;
        _statisticsTransformerService = statisticsTransformerService;
        _statisticsQueue = statisticsQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StatisticsProcessorService started");

        try
        {
            await foreach (var requestAndFields in _statisticsQueue.Channel.Reader.ReadAllAsync(stoppingToken))
            {
                _logger.LogInformation("Received statistics item");

                var userAccessEntry = await _statisticsTransformerService.TransformToUserAccessEntry(requestAndFields);
                ExportStatistics(userAccessEntry);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StatisticsProcessorService crashed");
            throw;
        }
    }
    private void ExportStatistics(UserAccessEntry userAccessEntry)
    {
        var jsonAccessEntry = JsonSerializer.Serialize(userAccessEntry, Constants.JsonDefaultOptions.DefaultSettings);
        _logger.LogInformation("User Access Entry:\n" + jsonAccessEntry);
    }
}
