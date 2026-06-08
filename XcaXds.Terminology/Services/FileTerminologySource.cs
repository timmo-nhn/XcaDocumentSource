using System.Text.Json;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.Services;

public class FileTerminologySource : ITerminologySource
{
    private readonly string _basePath;

    public FileTerminologySource(string basePath)
    {
        _basePath = basePath;
    }

    public async Task<ComprehensiveCodeSystem> FetchAsync(string sourceIdentifier)
    {
        var filePath = Path.IsPathRooted(sourceIdentifier)
            ? sourceIdentifier
            : Path.Combine(_basePath, sourceIdentifier);

        var content = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<ComprehensiveCodeSystem>(content);
    }
}
