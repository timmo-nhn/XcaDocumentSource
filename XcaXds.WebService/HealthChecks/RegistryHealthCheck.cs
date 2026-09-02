using Microsoft.Extensions.Diagnostics.HealthChecks;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.WebService.Services.XdsRegistry;

namespace XcaXds.WebService.HealthChecks;

public class RegistryHealthCheck : IHealthCheck
{
    private readonly ILogger<RegistryHealthCheck> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly RegistryWrapper _registryWrapper;

    public RegistryHealthCheck(
        ILogger<RegistryHealthCheck> logger,
        ApplicationConfig appConfig,
        RegistryWrapper registryWrapper)
    {
        _logger = logger;
        _appConfig = appConfig;
        _registryWrapper = registryWrapper;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var writeOk = false;
        var readOk = false;

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

        try
        {
            var updateResponse = _registryWrapper.AddDocumentReferenceDtosToDocumentRegistry([documentEntry]);
            writeOk = updateResponse.IsSuccess;

            if (!writeOk)
            {
                _logger.LogError("Failed to update document registry content. Error: {updateResponseMessage}", updateResponse.Message);
            }

            var storedEntry = _registryWrapper.GetRegistryItemAndRelated(documentEntry.Id)?
                .OfType<DocumentEntryDto>()
                .FirstOrDefault();

            readOk = storedEntry != null;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while checking registry status: {exception}", e.ToString());

            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Registry health check threw an exception",
                e,
                BuildData(readOk, writeOk)));
        }
        finally
        {
            TryCleanup(documentEntry);
        }

        var data = BuildData(readOk, writeOk);

        return Task.FromResult(readOk && writeOk
            ? HealthCheckResult.Healthy("Registry is readable and writable", data)
            : new HealthCheckResult(context.Registration.FailureStatus, "Registry read or write failed", data: data));
    }

    private void TryCleanup(DocumentEntryDto documentEntry)
    {
        try
        {
            _registryWrapper.DeleteRegistryObjectFromRegistry(documentEntry);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while cleaning up registry health check entry: {exception}", e.ToString());
        }
    }

    private static IReadOnlyDictionary<string, object> BuildData(bool readOk, bool writeOk)
    {
        return new Dictionary<string, object>
        {
            ["RegistryReadOk"] = readOk,
            ["RegistryWriteOk"] = writeOk
        };
    }
}
