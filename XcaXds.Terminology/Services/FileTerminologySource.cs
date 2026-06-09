using System.Text.Json;
using XcaXds.Terminology.Mappers;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.Services;

public class FileTerminologySource : ITerminologySource
{
    private readonly string _basePath;

    public FileTerminologySource()
    {
        _basePath = AppContext.BaseDirectory;
    }

    public async Task<ComprehensiveCodeSystem?> FetchAsync(TerminologySource<ICodeSystemMapper> terminologySource)
    {
        var filePath = Path.IsPathRooted(terminologySource.SourcePath)
            ? terminologySource.SourcePath
            : Path.Combine(_basePath, terminologySource.SourcePath);
        var content = await File.ReadAllTextAsync(filePath);

        return terminologySource.MapperToUse.MapToComprehensiveCodeSystem(content);
    }
}