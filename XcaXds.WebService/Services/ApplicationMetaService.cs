using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.Shared.Extensions;
using XcaXds.WebService.Models.Custom;
using XcaXds.WebService.Services.XdsRegistry;
using XcaXds.WebService.Services.XdsRepository;

namespace XcaXds.WebService.Services;

public class ApplicationMetaService
{
    private readonly ILogger<ApplicationMetaService> _logger;
    private readonly ApplicationConfig _applicationConfig;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly RegistryWrapper _registryWrapper;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApplicationMetaService(
        ILogger<ApplicationMetaService> logger,
        ApplicationConfig applicationConfig,
        RepositoryWrapper repositoryWrapper,
        RegistryWrapper registryWrapper,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _applicationConfig = applicationConfig;
        _repositoryWrapper = repositoryWrapper;
        _registryWrapper = registryWrapper;
        _httpClientFactory = httpClientFactory;
    }

    public string GetNukeKeyForRegistryRepository()
    {
        return DateTime.UtcNow.ToString("ddMMyyhhMM");
    }

    public RestfulApiResponse NukeRegistryRepository(string nukeKey)
    {
        var apiResponse = new RestfulApiResponse();

        var datetime = DateTime.UtcNow.ToString("ddMMyyhhMM");
        if (datetime != nukeKey)
        {
            apiResponse.AddError("InvalidKey", "Invalid Nuke key, get nuke key from the 'get-nuke-key'-endpoint");
            return apiResponse;
        }

        var documentEntries = _registryWrapper.GetDocumentRegistryContentAsDtos().ToArray();
        var documentIds = documentEntries.OfType<DocumentEntryDto>().Select(dent => dent.UniqueId).ToList();

        var amount = documentIds.Count;
        _logger.LogInformation("{traceIdentifier} - Fetched {amount} for nuking", Guid.NewGuid().ToString(), amount);

        if (amount == 0)
        {
            apiResponse.SetMessage("Nothing to nuke");
            return apiResponse;
        }

        documentIds.ForEach(docid => _repositoryWrapper.DeleteSingleDocument(docid));
        _registryWrapper.SetDocumentRegistryContentWithDtos([]);

        apiResponse.SetMessage($"Nuked {amount} documents from registry and repository");
        return apiResponse;
    }

    public async Task<HealthCheckResult> AtnaLogHealthCheck()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{StringExtensions.GetHostFromUrl(_applicationConfig.AtnaLogExporterEndpoint)}/healthz");
            var content = await response.Content.ReadAsStringAsync();

            var report = JsonSerializer.Deserialize<AtnaLogExporterHealthReport>(content, _jsonSerializerOptions);

            var status = report?.Status ?? HealthStatus.Unhealthy;
            var data = new Dictionary<string, object> { ["StatusCode"] = (int)response.StatusCode };

            if (report?.Entries is { } entries)
            {
                data["Entries"] = entries;
            }

            return new HealthCheckResult(status, description: $"AtnaLogExporter reported status: {status}", data: data);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex.ToString());
            return new HealthCheckResult(HealthStatus.Unhealthy, description: "AtnaLogExporter did not respond to the health check request", exception: ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex.ToString());
            return new HealthCheckResult(HealthStatus.Unhealthy, description: "AtnaLogExporter returned a health check response that could not be parsed", exception: ex);
        }
    }
}

file record AtnaLogExporterHealthReport(HealthStatus Status, TimeSpan TotalDuration, Dictionary<string, AtnaLogExporterHealthReportEntry> Entries);

file record AtnaLogExporterHealthReportEntry(HealthStatus Status, string Description, TimeSpan Duration, string Exception, object Data);
