using XcaXds.Commons.Commons;
using XcaXds.Shared;
using XcaXds.Shared.Extensions;

namespace XcaXds.WebService.Services;

public class MonitoringStatusService
{
    public DateTimeOffset StartupTime { get; set; }
    public DateTimeOffset LastAtnalogEligibleRequest { get; set; }
    public DateTimeOffset LastAtnaLogExported { get; set; }

    private BoundedDictionary<string, long>? _responseTimes;

    public BoundedDictionary<string, long> ResponseTimes 
    { 
        get { return _responseTimes ?? new(); }
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (_responseTimes != null)
            {
                _responseTimes.Updated -= OnResponseTimesUpdated;
            }

            _responseTimes = value;
            _responseTimes.Updated += OnResponseTimesUpdated;
        }
    }

    public MonitoringStatusService()
    {
        ResponseTimes = new();
    }

    private void OnResponseTimesUpdated(object? sender, BoundedDictionaryItemAddedEventArgs<string, long> eventArgs)
    {
        if (eventArgs.Item.Key.IsAnyOf(
            Constants.Urn.Custom.PepDeny,
            Constants.Urn.Custom.PepPermit)) return;

        LastAtnalogEligibleRequest = DateTimeOffset.UtcNow;
    }
}