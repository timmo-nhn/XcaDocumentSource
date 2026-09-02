using XcaXds.Commons.Models.Custom.Statistics;

namespace XcaXds.Commons.Interfaces;

public interface IStatisticsExporter
{
    Task ExportAsync(UserAccessEntry userAccessEntry, CancellationToken cancellationToken = default);
}
