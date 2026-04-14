using System.Text;
using System.Text.Json;
using XcaXds.Commons.Commons;

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

    public static string? GetMimeTypeFromMagicByte(byte[]? input)
    {
        // PNG: Starts with 0x89 0x50 0x4E 0x47
        if (input?.Length > 4 && input[0] == 0x89 && input[1] == 0x50 && input[2] == 0x4E && input[3] == 0x47)
            return Constants.MimeTypes.Png;

        // JPEG: Starts with 0xFF 0xD8 and ends with 0xFF 0xD9
        if (input?.Length > 4 && input[0] == 0xFF && input[1] == 0xD8 && input[input.Length - 2] == 0xFF && input[input.Length - 1] == 0xD9)
            return Constants.MimeTypes.Jpeg;

        // GIF: Starts with "GIF87a" or "GIF89a"
        if (input?.Length > 6 && input[0] == 0x47 && input[1] == 0x49 && input[2] == 0x46 && input[3] == 0x38 && (input[4] == 0x37 || input[4] == 0x39) && input[5] == 0x61)
            return Constants.MimeTypes.Gif;

        // TIFF: Starts with "II" (0x49 0x49) (little endian) or "MM" (0x4D 0x4D) (big endian)
        if (input?.Length > 2 && input[0] == 0x49 && (input[1] == 0x49 || input[1] == 0x4D))
            return Constants.MimeTypes.Tiff;

        // PDF: Starts with %PDF-
        if (input?.Length > 4 && input[0] == 0x25 && input[1] == 0x50 && input[2] == 0x44 && input[3] == 0x46)
            return Constants.MimeTypes.Pdf;

        // Plain text: All bytes are in the range of 32 (space) to 126 (~)
        if (input?.Length > 4 && input.All(b => b >= 32 && b <= 126))
            return Constants.MimeTypes.Text;

        // RTF: Starts with "{\\rtf"
        if (input?.Length > 4 && input[0] == 0x7B && input[1] == 0x5C && input[2] == 0x72 && input[3] == 0x74 && input[4] == 0x66)
            return Constants.MimeTypes.Rtf;

        // EXE: Starts with "0x4D 0x5A"
        if (input?.Length > 4 && input[0] == 0x4D && input[1] == 0x5A)
            return Constants.MimeTypes.Exe;

        // ZIP (also DOCX, XLSX, PPTX, JAR, APK, etc.)
        if (input?.Length > 3 && input[1] == 0x4B && (input[2] == 0x03 || input[2] == 0x05 || input[2] == 0x07) && (input[3] == 0x04 || input[3] == 0x06 || input[3] == 0x08))
            return Constants.MimeTypes.Zip;

        if (IsEqualToString(input, "<ClinicalDocument"))
            return Constants.MimeTypes.Hl7v3Xml;

        if (IsEqualToString(input, "<?xml version="))
            return Constants.MimeTypes.Xml;

        if (IsJson(input))
            return Constants.MimeTypes.Json;

        return null;
    }

    private static bool IsJson(byte[]? input)
    {
        try
        {
            JsonDocument.Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsEqualToString(byte[]? input, string marker)
    {
        if (input == null || input.Length < marker.Length)
            return false;

        var ascii = Encoding.ASCII.GetBytes(marker);
        for (int i = 0; i < ascii.Length; i++)
        {
            if (input[i] != ascii[i])
                return false;
        }

        return true;
    }

    public static bool IsAnyOf(this string? value, params string[] options)
    {
        if (string.IsNullOrWhiteSpace(value) || options == null) return false;

        return options.Contains(value);
    }
}