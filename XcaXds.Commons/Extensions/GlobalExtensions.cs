namespace XcaXds.Commons.Extensions;

public static class GlobalExtensions
{
    public static TResult? TryThis<TResult>(Func<TResult?> action, out bool success, out Exception? exception)
    {
        success = false;
        exception = null;
        try
        {
            var result = action();
            success = true;
            return result;
        }
        catch (Exception ex)
        {
            exception = ex;
            success = false;
            return default;
        }
    }

    public static int TicksToSeconds(this long value)
    {
        return (int)value / 10_000_000;
    }

    public static bool IsNullOrZero(this int? value)
    {
        return value is null or 0;
    }

    public static bool IsNullOrZero(this int value)
    {
        return value == 0;
    }

    public static bool IsEven(this int value)
    {
        return value % 2 == 0;
    }
}