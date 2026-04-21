using System.Text;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Models.ClinicalDocument;
using XcaXds.Commons.Serializers;

namespace XcaXds.Commons.Extensions;

public static class StringExtensions
{
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

        return $"urn:oid:{input.NoUrn()}";
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

    public static bool IsAnyOf(this string? value, params string[] options)
    {
        if (string.IsNullOrWhiteSpace(value) || options == null) return false;

        return options.Contains(value);
    }
}