using XcaXds.Commons;
using XcaXds.Commons.Models.Custom.DocumentEntry;
using XcaXds.Commons.Models.Soap.Custom;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;

namespace XcaXds.Source;


public class RegistryWrapper
{
    internal volatile List<RegistryObjectDto> _jsonDocumentRegistry = null;
    private readonly RegistryMetadataTransformerService _registryMetadataTransformerService;

    private readonly IDocumentRegistry _documentRegistry;

    public RegistryWrapper(RegistryMetadataTransformerService registryMetadataTransformerService, IDocumentRegistry documentRegistry)
    {
        _registryMetadataTransformerService = registryMetadataTransformerService;
        _documentRegistry = documentRegistry;
    }

    public List<RegistryObjectDto> GetDocumentRegistryContentAsDtos()
    {
        if (_jsonDocumentRegistry != null)
            return _jsonDocumentRegistry;

        if (_jsonDocumentRegistry == null)
        {
            try
            {
                _jsonDocumentRegistry = _documentRegistry.ReadRegistry();
            }
            catch (Exception ex)
            {
                _jsonDocumentRegistry = new();
            }
        }
        return _jsonDocumentRegistry;
    }

    public XmlDocumentRegistry GetDocumentRegistryContentAsRegistryObjects()
    {
        var dtoList = GetDocumentRegistryContentAsDtos();
        var registryObjs = _registryMetadataTransformerService
                .TransformRegistryObjectDtosToRegistryObjects(dtoList);

        return new XmlDocumentRegistry { RegistryObjectList = registryObjs };
    }

    public bool SetDocumentRegistryContentWithDtos(List<RegistryObjectDto> registryObjectDtos)
    {
        if (registryObjectDtos.Count == 0) return false;

        _documentRegistry.WriteRegistry(registryObjectDtos);
        _jsonDocumentRegistry = registryObjectDtos;
        return true;
    }

    public bool UpdateDocumentRegistryContentWithDtos(List<RegistryObjectDto> registryObjectDtos)
    {
        if (registryObjectDtos.Count == 0) return false;
        _jsonDocumentRegistry.AddRange(registryObjectDtos);
        _documentRegistry.WriteRegistry(_jsonDocumentRegistry);
        return true;
    }

    public SoapRequestResult<string> UpdateDocumentRegistryFromXml(XmlDocumentRegistry xml)
    {
        try
        {
            var dtoList = _registryMetadataTransformerService
                .TransformRegistryObjectsToRegistryObjectDtos(xml.RegistryObjectList);

            SetDocumentRegistryContentWithDtos(dtoList);

            return new SoapRequestResult<string>().Success("Updated OK");
        }
        catch (Exception ex)
        {
            return new SoapRequestResult<string>().Fault($"Error updating registry: {ex.Message}");
        }
    }
}
