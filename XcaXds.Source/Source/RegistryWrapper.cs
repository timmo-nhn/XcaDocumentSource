using XcaXds.Commons;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.Custom;
using XcaXds.Commons.Services;

namespace XcaXds.Source.Source;


public class RegistryWrapper
{
    internal volatile List<RegistryObjectDto> _jsonDocumentRegistry = null;

    private readonly IRegistry _documentRegistry;

    public RegistryWrapper(IRegistry documentRegistry)
    {
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
        var registryObjs = RegistryMetadataTransformerService
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

    public SoapRequestResult<string> SetDocumentRegistryFromXml(XmlDocumentRegistry xml)
    {
        try
        {
            var dtoList = RegistryMetadataTransformerService
                .TransformRegistryObjectsToRegistryObjectDtos(xml.RegistryObjectList);

            SetDocumentRegistryContentWithDtos(dtoList);

            return new SoapRequestResult<string>().Success("Updated OK");
        }
        catch (Exception ex)
        {
            return new SoapRequestResult<string>().Fault($"Error updating registry: {ex.Message}");
        }
    }

    public SoapRequestResult<string> UpdateDocumentRegistryFromXml(XmlDocumentRegistry xml)
    {
        try
        {
            var dtoList = RegistryMetadataTransformerService
                .TransformRegistryObjectsToRegistryObjectDtos(xml.RegistryObjectList);

            UpdateDocumentRegistryContentWithDtos(dtoList);

            return new SoapRequestResult<string>().Success("Updated OK");
        }
        catch (Exception ex)
        {
            return new SoapRequestResult<string>().Fault($"Error updating registry: {ex.Message}");
        }
    }
}
