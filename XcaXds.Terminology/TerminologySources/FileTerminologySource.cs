using Microsoft.Extensions.Logging;
using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Interfaces;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.TerminologySources;

public class FileTerminologySource : ITerminologySource
{
    private readonly ILogger<FileTerminologySource> _logger;

    private readonly string _basePath;

    public FileTerminologySource(ILogger<FileTerminologySource> logger)
    {
        _logger = logger;

        // When running in a container the path will be different
        var customPath = Environment.GetEnvironmentVariable("OFFLINE_CODE_SYSTEMS_FILE_PATH");

        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _basePath = customPath;
        }
        else
        {
            string baseDirectory = AppContext.BaseDirectory;
            _basePath = Path.Combine(baseDirectory, "..", "..", "..", "..", "XcaXds.Terminology", "OfflineCodeSystems");
        }

        _basePath = Path.GetFullPath(_basePath);

        Directory.CreateDirectory(_basePath);

        _logger.LogInformation($"OfflineCodeSystems repository path: {_basePath}");
    }

    public async Task<ComprehensiveCodeSystem?> FetchAsync(TerminologySource<ITerminologySource, ICodeSystemMapper> terminologySource)
    {
        var filePath = Path.Combine(_basePath, terminologySource.SourcePath);
        var content = await File.ReadAllTextAsync(filePath);

        _logger.LogDebug($"Read content from file {filePath}. Mapping to ComprehensiveCodesystem");

        return terminologySource.MapperToUse.MapToComprehensiveCodeSystem(content);
    }
}