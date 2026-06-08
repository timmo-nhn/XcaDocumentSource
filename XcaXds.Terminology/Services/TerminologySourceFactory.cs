namespace XcaXds.Terminology.Services;

public class TerminologySourceFactory
{
    private readonly HttpTerminologySource _httpSource;
    private readonly FileTerminologySource _fileSource;

    public TerminologySourceFactory(HttpTerminologySource httpSource, FileTerminologySource fileSource)
    {
        _httpSource = httpSource;
        _fileSource = fileSource;
    }

    public ITerminologySource GetSource(string sourceIdentifier)
    {
        if (sourceIdentifier.StartsWith("http://") || sourceIdentifier.StartsWith("https://"))
            return _httpSource;

        return _fileSource;
    }
}
