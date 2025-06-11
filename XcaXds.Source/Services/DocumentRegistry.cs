using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.DocumentEntry;

namespace XcaXds.Source.Services;

public class DocumentRegistry : IDocumentRegistry
{
    internal string _registryPath;
    internal string _registryFile;
    private readonly object _lock = new();

    public DocumentRegistry()
    {
        string baseDirectory = AppContext.BaseDirectory;
        _registryPath = Path.Combine(baseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry");
        _registryFile = Path.Combine(_registryPath, "Registry.json");
        EnsureRegistryFileExists();
    }

    public List<RegistryObjectDto> ReadRegistry()
    {
        lock (_lock)
        {
            var json = File.ReadAllText(_registryFile);
            return RegistryJsonSerializer.Deserialize<List<RegistryObjectDto>>(json);
        }
    }

    public bool WriteRegistry(List<RegistryObjectDto> dtos)
    {
        lock (_lock)
        {
            File.WriteAllTextAsync(_registryFile, RegistryJsonSerializer.Serialize(dtos));
            return true;
        }
    }

    private void EnsureRegistryFileExists()
    {
        lock (_lock)
        {
            if (!Directory.Exists(_registryPath))
            {
                Directory.CreateDirectory(_registryPath);
            }

            if (!File.Exists(_registryFile))
            {

                using (File.Create(_registryFile)) { }

                WriteRegistry(new List<RegistryObjectDto>());
            }
        }
    }

}