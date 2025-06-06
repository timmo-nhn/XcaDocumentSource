using System.Xml;
using System.Xml.Serialization;
using XcaXds.Commons;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.DocumentEntry;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;

namespace XcaXds.Source;

[XmlRoot("Registry")]
public class XmlDocumentRegistry
{
    [XmlElement("RegistryPackage", typeof(RegistryPackageType), Namespace = Constants.Soap.Namespaces.Rim)]
    [XmlElement("ExtrinsicObject", typeof(ExtrinsicObjectType), Namespace = Constants.Soap.Namespaces.Rim)]
    [XmlElement("Association", typeof(AssociationType), Namespace = Constants.Soap.Namespaces.Rim)]
    public List<IdentifiableType> RegistryObjectList { get; set; }

    private readonly object _lock = new();

    public XmlDocumentRegistry()
    {
        RegistryObjectList = [];
    }

}

public class RegistryWrapper
{
    internal volatile List<RegistryObjectDto> _jsonDocumentRegistry = null;
    internal string _registryPath;
    internal string _registryFile;
    private readonly object _lock = new();
    private readonly RegistryMetadataTransformerService _registryMetadataTransformerService;

    public RegistryWrapper(RegistryMetadataTransformerService registryMetadataTransformerService)
    {
        string baseDirectory = AppContext.BaseDirectory;
        _registryPath = Path.Combine(baseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry");
        _registryFile = Path.Combine(_registryPath, "Registry.json");

        _registryMetadataTransformerService = registryMetadataTransformerService;

        EnsureRegistryFileExists();
    }

    public List<RegistryObjectDto> GetDocumentRegistryContentAsDtos()
    {
        if (_jsonDocumentRegistry != null)
            return _jsonDocumentRegistry;

        lock (_lock)
        {
            if (_jsonDocumentRegistry == null)
            {
                try
                {
                    var json = File.ReadAllText(_registryFile);
                    _jsonDocumentRegistry =
                        RegistryJsonSerializer.Deserialize<List<RegistryObjectDto>>(json) ?? new();
                }
                catch (Exception ex)
                {
                    _jsonDocumentRegistry = new();
                }
            }
            return _jsonDocumentRegistry;
        }
    }

    public XmlDocumentRegistry GetDocumentRegistryContentAsRegistryObjects()
    {
        var dtoList = GetDocumentRegistryContentAsDtos();
        var registryObjs = _registryMetadataTransformerService
                .TransformRegistryObjectDtosToRegistryObjects(dtoList);

        return new XmlDocumentRegistry { RegistryObjectList = registryObjs };
    }

    public bool UpdateDocumentRegistryWithDtos(List<RegistryObjectDto> registryObjectDtos)
    {
        lock (_lock)
        {
            if (registryObjectDtos.Count == 0) return false;

            File.WriteAllText(_registryFile, RegistryJsonSerializer.Serialize(registryObjectDtos));
            _jsonDocumentRegistry = registryObjectDtos;
            return true;
        }
    }

    public SoapRequestResult<string> UpdateDocumentRegistryFromXml(XmlDocumentRegistry xml)
    {
        try
        {
            var dtoList = _registryMetadataTransformerService
                .TransformRegistryObjectsToRegistryObjectDtos(xml.RegistryObjectList);

            UpdateDocumentRegistryWithDtos(dtoList);

            return new SoapRequestResult<string>().Success("Updated OK");
        }
        catch (Exception ex)
        {
            return new SoapRequestResult<string>().Fault($"Error updating registry: {ex.Message}");
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

                UpdateDocumentRegistryFromXml(new XmlDocumentRegistry());
            }
        }
    }
}
