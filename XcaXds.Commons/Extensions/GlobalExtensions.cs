namespace XcaXds.Commons.Extensions;

public static class GlobalExtensions
{
    public static bool TryThis(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool IsNullOrZero(this int? value)
    {
        return value == null || value == 0;
    }

}