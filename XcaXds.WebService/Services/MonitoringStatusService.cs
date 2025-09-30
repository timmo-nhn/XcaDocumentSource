using XcaXds.Commons.Commons;

namespace XcaXds.WebService.Services;

public class MonitoringStatusService
{
    public BoundedDictionary<string,long> ResponseTimes { get; set; }
    public MonitoringStatusService()
    {
        ResponseTimes = new();
    }
}