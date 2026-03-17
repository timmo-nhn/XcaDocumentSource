
using Microsoft.OpenApi.MicrosoftExtensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RestfulRegistry;

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
        return DateTime.Now.ToString("ddMMyyhhMM");
    }

    public RestfulApiResponse NukeRegistryRepository(string nukeKey)
    {
        var apiResponse = new RestfulApiResponse();

        var datetime = DateTime.Now.ToString("ddMMyyhhMM");
        if (datetime != nukeKey)
        {
            apiResponse.AddError("InvalidKey", "Invalid Nuke key, get nuke key from the 'get-nuke-key'-endpoint");
            return apiResponse;
        }

        var documentIds = _registryWrapper.GetDocumentRegistryContentAsDtos().OfType<DocumentEntryDto>().Select(dent => dent.UniqueId).ToList();

        var amount = documentIds.Count;
        _logger.LogInformation($"Fetched {amount} for nuking");

        if (amount == 0)
        {
            apiResponse.SetMessage("Nothing to nuke");
            return apiResponse;
        }

        documentIds.ForEach(docid => _repositoryWrapper.DeleteSingleDocument(docid));
        _registryWrapper.SetDocumentRegistryContentWithDtos(new List<RegistryObjectDto>());

        apiResponse.SetMessage($"Nuked {amount} documents from registry and repository");
        return apiResponse;
    }
}
