using System.Text.RegularExpressions;
using XcaXds.Commons.Interfaces;

namespace XcaXds.Source.Source;

public partial class RepositoryWrapper
{
    private readonly ApplicationConfig _appConfig;


    private readonly IRepository _repository;

    public RepositoryWrapper(ApplicationConfig appConfig, IRepository repository)
    {
        _repository = repository;
        _appConfig = appConfig;
    }

    public byte[]? GetDocumentFromRepository(string homeCommunityId, string repositoryUniqueId, string documentUniqueId)
    {
        if (_appConfig.HomeCommunityId != homeCommunityId) return null;
        if (repositoryUniqueId.Substring(repositoryUniqueId.LastIndexOf('/') + 1) != _appConfig.RepositoryUniqueId) return null;

        return _repository.Read(documentUniqueId);
    }

    public bool StoreDocument(string documentId, byte[] documentContent, string patientIdPart)
    {
        return _repository.Write(documentId, documentContent, patientIdPart);
    }

    public bool DeleteSingleDocument(string documentUniqueId)
    {
        return _repository.Delete(documentUniqueId);
    }

    public bool CheckIfFileExistsInRepository(string documentUniqueId)
    {
        return _repository.Read(documentUniqueId) != null;
    }

}
