using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.WebService.Services.XdsRegistry;

public class RegistryWrapper
{
    private readonly IRegistry _registry;
    private readonly ILogger<RegistryWrapper> _logger;

    internal volatile IEnumerable<RegistryObjectDto>? _registryObjectList = null;

    public RegistryWrapper(IRegistry registry, ILogger<RegistryWrapper> logger)
    {
        _logger = logger;
        _registry = registry;
    }


    public IEnumerable<IdentifiableType> GetDocumentRegistryContentAsRegistryObjects(PatientId? patientIdentifier = null)
    {
        var dtoList = GetDocumentRegistryContentAsDtos(patientIdentifier);
        foreach (var dto in dtoList)
        {
            var item = RegistryMetadataTransformerService.TransformRegistryObjectDtoToRegistryObjectStateless(dto);

            if (item == null) continue;

            yield return item;
        }
    }

    public RegistryObjectDto? GetSingleRegistryObjectAsDto(string? idOrUniqueId)
    {
        if (string.IsNullOrWhiteSpace(idOrUniqueId)) return null;

        return _registry.GetSingleRegistryItem(idOrUniqueId);
    }


    public IEnumerable<RegistryObjectDto>? GetRegistryItemAndRelated(string? idOrUniqueId)
    {
        if (string.IsNullOrWhiteSpace(idOrUniqueId)) return null;

        return _registry.GetRegistryItemsAndRelated(idOrUniqueId);
    }

    public IEnumerable<RegistryObjectDto>? GetDocumentRegistryContentAsDtosByPatientId(PatientId? patientIdentifier = null)
    {
        if (patientIdentifier == null || patientIdentifier.Id == null) return null;

        return _registry.GetRegistryItemsForPatient(patientIdentifier);
    }

    public IEnumerable<RegistryObjectDto> GetDocumentRegistryContentAsDtos(PatientId? patientIdentifier = null)
    {
        if (_registryObjectList != null && patientIdentifier == null)
            return _registryObjectList;

        if (patientIdentifier != null)
            return _registry.GetRegistryItemsForPatient(patientIdentifier);

        try
        {
            _registryObjectList = _registry.ReadRegistry().ToBlockingEnumerable();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading registry content: {Message}", ex.Message);
            _registryObjectList = [];
        }

        return _registryObjectList;
    }

    public OperationResponse SetDocumentRegistryContentWithDtos(List<RegistryObjectDto>? registryObjectDtos)
    {
        if (registryObjectDtos == null) return OperationResponse.Failure("No registry objects provided");

        var response = _registry.WriteRegistry(registryObjectDtos);
        _registryObjectList = _registry.ReadRegistry().ToBlockingEnumerable();
        return response;
    }

    public OperationResponse InsertOrUpdateDocumentRegistryContentWithDtos(RegistryObjectDto registryObjectDto)
    {
        return InsertOrUpdateDocumentRegistryContentWithDtos(new List<RegistryObjectDto>() { registryObjectDto });
    }

    public OperationResponse DeleteRegistryObjectFromRegistry(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return OperationResponse.Failure("No ID provided");

        var deleteResponse = _registry.DeleteRegistryItem(id);
        _registryObjectList = _registry.ReadRegistry().ToBlockingEnumerable();
        
        return deleteResponse;
    }

    public OperationResponse DeleteRegistryObjectFromRegistry(RegistryObjectDto registryObjectDto)
    {
        return DeleteRegistryObjectFromRegistry(registryObjectDto.Id);
    }

    public OperationResponse AddDocumentReferenceDtosToDocumentRegistry(List<RegistryObjectDto> registryObjectDtos)
    {
        if (registryObjectDtos.Count == 0) return OperationResponse.Failure("No registry objects provided");
        _registryObjectList ??= GetDocumentRegistryContentAsDtos();

        var response = _registry.AddItemsToRegistry(registryObjectDtos);
        _registryObjectList = _registry.ReadRegistry().ToBlockingEnumerable();

        return response;
    }

    public OperationResponse InsertOrUpdateDocumentRegistryContentWithDtos(List<RegistryObjectDto> registryObjectDtos)
    {
        if (registryObjectDtos.Count == 0) return OperationResponse.Failure("No registry objects provided");
        _registryObjectList ??= GetDocumentRegistryContentAsDtos();

        var response = _registry.InsertOrUpdateRegistry(registryObjectDtos);
        _registryObjectList = _registry.ReadRegistry().ToBlockingEnumerable();

        return response;
    }
}