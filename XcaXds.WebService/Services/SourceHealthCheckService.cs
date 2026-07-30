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
        var registryOk = false;
        var repositoryOk = false;

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
            if (!updateResponse.IsSuccess)
            {
                _logger.LogError($"Failed to update document registry content. Error: {updateResponse.Message}");
            }

            var storeResponse = _repositoryWrapper.StoreDocument(documentEntry.UniqueId, [0x00], "test");
            if (!storeResponse.IsSuccess)
            {
                _logger.LogError($"Failed to store document in repository. Error: {storeResponse.Message}");
            }

            if (!storeResponse.IsSuccess || !updateResponse.IsSuccess)
            {
                return new SourceStatus(storeResponse.IsSuccess, updateResponse.IsSuccess);
            }

            var randomEntry = _registryWrapper.GetRegistryItemAndRelated(documentEntry.Id)?.OfType<DocumentEntryDto>().FirstOrDefault();
            var document = _repositoryWrapper.GetDocumentFromRepository(randomEntry?.HomeCommunityId, randomEntry?.RepositoryUniqueId, randomEntry?.UniqueId);

            registryOk = randomEntry != null;
            repositoryOk = document != null;

            _registryWrapper.DeleteRegistryObjectFromRegistry(documentEntry);
            _repositoryWrapper.DeleteSingleDocument(documentEntry.UniqueId);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }

        return new SourceStatus(registryOk, repositoryOk);
    }
}

public class SourceStatus
{
    public SourceStatus(bool registryOk, bool repositoryOk)
    {
        RegistryOk = registryOk;
        RepositoryOk = repositoryOk;
    }

    public bool RegistryOk { get; set; }
    public bool RepositoryOk { get; set; }
}