namespace XcaXds.Commons.Extensions;

public static class StringExtensions
{
    public static string NoUrn(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input; 
        }

        return input.Replace("urn:uuid:", "");
    }
}
