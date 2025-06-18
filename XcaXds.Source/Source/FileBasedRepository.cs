using System.Text.RegularExpressions;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;

namespace XcaXds.Source.Source;

public class FileBasedRepository : IRepository
{
    private readonly ApplicationConfig _appConfig;
    private readonly string _repositoryPath;
    private readonly object _lock = new();

    private static readonly Regex SafeFileNameRegex = new(@"^[a-zA-Z0-9\-_\.^]+$", RegexOptions.Compiled);
    private static readonly Regex SafeCharacters = new(@"[^a-zA-Z0-9\-_\.^]+", RegexOptions.Compiled);


    public FileBasedRepository(ApplicationConfig appConfig)
    {
        _appConfig = appConfig;
        _repositoryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Repository", _appConfig.RepositoryUniqueId);
    }

    public byte[]? Read(string documentUniqueId)
    {
        if (!IsValidIdentifier(documentUniqueId)) return null;

        lock (_lock)
        {
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
    }

    public bool Write(string documentId, byte[] documentContent, string patientIdPart)
    {
        documentId = SafeCharacters.Replace(documentId, "");
        patientIdPart = SafeCharacters.Replace(patientIdPart, "");

        if (!IsValidIdentifier(documentId) || !IsValidIdentifier(patientIdPart)) return false;

        lock (_lock)
        {
            var documentPath = Path.Combine(_repositoryPath, patientIdPart);

            if (!Directory.Exists(documentPath))
            {
                Directory.CreateDirectory(documentPath);
            }

            string filePath = Path.Combine(documentPath, documentId.NoUrn());
            File.WriteAllBytes(filePath, documentContent);

            return true;
        }
    }

    public bool Delete(string documentUniqueId)
    {
        documentUniqueId = SafeCharacters.Replace(documentUniqueId, "");

        lock (_lock)
        {
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
    }


    /// <summary>
    /// Ensures that file and directory names are safe by allowing only alphanumeric characters, dashes, and underscores.
    /// </summary>
    private static bool IsValidIdentifier(string input)
    {
        return !string.IsNullOrEmpty(input) && SafeFileNameRegex.IsMatch(input);
    }

}
