using System.Xml;
using System.Xml.Serialization;
using XcaXds.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Source;

[XmlRoot("Registry")]
public class DocumentRegistry
{
    [XmlElement("RegistryPackage", typeof(RegistryPackageType), Namespace = Constants.Soap.Namespaces.Rim)]
    [XmlElement("ExtrinsicObject", typeof(ExtrinsicObjectType), Namespace = Constants.Soap.Namespaces.Rim)]
    [XmlElement("Association", typeof(AssociationType), Namespace = Constants.Soap.Namespaces.Rim)]
    public List<IdentifiableType> RegistryObjectList { get; set; }

    private readonly object _lock = new();

    public DocumentRegistry()
    {
        RegistryObjectList = [];
    }

}

public class RegistryWrapper
{
    internal string _registryPath;
    internal string _registryFile;

    private readonly object _lock = new(); // Locking object for thread-safety

    public RegistryWrapper()
    {
        string baseDirectory = AppContext.BaseDirectory;
        _registryPath = Path.Combine(baseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry");
        _registryFile = Path.Combine(_registryPath, "Registry.xml");

        EnsureRegistryFileExists();
    }

    public async Task<DocumentRegistry> GetDocumentRegistryContentAsync()
    {
        EnsureRegistryFileExists();

        lock (_lock)
        {
            try
            {
                using (var reader = new StreamReader(_registryFile))
                {
                    var serializer = new XmlSerializer(typeof(DocumentRegistry));

                    return (DocumentRegistry)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {

                return new DocumentRegistry();
            }
        }
    }

    public DocumentRegistry GetDocumentRegistryContent()
    {
        EnsureRegistryFileExists();

        lock (_lock)
        {
            try
            {
                using (var reader = new StreamReader(_registryFile))
                {
                    var serializer = new XmlSerializer(typeof(DocumentRegistry));

                    return (DocumentRegistry)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {

                return new DocumentRegistry();
            }
        }
    }

    public SoapRequestResult<string> UpdateDocumentRegistry(DocumentRegistry registryContent)
    {
        EnsureRegistryFileExists();

        lock (_lock)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(DocumentRegistry));
                using (var writer = new StreamWriter(_registryFile))
                {
                    serializer.Serialize(writer, registryContent);
                }

                return new SoapRequestResult<string>().Success("Updated OK");
            }
            catch (Exception ex)
            {
                return new SoapRequestResult<string>().Fault($"Error updating registry: {ex.Message}");
            }
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

                UpdateDocumentRegistry(new DocumentRegistry());
            }
        }
    }
}
