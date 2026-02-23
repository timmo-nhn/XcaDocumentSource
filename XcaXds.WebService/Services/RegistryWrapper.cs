using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.Custom;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.DataManipulators;

namespace XcaXds.WebService.Services;


public class RegistryWrapper
{
    internal volatile IEnumerable<RegistryObjectDto> _registryObjectList = null;

    private readonly IRegistry _documentRegistry;

    public RegistryWrapper(IRegistry documentRegistry)
    {
        _documentRegistry = documentRegistry;
    }

    public IEnumerable<RegistryObjectDto> GetDocumentRegistryContentAsDtos()
    {
        if (_registryObjectList != null)
            return _registryObjectList;

        if (_registryObjectList == null)
        {
            try
            {
                _registryObjectList = _documentRegistry.ReadRegistry();
            }
            catch (Exception ex)
            {
                _registryObjectList = Enumerable.Empty<RegistryObjectDto>();
            }
        }
        return _registryObjectList;
    }

    public XmlDocumentRegistry GetDocumentRegistryContentAsRegistryObjects()
    {
        var dtoList = GetDocumentRegistryContentAsDtos();
        var registryObjs = RegistryMetadataTransformerService
                .TransformRegistryObjectDtosToRegistryObjects(dtoList);

        return new XmlDocumentRegistry { RegistryObjectList = registryObjs };
    }

    public bool SetDocumentRegistryContentWithDtos(List<RegistryObjectDto>? registryObjectDtos)
    {
        if (registryObjectDtos == null) return false;

        _documentRegistry.WriteRegistry(registryObjectDtos);
        _registryObjectList = _documentRegistry.ReadRegistry();
        return true;
    }

    public bool UpdateDocumentRegistryContentWithDtos(RegistryObjectDto registryObjectDto)
    {
        return UpdateDocumentRegistryContentWithDtos(new List<RegistryObjectDto>() { registryObjectDto });
    }

    public bool DeleteDocumentEntryFromRegistry(RegistryObjectDto registryObjectDto)
    {
        if (registryObjectDto == null) return false;

        var deleteResponse = _documentRegistry.DeleteRegistryItem(registryObjectDto.Id);
        _registryObjectList = _documentRegistry.ReadRegistry();


        return deleteResponse;
    }

    public bool UpdateDocumentRegistryContentWithDtos(List<RegistryObjectDto> registryObjectDtos)
    {
        if (registryObjectDtos.Count == 0) return false;
        _registryObjectList ??= GetDocumentRegistryContentAsDtos();

        _documentRegistry.UpdateRegistry(registryObjectDtos);
        _registryObjectList = _documentRegistry.ReadRegistry();

        return true;
    }

    public SoapRequestResult<string> SetDocumentRegistryFromRegistryObjects(IdentifiableType[] registryObjects)
    {
        try
        {
            var dtoList = RegistryMetadataTransformerService.TransformRegistryObjectsToRegistryObjectDtos(registryObjects);

            SetDocumentRegistryContentWithDtos(dtoList);

            return new SoapRequestResult<string>().Success("Updated OK");
        }
        catch (Exception ex)
        {
            return new SoapRequestResult<string>().Fault($"Error updating registry: {ex.Message}");
        }
    }

    public SoapRequestResult<string> UpdateDocumentRegistryFromRegistryObjects(IEnumerable<IdentifiableType> registryObjects)
    {
        try
        {
            var dtoList = RegistryMetadataTransformerService
                .TransformRegistryObjectsToRegistryObjectDtos(registryObjects);

            UpdateDocumentRegistryContentWithDtos(dtoList);

            return new SoapRequestResult<string>().Success("Updated OK");
        }
        catch (Exception ex)
        {
            return new SoapRequestResult<string>().Fault($"Error updating registry: {ex.Message}");
        }
    }
}
