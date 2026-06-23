using System.Text;
using System.Text.RegularExpressions;

namespace XcaXds.Shared.Extensions;

public static class StringExtensions
{
	private static readonly Regex OidRegex = new(@"^\d+(\.\d+)*$", RegexOptions.Compiled);

	/// <summary>
	/// Remove "urn:uuid:" and "urn:oid:" on the string
	/// </summary>
	public static string NoUrn(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input.Replace("urn:uuid:", "").Replace("urn:oid:", "");
    }

    /// <summary>
    /// Prepend "urn:oid:" on the string
    /// </summary>
    public static string WithUrnOid(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // fyi: some other values I've come across during testing: 
        // input which contains "http://"
        // input which contains "*****" (restricted documents), sperrede dokumenter 
        // input which is set to "Ukjent"
        // input which is set to "infoflyt-api-test", seen for Usnobbet klokke (17855599120):

        input = input.NoUrn(); 

		if (OidRegex.IsMatch(input))
		{
			input = "urn:oid:" + input;
		}

		return input;
    }

    /// <summary>
    /// Prepend "urn:uuid:" on the string
    /// </summary>
    public static string WithUrnUuid(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return $"urn:uuid:{input.NoUrn()}";
    }

    public static byte[] GetAsUtf8Bytes(this string input)
    {
        return Encoding.UTF8.GetBytes(input);
    }

    public static bool IsAnyOf(this string? value, params string?[] options)
    {
        if (string.IsNullOrWhiteSpace(value) || options == null) return false;

        return options.OfType<string>().Contains(value);
    }
}