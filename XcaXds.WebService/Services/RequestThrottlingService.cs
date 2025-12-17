namespace XcaXds.WebService.Services;

/// <summary>
/// Service to intentionally throttle response times for debugging purposes
/// </summary>
public class RequestThrottlingService
{
    private readonly object _lock = new();
    private int _throttleTimeMillis { get; set; }

    private DateTime _throttleTimeSet { get; set; }
    private int _throttleTimeDuration { get; set; }


    public bool IsThrottleTimeSet()
    {
        lock (_lock)
        {
            return _throttleTimeMillis != 0;
        }
    }

    public int GetThrottleTime()
    {
        lock (_lock)
        {
            if (DateTime.UtcNow >= _throttleTimeSet.AddSeconds(_throttleTimeDuration))
            {
                _throttleTimeMillis = 0;
            }

            return _throttleTimeMillis;
        }
    }

    public void SetThrottleTime(int input, int duration)
    {
        lock (_lock)
        {
            _throttleTimeSet = DateTime.UtcNow;
            _throttleTimeDuration = duration;

            if (input >= 0)
            {
                _throttleTimeMillis = input;
            }
        }
    }
}