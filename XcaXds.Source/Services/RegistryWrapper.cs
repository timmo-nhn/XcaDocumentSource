using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.DocumentEntryDto;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;

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

    public DocumentRegistry GetDocumentRegistryContent()
    {
        EnsureRegistryFileExists();

        lock (_lock)
        {
            try
            {
                using (var reader = new StreamReader(_registryFile))
                {
                    var documentEntry = JsonSerializer.Deserialize<DocumentReferenceDto[]>(reader.ReadToEnd());

                    if (documentEntry == null) throw new Exception();

                    var registryObjects = new List<IdentifiableType>();

                    foreach (var item in documentEntry)
                    {
                        registryObjects.AddRange(_registryMetadataTransformerService.TransformDocumentEntryDtoToRegistryObjects(item));
                    }
                    registryObjects = registryObjects.Where(ro => ro != null).ToList();
                    return new DocumentRegistry() { RegistryObjectList = registryObjects };
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
                var associations = registryContent.RegistryObjectList.OfType<AssociationType>()
                    .Where(assoc => assoc.AssociationTypeData == Constants.Xds.AssociationType.HasMember).ToArray();
                var extrinsicObjects = registryContent.RegistryObjectList.OfType<ExtrinsicObjectType>().ToArray();
                var registryPackages = registryContent.RegistryObjectList.OfType<RegistryPackageType>().ToArray();

                var documentDtoEntries = new List<RegistryObjectDto>();

                foreach (var association in associations)
                {
                    var extrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());
                    var registryPackage = registryPackages.FirstOrDefault(rp => rp.Id.NoUrn() == association.SourceObject.NoUrn());

                    var documentEntryDto = _registryMetadataTransformerService.TransformRegistryObjectsToDocumentEntryDto(extrinsicObject, registryPackage, association, null);
                    documentDtoEntries.Add(documentEntryDto);
                }

                using (var reader = new StreamWriter(_registryFile))
                {
                    reader.Write(JsonSerializer.Serialize(documentDtoEntries, new JsonSerializerOptions() { WriteIndented = true }));
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
