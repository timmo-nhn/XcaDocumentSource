using Microsoft.Extensions.Diagnostics.HealthChecks;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.WebService.Services;

public class SourceHealthCheckService
{
    private readonly ILogger<SourceHealthCheckService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly HealthCheckService _healthCheckService;
    private readonly RegistryWrapper _registryWrapper;
    private readonly RepositoryWrapper _repositoryWrapper;

    public SourceHealthCheckService(ILogger<SourceHealthCheckService> logger, ApplicationConfig appConfig, HealthCheckService healthCheckService, RegistryWrapper registryWrapper, RepositoryWrapper repositoryWrapper)
    {
        _logger = logger;
        _healthCheckService = healthCheckService;
        _registryWrapper = registryWrapper;
        _repositoryWrapper = repositoryWrapper;
        _appConfig = appConfig;
    }

    public async Task<HealthReport> CheckHealthAsync()
    {
        return await _healthCheckService.CheckHealthAsync();
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
                    PatientId = new("test","1.2.3.4")
                }
            };
            
            _registryWrapper.UpdateDocumentRegistryContentWithDtos([documentEntry]);
            _repositoryWrapper.StoreDocument(documentEntry.UniqueId,[0x00],"test");
            
            var randomEntry = _registryWrapper.GetRegistryItemAndRelated(documentEntry.Id)?.OfType<DocumentEntryDto>().FirstOrDefault();
            var document = _repositoryWrapper.GetDocumentFromRepository(randomEntry?.HomeCommunityId, randomEntry?.RepositoryUniqueId, randomEntry?.UniqueId);
            
            registryOk = randomEntry != null;
            repositoryOk = document != null;

            _registryWrapper.DeleteDocumentEntryFromRegistry(documentEntry);
            _repositoryWrapper.DeleteSingleDocument(documentEntry.UniqueId);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
        
        return new SourceStatus()
        {
            RepositoryOk = repositoryOk,
            RegistryOk = registryOk,
        };
    }
}

public class SourceStatus
{
    public bool RegistryOk { get; set; }
    public bool RepositoryOk { get; set; }
}