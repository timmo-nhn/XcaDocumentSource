using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Source.Source;

public class FileBasedRegistry : IRegistry
{
    internal string _registryPath;
    internal string _registryFile;
    private readonly object _lock = new();
    private readonly ILogger<FileBasedRegistry> _logger;

    public FileBasedRegistry()
    {
        // When running in a container the path will be different
        var customPath = Environment.GetEnvironmentVariable("REGISTRY_FILE_PATH");
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _registryPath = customPath;
        }
        else
        {
            string baseDirectory = AppContext.BaseDirectory;
            _registryPath = Path.Combine(baseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry");
        }

        _registryFile = Path.Combine(_registryPath, "Registry.json");
    }

    public FileBasedRegistry(ILogger<FileBasedRegistry> logger)
    {
        _logger = logger;

        // When running in a container the path will be different
        var customPath = Environment.GetEnvironmentVariable("REGISTRY_FILE_PATH");
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _registryPath = customPath;
        }
        else
        {
            string baseDirectory = AppContext.BaseDirectory;
            _registryPath = Path.Combine(baseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry");
        }

        _registryFile = Path.Combine(_registryPath, "Registry.json");
    }

    public IEnumerable<RegistryObjectDto> ReadRegistry()
    {
        EnsureRegistryFileExists();
        lock (_lock)
        {
            var json = File.ReadAllText(_registryFile);
            var registryContent = RegistryJsonSerializer.Deserialize<List<RegistryObjectDto>>(json);

            _logger?.LogInformation($"Read {registryContent?.Count ?? 0} entries from {_registryPath}");

            return registryContent ?? new List<RegistryObjectDto>();
        }
    }

    public bool WriteRegistry(List<RegistryObjectDto> dtos)
    {
        EnsureRegistryFileExists();
        lock (_lock)
        {
            File.WriteAllText(_registryFile, RegistryJsonSerializer.Serialize(dtos));
            return true;
        }
    }

    public bool UpdateRegistry(List<RegistryObjectDto> dtos)
    {
        var registry = ReadRegistry().ToList();
        registry.AddRange(dtos);
        return WriteRegistry(registry);
    }

    public bool DeleteRegistryItem(string id)
    {
        var registry = ReadRegistry();
        var itemToDelete = registry.FirstOrDefault(x => x.Id == id);
        if (itemToDelete == null) return false;

        var registryRemoved = registry.ToList();
        registryRemoved.Remove(itemToDelete);

        return WriteRegistry(registryRemoved);
    }

    public void MarkFileRegistryAsMigrated()
    {
        lock (_lock)
        {
            if (Directory.Exists(_registryPath))
            {
                using (File.Create(Path.Combine(_registryPath, ".migrated"))) { }
            }
        }
    }

    public bool IsFileRegistryAsMigrated()
    {
        lock (_lock)
        {
            return File.Exists(Path.Combine(_registryPath, ".migrated"));
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

    public bool RegistryExists()
    {
        lock (_lock)
        {
            var exists = File.Exists(Path.Combine(_registryFile));

            if (!exists) return false;

            var hasContent = ReadRegistry().Count() != 0;

            return hasContent;
        }
    }
}