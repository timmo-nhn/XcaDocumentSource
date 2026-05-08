using System.Threading.Channels;
using XcaXds.Commons.Interfaces.Statistics;

namespace XcaXds.Commons.Models.Custom.Statistics;

public class StatisticsQueue : IStatisticsQueue
{
    public Channel<StatisticsRequestAndFields> Channel { get; } =
        System.Threading.Channels.Channel.CreateUnbounded<StatisticsRequestAndFields>();
}
