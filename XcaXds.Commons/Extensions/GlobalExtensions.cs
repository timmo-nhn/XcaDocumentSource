namespace XcaXds.Commons.Extensions;

public static class GlobalExtensions
{
    public static bool TryThis(Action action, out Exception? exception)
    {
        exception = null;
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }
    }

    public static bool IsNullOrZero(this int? value)
    {
        return value == null || value == 0;
    }
}