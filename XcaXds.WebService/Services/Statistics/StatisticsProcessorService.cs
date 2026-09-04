using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Interfaces.Statistics;
using XcaXds.Commons.Models.Custom.Statistics;
using XcaXds.Shared;

namespace XcaXds.WebService.Services.Statistics;

public class StatisticsProcessorService : BackgroundService
{
    private readonly ILogger<StatisticsProcessorService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly StatisticsTransformerService _statisticsTransformerService;
    private readonly IStatisticsQueue _statisticsQueue;
    private readonly IStatisticsExporter _statisticsExporter;

    public StatisticsProcessorService(ILogger<StatisticsProcessorService> logger, ApplicationConfig appConfig, StatisticsTransformerService statisticsTransformerService, IStatisticsQueue statisticsQueue, IStatisticsExporter statisticsExporter)
    {
        _logger = logger;
        _appConfig = appConfig;
        _statisticsTransformerService = statisticsTransformerService;
        _statisticsQueue = statisticsQueue;
        _statisticsExporter = statisticsExporter;
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
                await ExportStatistics(userAccessEntry, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StatisticsProcessorService crashed");
            throw;
        }
    }

    private async Task ExportStatistics(UserAccessEntry userAccessEntry, CancellationToken cancellationToken)
    {
        var jsonAccessEntry = JsonSerializer.Serialize(userAccessEntry, Constants.JsonDefaultOptions.DefaultSettings);
        _logger.LogInformation("User Access Entry generated. Exporting...");
        _logger.LogDebug("User Access Entry: \n{json}", jsonAccessEntry);

        try
        {
            await _statisticsExporter.ExportAsync(userAccessEntry, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export UserAccessEntry to statistics store");
        }
    }
}
