using System.Text;

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

        // The \u0027 is to handle cases where the string is encoded with single quotes (such as in GetDocumentAssociations with a list of UUIDs)
        /* Example content from AdhocQuery received from XCA (for GetDocumentAssociations): 
		   <ns2:Slot name="$uuid">
                <ns2:ValueList>
                    <ns2:Value>(\u0027urn:uuid:0ae98a90-f5ef-4bde-b717-dc0119a5777f\u0027)</ns2:Value>
                    <ns2:Value>(\u0027urn:uuid:2690c091-e46f-4c7e-9742-6572cc455355\u0027)</ns2:Value>
                <ns2:ValueList>
            </ns2:Slot>
		*/
        return input.Replace("\\u0027", "").Replace("urn:uuid:", "").Replace("urn:oid:", "");
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

    public static string? GetMimetypeFromMagicNumber(byte[] input)
    {

        // Make sure the input is large enough to check for magic numbers
        if (input.Length < 4)
        {
            throw new ArgumentException("Input file is too small to detect MIME type.");
        }

        return input[0] switch
        {
            // Check for JPEG: Starts with 0xFF 0xD8 and ends with 0xFF 0xD9
            0xFF => (input[1] == 0xD8 && input[input.Length - 2] == 0xFF && input[input.Length - 1] == 0xD9)
                    ? "image/jpeg" : null,

            // Check for PNG: Starts with 0x89 0x50 0x4E 0x47
            0x89 => (input[1] == 0x50 && input[2] == 0x4E && input[3] == 0x47)
                    ? "image/png" : null,

            // Check for PDF: Starts with %PDF-
            0x25 => (input[1] == 0x50 && input[2] == 0x44 && input[3] == 0x46)
                    ? "application/pdf" : null,

            // Check for GIF: Starts with "GIF87a" or "GIF89a"
            0x47 => (input[1] == 0x49 && input[2] == 0x46 &&
                     (input[3] == 0x38 && (input[4] == 0x37 || input[4] == 0x39) && input[5] == 0x61))
                    ? "image/gif" : null,

            // TIFF: Starts with "II" (0x49 0x49) or "MM" (0x4D 0x4D)
            0x49 => (input[1] == 0x49 || input[1] == 0x4D)
                    ? "image/tiff" : null,

            // RTF: Starts with "{\\rtf"
            0x7B => (input[1] == 0x5C && input[2] == 0x72 && input[3] == 0x74 && input[4] == 0x66)
                    ? "application/rtf" : null,

            // ClinicalDocument: Starts with "<ClinicalDocument"
            0x3C => IsClinicalDocument(input) ? "application/hl7-v3+xml" : null,

            // Check for TXT: All characters in range of printable ASCII
            _ => input.All(b => b >= 32 && b <= 126) ? "text/plain" : null,
        };
    }

    public static bool IsAnyOf(this string value, params string[] options)
    {
        if (string.IsNullOrWhiteSpace(value) || options == null) return false;

        return options.Contains(value);
    }

    public static bool IsNotAnyOf(this string value, params string[] options)
    {
        if (string.IsNullOrWhiteSpace(value) || options == null) return false;

        return !options.Contains(value);
    }

    private static bool IsClinicalDocument(byte[] input)
    {
        // Ensure input is long enough to hold "<ClinicalDocument"
        const string marker = "<ClinicalDocument";
        if (input.Length < marker.Length)
            return false;

        var ascii = Encoding.ASCII.GetBytes(marker);
        for (int i = 0; i < ascii.Length; i++)
        {
            if (input[i] != ascii[i])
                return false;
        }

        return true;
    }
}