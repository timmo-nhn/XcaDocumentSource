using XcaXds.Commons.Commons;

namespace XcaXds.WebService.Services;

/// <summary>
/// Depencency injected service to intentionally throttle response times
/// </summary>
public class RequestThrottlingService
{
    private int ThrottleTimeMillis { get; set; }

    public bool IsThrottleTimeSet()
    {
        return ThrottleTimeMillis != 0;
    }

    public int GetThrottleTime()
    {
        return ThrottleTimeMillis;
    }

    public void SetThrottleTime(int input)
    {
        if (input >= 0)
        {
            ThrottleTimeMillis = input;
        }
    }
}