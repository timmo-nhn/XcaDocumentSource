using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace XcaXds.Commons.Models.Custom;

public class SoapEnvelopeMultipartReader
{
    private string _boundary;
    private Stream _stream;
    private StreamReader _reader;
    private string _currentSectionContentId;

    private int _currentSection;

    public SoapEnvelopeMultipartReader(string boundary, Stream stream)
    {
        _boundary = boundary;
        _stream = stream;
        _reader = new StreamReader(stream, leaveOpen:true);
    }

    public async Task<MultipartSection?> ReadNextSectionAsync()
    {
        var line = await _reader.ReadLineAsync();
        if (line == null) return null;

        var sanitizedLine = line.Trim('-');

        if (sanitizedLine.Equals(_boundary))
        {
            _currentSection++;
            var wholeSection = ReadUntilBoundaryHit(_reader, _boundary, out var contentId, out var isFinalBoundary);
            return new MultipartSection()
            {
                ContentId = contentId,
                Section = Encoding.UTF8.GetBytes(wholeSection)
            };
        }

        return null;
    }

    private string ReadUntilBoundaryHit(StreamReader reader, string boundary, out string? contentId, out bool isFinalBoundary)
    {
        contentId = null;
        isFinalBoundary = false;

        var inHeaders = true;
        var sb = new StringBuilder();

        var boundaryLine = $"--{boundary}";
        var finalBoundaryLine = $"--{boundary}--";

        while (reader.ReadLine() is { } line)
        {
            if (line == boundaryLine)
                break;

            if (line == finalBoundaryLine)
            {
                isFinalBoundary = true;
                break;
            }

            if (inHeaders)
            {
                if (line.StartsWith("Content-ID:", StringComparison.OrdinalIgnoreCase))
                {
                    contentId = line["Content-ID:".Length..]
                        .Trim()
                        .Trim('<', '>');
                }
                
                // MIME Headers and Body should be separated by an empty line  
                if (string.IsNullOrEmpty(line))
                {
                    inHeaders = false;
                }
            }
            else
            { 
                sb.AppendLine(line);
            }
        }

        return sb.ToString();
    } 
}