using XcaXds.Commons.Models.Hl7.DataType;

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

    public static string GetMimetypeFromMagicNumber(byte[] input)
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
                    ? "image/jpeg" : "application/octet-stream",

            // Check for PNG: Starts with 0x89 0x50 0x4E 0x47
            0x89 => (input[1] == 0x50 && input[2] == 0x4E && input[3] == 0x47)
                    ? "image/png" : "application/octet-stream",

            // Check for PDF: Starts with %PDF-
            0x25 => (input[1] == 0x50 && input[2] == 0x44 && input[3] == 0x46)
                    ? "application/pdf" : "application/octet-stream",

            // Check for GIF: Starts with "GIF87a" or "GIF89a"
            0x47 => (input[1] == 0x49 && input[2] == 0x46 &&
                     (input[3] == 0x38 && (input[4] == 0x37 || input[4] == 0x39) && input[5] == 0x61))
                    ? "image/gif" : "application/octet-stream",

            // TIFF: Starts with "II" (0x49 0x49) or "MM" (0x4D 0x4D)
            0x49 => (input[1] == 0x49 || input[1] == 0x4D)
                    ? "image/tiff" : "application/octet-stream",

            // RTF: Starts with "{\\rtf"
            0x7B => (input[1] == 0x5C && input[2] == 0x72 && input[3] == 0x74 && input[4] == 0x66)
                    ? "application/rtf" : "application/octet-stream",


            // Check for TXT: All characters in range of printable ASCII
            _ => input.All(b => b >= 32 && b <= 126) ? "text/plain" : "application/octet-stream",
        };
    }
}
