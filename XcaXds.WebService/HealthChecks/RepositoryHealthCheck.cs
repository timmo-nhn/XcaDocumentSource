using Microsoft.Extensions.Diagnostics.HealthChecks;
using XcaXds.WebService.Services.XdsRepository;

namespace XcaXds.WebService.HealthChecks;

public class RepositoryHealthCheck : IHealthCheck
{
    private readonly ILogger<RepositoryHealthCheck> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly RepositoryWrapper _repositoryWrapper;

    public RepositoryHealthCheck(
        ILogger<RepositoryHealthCheck> logger,
        ApplicationConfig appConfig,
        RepositoryWrapper repositoryWrapper)
    {
        _logger = logger;
        _appConfig = appConfig;
        _repositoryWrapper = repositoryWrapper;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var writeOk = false;
        var readOk = false;

        var documentUniqueId = Guid.NewGuid().ToString();

        try
        {
            var storeResponse = _repositoryWrapper.StoreDocument(documentUniqueId, [0x00], "test");
            writeOk = storeResponse.IsSuccess;

            if (!writeOk)
            {
                _logger.LogError("Failed to store document in repository. Error: {storeResponseMessage}", storeResponse.Message);
            }

            var document = _repositoryWrapper.GetDocumentFromRepository(
                _appConfig.HomeCommunityId,
                _appConfig.RepositoryUniqueId,
                documentUniqueId);

            readOk = document != null;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while checking repository status: {exception}", e.ToString());

            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Repository health check threw an exception",
                e,
                BuildData(readOk, writeOk)));
        }
        finally
        {
            TryCleanup(documentUniqueId);
        }

        var data = BuildData(readOk, writeOk);

        return Task.FromResult(readOk && writeOk
            ? HealthCheckResult.Healthy("Repository is readable and writable", data)
            : new HealthCheckResult(context.Registration.FailureStatus, "Repository read or write failed", data: data));
    }

    private void TryCleanup(string documentUniqueId)
    {
        try
        {
            _repositoryWrapper.DeleteSingleDocument(documentUniqueId);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while cleaning up repository health check document: {exception}", e.ToString());
        }
    }

    private static IReadOnlyDictionary<string, object> BuildData(bool readOk, bool writeOk)
    {
        return new Dictionary<string, object>
        {
            ["RepositoryReadOk"] = readOk,
            ["RepositoryWriteOk"] = writeOk
        };
    }
}
