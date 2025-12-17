namespace XcaXds.Commons.Extensions;

public static class DateTimeExtensions
{
    public static DateTime TruncateMilliseconds(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
    }

    public static DateTime ParseNinToDateTime(string patientId)
    {
        throw new NotImplementedException();
    }
}
