using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace XcaXds.Commons.Models.Custom;

public class SoapEnvelopeMultipartReader : IDisposable
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
        _reader = new StreamReader(stream);
    }

    public async Task<MultipartSection?> ReadNextSectionAsync()
    {
        var line = await _reader.ReadLineAsync();
        if (line == null) return null;
        
        var sanitizedLine = line.Trim('-');

        if (sanitizedLine.Equals(_boundary))
        {
            _currentSection++;
            var wholeSection = ReadUntilBoundaryIsHit(_reader, _boundary, out var contentId);
            return new MultipartSection()
            {
                ContentId =  contentId,
                Section = Encoding.UTF8.GetBytes(wholeSection)
            };
        }

        return null;
    }

    private string ReadUntilBoundaryIsHit(StreamReader reader, string boundary, out string? contentId)
    {
        contentId = null;
        bool inHeader = true;
        bool foundCid = false;
        
        var sb = new StringBuilder();
        while (reader.ReadLine() is { } line && !line.Trim('-').Equals(boundary))
        {
            if (inHeader)
            {
                if (line.StartsWith("Content-ID: "))
                {
                    contentId = line["Content-ID: ".Length..].Trim('<').Trim('>');
                    foundCid = true;
                }
            }

            if (line == "" && foundCid)
            {
                inHeader = false;
            }
            
            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    public void Dispose()
    {
        _stream.Dispose();
        _reader.Dispose();
    }
}