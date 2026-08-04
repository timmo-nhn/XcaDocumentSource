using Microsoft.Extensions.Diagnostics.HealthChecks;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.WebService.Services.XdsRegistry;
using XcaXds.WebService.Services.XdsRepository;

namespace XcaXds.WebService.Services;

public class SourceHealthCheckService
{
    private readonly ILogger<SourceHealthCheckService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly MonitoringStatusService _monitoringStatusService;
    private readonly HealthCheckService _healthCheckService;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;

    public SourceHealthCheckService(
        ILogger<SourceHealthCheckService> logger,
        ApplicationConfig appConfig,
        MonitoringStatusService monitoringStatusService,
        HealthCheckService healthCheckService,
        RegistryWrapper registryWrapper,
        RepositoryWrapper repositoryWrapper)
    {
        _logger = logger;
        _appConfig = appConfig;
        _monitoringStatusService = monitoringStatusService;
        _healthCheckService = healthCheckService;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<HealthReport> CheckHealthAsync()
    {
        return await _healthCheckService.CheckHealthAsync();
    }

    public async Task CheckAtnaLogExport()
    {
        var lastRequest = _monitoringStatusService.LastRequest;
        var lastAtnalogExport = _monitoringStatusService.LastAtnaLogExported;
    }

    public async Task<SourceStatus> GetRegistryRepositoryStatus()
    {
        var registryReadOk = false;
        var registryWriteOk = false;
        var repositoryReadOk = false;
        var repositoryWriteOk = false;

        try
        {
            var documentEntry = new DocumentEntryDto()
            {
                UniqueId = Guid.NewGuid().ToString(),
                HomeCommunityId = _appConfig.HomeCommunityId,
                RepositoryUniqueId = _appConfig.RepositoryUniqueId,
                SourcePatientInfo = new()
                {
                    PatientId = new("test", "1.2.3.4")
                }
            };

            var updateResponse = _registryWrapper.AddDocumentReferenceDtosToDocumentRegistry([documentEntry]);
            registryWriteOk = updateResponse.IsSuccess;
            if (!updateResponse.IsSuccess)
            {
                _logger.LogError("Failed to update document registry content. Error: {updateResponseMessage}", updateResponse.Message);
            }

            var storeResponse = _repositoryWrapper.StoreDocument(documentEntry.UniqueId, [0x00], "test");
            repositoryWriteOk = storeResponse.IsSuccess;
            if (!storeResponse.IsSuccess)
            {
                _logger.LogError("Failed to store document in repository. Error: {storeResponseMessage}", storeResponse.Message);
            }

            var randomEntry = _registryWrapper.GetRegistryItemAndRelated(documentEntry.Id)?.OfType<DocumentEntryDto>().FirstOrDefault();
            var document = _repositoryWrapper.GetDocumentFromRepository(randomEntry?.HomeCommunityId, randomEntry?.RepositoryUniqueId, randomEntry?.UniqueId);

            registryReadOk = randomEntry != null;
            repositoryReadOk = document != null;

            _registryWrapper.DeleteRegistryObjectFromRegistry(documentEntry);
            _repositoryWrapper.DeleteSingleDocument(documentEntry.UniqueId);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while checking registry and repository status: {exception}", e.ToString());
        }

        return new SourceStatus(registryReadOk, registryWriteOk, repositoryReadOk, repositoryWriteOk);
    }
}

public class SourceStatus
{
    public SourceStatus(bool registryReadOk, bool registryWriteOk, bool repositoryReadOk, bool repositoryWriteOk)
    {
        RegistryReadOk = registryReadOk;
        RegistryWriteOk = registryWriteOk;
        RepositoryReadOk = repositoryReadOk;
        RepositoryWriteOk = repositoryWriteOk;
    }

    public bool RegistryReadOk { get; set; }
    public bool RegistryWriteOk { get; set; }
    public bool RepositoryReadOk { get; set; }
    public bool RepositoryWriteOk { get; set; }
}