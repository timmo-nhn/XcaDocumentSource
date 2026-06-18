using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XcaXds.Terminology.Interfaces;
using XcaXds.Terminology.Models.Custom;
using XcaXds.Terminology.TerminologySources;
using XcaXds.Terminology.ValueSetMappers.Hl7;
using XcaXds.Terminology.ValueSetMappers.Norway;
using XcaXds.Terminology.ValueSetMappers.XcaDocumentSource;

namespace XcaXds.Terminology.Sources;

/// <summary>
/// This class defines a list of the sources of the Value sets that XcaDocumentSource will use
/// <para/>
/// Each source is defined by a code system name and a list of <see cref="TerminologySource{TMapper}"/> objects,
/// which include the source path and an implementation of the <see cref="ICodeSystemMapper"/>
/// that will be used to convert the content from the sourcePath to a ComprehensiveCodeSystem.
/// <para/>
/// The code systems can either be fetched from an API endpoint (<see cref="HttpTerminologySource"/>) or from a file (<see cref="FileTerminologySource"/>) or any other mechanism you can come up with :)
/// </summary>
public class TerminologySourcesRegistryService
{
    private readonly ILogger<TerminologySourcesRegistryService> _logger;
    private readonly ApplicationConfig _applicationConfig;
    private readonly FileTerminologySource _fileSource;
    private readonly HttpTerminologySource _httpSource;
    private readonly StringTerminologySource _stringSource;
    private readonly List<TerminologySourceDefinition> _cachedDefinitions;


    public TerminologySourcesRegistryService(
        ILogger<TerminologySourcesRegistryService> logger,
        IConfiguration configuration,
        ApplicationConfig applicationConfig,
        FileTerminologySource fileSource,
        HttpTerminologySource httpSource,
        StringTerminologySource stringSource)
    {
        _logger = logger;
        _applicationConfig = applicationConfig;
        _fileSource = fileSource;
        _httpSource = httpSource;
        _stringSource = stringSource;

        var terminologyConfiguration = configuration.GetSection("TerminologySources").Get<TerminologySourcesConfiguration>();

        if (terminologyConfiguration?.Definitions == null || terminologyConfiguration.Definitions.Count == 0)
        {
            throw new InvalidOperationException("TerminologySources configuration is missing or empty.");
        }

        _cachedDefinitions = [.. terminologyConfiguration.Definitions.Select(MapToRuntimeDefinition)];
        _logger.LogInformation("Loaded {Count} terminology definitions from configuration", _cachedDefinitions.Count);
    }

    public List<TerminologySourceDefinition> GetAllDefinitions() => _cachedDefinitions;

    private TerminologySourceDefinition MapToRuntimeDefinition(TerminologySourceDefinitionConfiguration configDefinition)
    {
        if (string.IsNullOrWhiteSpace(configDefinition.Name))
        {
            throw new InvalidOperationException("TerminologySources contains a definition with an empty name.");
        }

        if (configDefinition.Sources == null || configDefinition.Sources.Count == 0)
        {
            throw new InvalidOperationException($"TerminologySources definition '{configDefinition.Name}' contains no sources.");
        }

        return new TerminologySourceDefinition(
            configDefinition.Name,
            [.. configDefinition.Sources.Select(MapToRuntimeSource)]);
    }

    private TerminologySource<ITerminologySource, ICodeSystemMapper> MapToRuntimeSource(
        TerminologySourceConfiguration configSource)
    {
        if (string.IsNullOrWhiteSpace(configSource.Type))
        {
            throw new InvalidOperationException("Terminology source is missing required 'Type'.");
        }

        if (string.IsNullOrWhiteSpace(configSource.Mapper))
        {
            throw new InvalidOperationException("Terminology source is missing required 'Mapper'.");
        }

        if (string.IsNullOrWhiteSpace(configSource.SourcePath))
        {
            throw new InvalidOperationException("Terminology source is missing required 'SourcePath'.");
        }

        var source = ResolveSource(configSource.Type);
        var mapper = ResolveMapper(configSource.Mapper, configSource.MapperOptions);
        var sourcePath = ResolveSourcePath(configSource.SourcePath);

        return new(source, sourcePath, mapper);
    }

    private ITerminologySource ResolveSource(string sourceType)
    {
        return sourceType.ToLowerInvariant() switch
        {
            "file" => _fileSource,
            "http" => _httpSource,
            "string" => _stringSource,
            _ => throw new InvalidOperationException($"Unsupported terminology source type '{sourceType}'")
        };
    }

    private ICodeSystemMapper ResolveMapper(string mapperName, TerminologyMapperOptions? options)
    {
        return mapperName switch
        {
            nameof(FileBasedJsonMapper) => new FileBasedJsonMapper(),
            nameof(FinnKodeMapper) => new FinnKodeMapper(),
            nameof(FinnKodeClassCodeMapper) => new FinnKodeClassCodeMapper(),
            nameof(FinnKodeTypeCodeMapper) => new FinnKodeTypeCodeMapper(),
            nameof(Hl7FhirCodeSystemMapper) => string.IsNullOrWhiteSpace(options?.DisplayDiscriminator)
                ? new Hl7FhirCodeSystemMapper()
                : new Hl7FhirCodeSystemMapper(options.DisplayDiscriminator),
            nameof(StringBasedMapper) => new StringBasedMapper(options?.Separator, options?.System ??
                throw new InvalidOperationException("StringBasedMapper requires MapperOptions.System")),
            _ => throw new InvalidOperationException($"Unsupported terminology mapper '{mapperName}'")
        };
    }

    private string ResolveSourcePath(string sourcePath)
    {
        return sourcePath
            .Replace("{HomeCommunityId}", _applicationConfig.HomeCommunityId, StringComparison.Ordinal)
            .Replace("{RepositoryUniqueId}", _applicationConfig.RepositoryUniqueId, StringComparison.Ordinal);
    }
}