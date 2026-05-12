using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces.Statistics;
using XcaXds.Commons.Models.Custom.Statistics;

namespace XcaXds.WebService.Services.Statistics;

public class  MockStatisticsProcessorService : BackgroundService
{
    private readonly ILogger<MockStatisticsProcessorService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly StatisticsTransformerService _statisticsTransformerService;
    private readonly IStatisticsQueue _statisticsQueue;

    public static string? UserAccessEntryJson { get; private set; }


    public MockStatisticsProcessorService(ILogger<MockStatisticsProcessorService> logger, ApplicationConfig appConfig, StatisticsTransformerService statisticsTransformerService, IStatisticsQueue statisticsQueue)
    {
        _logger = logger;
        _appConfig = appConfig;
        _statisticsTransformerService = statisticsTransformerService;
        _statisticsQueue = statisticsQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StatisticsProcessorService started");

        await foreach (var requestAndFields in _statisticsQueue.Channel.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Received statistics item");

            try
            {
                var userAccessEntry = await _statisticsTransformerService.TransformToUserAccessEntry(requestAndFields);
                ExportStatistics(userAccessEntry);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break; // normal shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform/export statistics item");
            }
        }
    }

    private void ExportStatistics(UserAccessEntry userAccessEntry)
    {
        _logger.LogInformation("User Access Entry");
        _logger.LogInformation("{@UserAccessEntry}", userAccessEntry);
        UserAccessEntryJson = JsonSerializer.Serialize(userAccessEntry, Constants.JsonDefaultOptions.DefaultSettingsInline);
    }
}
