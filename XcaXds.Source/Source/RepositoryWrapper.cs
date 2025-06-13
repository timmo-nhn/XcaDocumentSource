using System.Text.RegularExpressions;
using XcaXds.Commons.Interfaces;

namespace XcaXds.Source.Source;

public partial class RepositoryWrapper
{
    private readonly ApplicationConfig _appConfig;

    private static readonly Regex SafeFileNameRegex = new(@"^[a-zA-Z0-9\-_\.^]+$", RegexOptions.Compiled);
    private static readonly Regex SafeCharacters = new(@"[^a-zA-Z0-9\-_\.^]+", RegexOptions.Compiled);

    private readonly IRepository _repository;

    public RepositoryWrapper(ApplicationConfig appConfig, IRepository repository)
    {
        _repository = repository;
        _appConfig = appConfig;
    }

    public byte[]? GetDocumentFromRepository(string homeCommunityId, string repositoryUniqueId, string documentUniqueId)
    {
        if (!IsValidIdentifier(documentUniqueId)) return null;

        if (_appConfig.HomeCommunityId != homeCommunityId) return null;

        if (repositoryUniqueId.Substring(repositoryUniqueId.LastIndexOf('/') + 1) != _appConfig.RepositoryUniqueId) return null;
        return _repository.Read(documentUniqueId);
    }

    public bool StoreDocument(string documentId, byte[] documentContent, string patientIdPart)
    {
        documentId = SafeCharacters.Replace(documentId, "");
        patientIdPart = SafeCharacters.Replace(patientIdPart, "");

        if (!IsValidIdentifier(documentId) || !IsValidIdentifier(patientIdPart)) return false;

        return _repository.Write(documentId, documentContent, patientIdPart);
    }

    public bool DeleteSingleDocument(string documentUniqueId)
    {
        documentUniqueId = SafeCharacters.Replace(documentUniqueId, "");

        return _repository.Delete(documentUniqueId);
    }

    public bool CheckIfFileExistsInRepository(string documentUniqueId)
    {
        return _repository.Read(documentUniqueId) != null;
    }

    /// <summary>
    /// Ensures that file and directory names are safe by allowing only alphanumeric characters, dashes, and underscores.
    /// </summary>
    private static bool IsValidIdentifier(string input)
    {
        return !string.IsNullOrEmpty(input) && SafeFileNameRegex.IsMatch(input);
    }
}
