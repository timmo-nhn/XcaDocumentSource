using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Channels;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Statistics;
using XcaXds.Commons.Models.Custom.Statistics;
using XcaXds.WebService.Middleware;

namespace XcaXds.WebService.Services;

public class StatisticsProcessorService : BackgroundService
{
    private readonly ILogger<StatisticsProcessorService> _logger;

    public StatisticsProcessorService(ILogger<StatisticsProcessorService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var soapEnvelopeAndFields in SoapServiceStatisticsMiddleware.RawStatisticsOutputChannel.Reader.ReadAllAsync(CancellationToken.None))
        {
            var userAccessEntry = StatisticsTransformer.TransformToUserAccessEntry(soapEnvelopeAndFields);

            ExportStatistics(userAccessEntry);
        }
    }

    private void ExportStatistics(UserAccessEntry userAccessEntry)
    {
        _logger.LogInformation("User Access Entry:");
        _logger.LogInformation(JsonSerializer.Serialize(userAccessEntry, Constants.JsonDefaultOptions.DefaultSettings));
    }
}
