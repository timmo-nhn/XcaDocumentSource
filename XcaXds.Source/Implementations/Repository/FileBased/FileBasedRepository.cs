using System.Text.RegularExpressions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Shared.Extensions;

namespace XcaXds.Source.Implementations.Repository.FileBased;

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

        // When running in a container the path will be different
        var customPath = Environment.GetEnvironmentVariable("REPOSITORY_FILE_PATH");

        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _repositoryPath = Path.Combine(customPath, _appConfig.RepositoryUniqueId);
        }
        else
        {
            string baseDirectory = AppContext.BaseDirectory;
            _repositoryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Repository", _appConfig.RepositoryUniqueId);
        }
    }

    public byte[]? Read(string documentUniqueId)
    {
        if (!IsValidIdentifier(documentUniqueId, out _)) return null;

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

    public OperationResponse Write(string documentId, byte[] documentContent, string? patientIdPart)
    {
        documentId = SafeCharacters.Replace(documentId, "");
        patientIdPart = SafeCharacters.Replace(patientIdPart ?? "", "");

        if (!IsValidIdentifier(documentId, out var invalidCharacters))
            return OperationResponse.Failure($"Invalid Document ID {documentId}, Invalid characters {invalidCharacters}");

        if (!IsValidIdentifier(patientIdPart, out var invalidPatientIdCharacters))
            return OperationResponse.Failure($"Invalid Patient ID {patientIdPart}, Invalid characters {invalidPatientIdCharacters}");

        lock (_lock)
        {
            var documentPath = Path.Combine(_repositoryPath, patientIdPart);

            if (!Directory.Exists(documentPath))
            {
                Directory.CreateDirectory(documentPath);
            }

            string filePath = Path.Combine(documentPath, documentId.NoUrn());
            File.WriteAllBytes(filePath, documentContent);
            return OperationResponse.Success($"Document written to {filePath}");
        }
    }

    public OperationResponse Delete(string? documentUniqueId)
    {
        if (string.IsNullOrWhiteSpace(documentUniqueId)) return OperationResponse.Failure("No Document ID provided");

        documentUniqueId = SafeCharacters.Replace(documentUniqueId, "");

        lock (_lock)
        {
            if (!Directory.Exists(_repositoryPath)) return OperationResponse.Failure("Repository path does not exist");

            var repositoryDirectories = Directory.GetDirectories(_repositoryPath).SelectMany(f => Directory.GetFiles(f)).ToArray();

            var documentToDelete = repositoryDirectories.FirstOrDefault(file => Path.GetFileName(file) == documentUniqueId);

            if (string.IsNullOrWhiteSpace(documentToDelete))
            {
                return OperationResponse.Failure("Document not found");
            }

            try
            {
                File.Delete(documentToDelete);
                return OperationResponse.Success($"Document {documentUniqueId} deleted successfully");
            }
            catch (UnauthorizedAccessException ex)
            {
                return OperationResponse.Failure($"Access denied when deleting document '{documentUniqueId}': {ex.Message}");
            }
            catch (IOException ex)
            {
                return OperationResponse.Failure($"Document '{documentUniqueId}' could not be deleted, it may be in use: {ex.Message}");
            }
        }
    }


    /// <summary>
    /// Ensures that file and directory names are safe by allowing only alphanumeric characters, dashes, and underscores.
    /// </summary>
    private static bool IsValidIdentifier(string input, out string invalidCharacters)
    {
        invalidCharacters = string.Empty;
        if (string.IsNullOrEmpty(input)) return false;

        var matches = SafeFileNameRegex.Matches(input);
        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                invalidCharacters += match.Value;
            }
        }

        return string.IsNullOrEmpty(invalidCharacters);
    }

    public bool SetNewOid(string repositoryOid, out string? oldId)
    {
        var parentDir = Directory.GetParent(_repositoryPath)?.FullName;
        if (!Directory.Exists(parentDir))
        {
            oldId = null;
            return false;
        }

        var currentId = Path.GetFileName(Directory.GetFileSystemEntries(parentDir).FirstOrDefault());
        oldId = currentId;


        if (currentId == null) return false;
        if (parentDir == null) return false;

        var newDir = Path.Combine(parentDir, repositoryOid);
        var currentDir = Path.Combine(parentDir, currentId);

        if (newDir == currentDir) return false;

        Directory.Move(currentDir, Path.Combine(parentDir, repositoryOid));
        return true;
    }
}
