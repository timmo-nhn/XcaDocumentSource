using System.Text.RegularExpressions;
using XcaXds.Commons.Extensions;

namespace XcaXds.Source;

public partial class RepositoryWrapper
{
    private readonly ApplicationConfig _appConfig;
    private readonly string _repositoryPath;

    private static readonly Regex SafeFileNameRegex = new(@"^[a-zA-Z0-9\-_\.^]+$", RegexOptions.Compiled);
    private static readonly Regex SafeCharacters = new(@"[^a-zA-Z0-9\-_\.^]+", RegexOptions.Compiled);

    public RepositoryWrapper(ApplicationConfig appConfig)
    {
        _appConfig = appConfig;
        _repositoryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Repository", _appConfig.RepositoryUniqueId);
    }

    public byte[]? GetDocumentFromRepository(string homeCommunityId, string repositoryUniqueId, string documentUniqueId)
    {
        if (!IsValidIdentifier(documentUniqueId)) return null;

        if (!Directory.Exists(_repositoryPath)) return null;

        foreach (var directory in Directory.GetDirectories(_repositoryPath))
        {
            foreach (var file in Directory.GetFiles(directory))
            {

                var name = Path.GetFileName(file);

                if (name.Replace("^", "") == documentUniqueId.Replace("^", ""))
                {
                    return File.ReadAllBytes(file);
                }
            }
        }
        return null;
    }

    public bool StoreDocument(string documentId, byte[] documentContent, string patientIdPart)
    {
        documentId = SafeCharacters.Replace(documentId, "");
        patientIdPart = SafeCharacters.Replace(patientIdPart, "");

        if (!IsValidIdentifier(documentId) || !IsValidIdentifier(patientIdPart)) return false;

        var documentPath = Path.Combine(_repositoryPath, patientIdPart);

        if (!Directory.Exists(documentPath))
        {
            Directory.CreateDirectory(documentPath);
        }

        string filePath = Path.Combine(documentPath, documentId.NoUrn());

        File.WriteAllBytes(filePath, documentContent);
        return true;
    }

    public bool CheckIfFileExistsInRepository(string documentUniqueId)
    {
        if (!Directory.Exists(_repositoryPath)) return false;

        foreach (var directory in Directory.GetDirectories(_repositoryPath))
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                var name = Path.GetFileName(file);
                if (name == documentUniqueId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool DeleteSingleDocument(string documentUniqueId)
    {
        documentUniqueId = SafeCharacters.Replace(documentUniqueId, "");
        if (!Directory.Exists(_repositoryPath)) return false;

        foreach (var directory in Directory.GetDirectories(_repositoryPath))
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                var name = Path.GetFileName(file);
                if (name == documentUniqueId)
                {
                    File.Delete(file);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Ensures that file and directory names are safe by allowing only alphanumeric characters, dashes, and underscores.
    /// </summary>
    private static bool IsValidIdentifier(string input)
    {
        return !string.IsNullOrEmpty(input) && SafeFileNameRegex.IsMatch(input);
    }
}
