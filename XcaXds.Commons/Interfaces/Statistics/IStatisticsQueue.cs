using System.Threading.Channels;
using XcaXds.Commons.Models.Custom.Statistics;

namespace XcaXds.Commons.Interfaces.Statistics;

public interface IStatisticsQueue
{
    Channel<StatisticsRequestAndFields> Channel { get; }
}
