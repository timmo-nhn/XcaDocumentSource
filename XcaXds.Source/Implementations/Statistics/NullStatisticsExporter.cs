using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.Statistics;

namespace XcaXds.Source.Implementations.Statistics;

/// <summary>
/// No-op statistics exporter used when no PostgreSQL connection string is configured.
/// </summary>
public class NullStatisticsExporter : IStatisticsExporter
{
    public Task ExportAsync(UserAccessEntry userAccessEntry, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
