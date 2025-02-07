using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Source;

public class RepositoryWrapper
{
    internal string _repositoryPath;

    public RepositoryWrapper()
    {
        string baseDirectory = AppContext.BaseDirectory;
        _repositoryPath = Path.Combine(baseDirectory, "..", "..", "..", "..", "XdsDocumentSource", "Repository");
    }
    public byte[]? GetDocumentFromRepository(string homeCommunityId, string repositoryUniqueId, string documentUniqueId)
    {
        string baseDirectory = AppContext.BaseDirectory;
        if (!Directory.Exists(_repositoryPath))
        {
            return null;
        }
        var directories = Directory.GetDirectories(_repositoryPath);
        foreach (var directory in directories)
        {
            var files = Directory.GetFiles(directory);
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                if (name.Replace("^", "") != documentUniqueId.Replace("^", ""))
                {
                    continue;
                }
                //var content = File.ReadAllBytes(file);
                var content = File.ReadAllBytes(file);
                return content;
            }
        }
        return null;
    }

    public bool StoreDocument(DocumentType assocDocument, string patientIdPart)
    {
        var documentPath = Path.Combine(_repositoryPath, patientIdPart);

        if (!Directory.Exists(documentPath))
        {
            Directory.CreateDirectory(documentPath);
        }

        // Create a file in the Repository folder
        string filePath = Path.Combine(_repositoryPath, patientIdPart, assocDocument.Id.NoUrn());
        File.WriteAllBytes(filePath, assocDocument.Value);
        return true;

    }

}
