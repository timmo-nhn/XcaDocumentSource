using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;

namespace XcaXds.Source.Source;

public class FileBasedRepository : IRepository
{
    private readonly ApplicationConfig _appConfig;
    private readonly string _repositoryPath;
    private readonly object _lock = new();
    public FileBasedRepository(ApplicationConfig appConfig)
    {
        _appConfig = appConfig;
        _repositoryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Repository", _appConfig.RepositoryUniqueId);
    }

    public byte[]? Read(string documentUniqueId)
    {
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
}
