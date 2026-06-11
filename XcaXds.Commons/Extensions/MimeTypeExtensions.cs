using System.Text;
using System.Text.Json;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Models.ClinicalDocument;
using XcaXds.Commons.Serializers;
using XcaXds.Shared.Commons;

namespace XcaXds.Commons.Extensions;

public static class MimeTypeExtensions
{
    public static bool TryGetMimeTypeFromDocumentBytes(byte[]? input, out string? mimeType, bool fromCdaNonXmlBody = false)
    {
        mimeType = null;

        // PNG: Starts with 0x89 0x50 0x4E 0x47
        if (input?.Length > 4 && input[0] == 0x89 && input[1] == 0x50 && input[2] == 0x4E && input[3] == 0x47)
            mimeType = Constants.MimeTypes.Png;

        // JPEG: Starts with 0xFF 0xD8 and ends with 0xFF 0xD9
        else if (input?.Length > 4 && input[0] == 0xFF && input[1] == 0xD8 && input[^2] == 0xFF && input[^1] == 0xD9)
            mimeType = Constants.MimeTypes.Jpeg;

        // GIF: Starts with "GIF87a" or "GIF89a"
        else if (input?.Length > 6 && input[0] == 0x47 && input[1] == 0x49 && input[2] == 0x46 && input[3] == 0x38 && (input[4] == 0x37 || input[4] == 0x39) && input[5] == 0x61)
            mimeType = Constants.MimeTypes.Gif;

        // TIFF: Starts with "II" (0x49 0x49) (little endian) or "MM" (0x4D 0x4D) (big endian)
        else if (input?.Length > 2 && input[0] == 0x49 && (input[1] == 0x49 || input[1] == 0x4D))
            mimeType = Constants.MimeTypes.Tiff;

        // PDF: Starts with %PDF-
        else if (input?.Length > 4 && input[0] == 0x25 && input[1] == 0x50 && input[2] == 0x44 && input[3] == 0x46)
            mimeType = Constants.MimeTypes.Pdf;

        // RTF: Starts with "{\\rtf"
        else if (input?.Length > 4 && input[0] == 0x7B && input[1] == 0x5C && input[2] == 0x72 && input[3] == 0x74 && input[4] == 0x66)
            mimeType = Constants.MimeTypes.TextRtf;

        // EXE: Starts with "0x4D 0x5A"
        else if (input?.Length > 4 && input[0] == 0x4D && input[1] == 0x5A)
            mimeType = Constants.MimeTypes.Exe;

        // ZIP (also DOCX, XLSX, PPTX, JAR, APK, etc.)
        else if (input?.Length > 3 && input[1] == 0x4B && (input[2] == 0x03 || input[2] == 0x05 || input[2] == 0x07) && (input[3] == 0x04 || input[3] == 0x06 || input[3] == 0x08))
            mimeType = Constants.MimeTypes.Zip;

        else if (IsXmlLike(input, out var kind))
            mimeType = Constants.MimeTypes.Xml;

        else if (IsJson(input))
            mimeType = Constants.MimeTypes.Json;

        // Plain text: All bytes are in the range of 32 (space) to 126 (~)
        // Note! Do this check last, to ensure other text-like mimetypes are covered (e.g., XML, JSON)
        else if (input?.Length > 4 && input.All(b => b >= 32 && b <= 126))
            mimeType = Constants.MimeTypes.Text;

        return string.IsNullOrWhiteSpace(mimeType) == false;
    }

    private static byte[]? GetClinicalDocumentDocument(byte[]? input)
    {
        var sxmls = new SoapXmlSerializer();
        var cdaDocument = sxmls.DeserializeXmlString<ClinicalDocument>(new MemoryStream(input ?? Array.Empty<byte>()));
        return Convert.FromBase64String(cdaDocument.Component.NonXmlBody?.Text.Data ?? "");
    }

    private static bool IsXmlLike(byte[]? input, out DocumentSniffer.DocumentKind kind)
    {
        kind = DocumentSniffer.DetectKind(input);
        return kind != DocumentSniffer.DocumentKind.Unknown;
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
}
