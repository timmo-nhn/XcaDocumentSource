using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.WebService.Services.XdsRegistry;
using XcaXds.WebService.Services.XdsRepository;

namespace XcaXds.WebService.Services;

public class ApplicationMetaService
{
    private readonly ILogger<ApplicationMetaService> _logger;
    private readonly RepositoryWrapper _repositoryWrapper;
    private readonly RegistryWrapper _registryWrapper;

    public ApplicationMetaService(ILogger<ApplicationMetaService> logger, RepositoryWrapper repositoryWrapper, RegistryWrapper registryWrapper)
    {
        _logger = logger;
        _repositoryWrapper = repositoryWrapper;
        _registryWrapper = registryWrapper;
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
}
