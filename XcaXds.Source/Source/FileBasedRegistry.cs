﻿using XcaXds.Commons;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Source.Source;

public class FileBasedRegistry : IRegistry
{
    internal string _registryPath;
    internal string _registryFile;
    private readonly object _lock = new();

    public FileBasedRegistry()
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
            var backup = File.ReadAllText(_registryFile);

            File.WriteAllText(_registryFile + $".backup_{DateTime.UtcNow.ToString(Constants.Hl7.Dtm.DtmYyFormat)}", backup);

            File.WriteAllText(_registryFile, RegistryJsonSerializer.Serialize(dtos));
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