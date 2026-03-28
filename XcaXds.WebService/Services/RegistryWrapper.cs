using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.WebService.Services;


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
            var item = RegistryMetadataTransformer.TransformRegistryObjectDtoToRegistryObject(dto);

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
            _registryObjectList = _registry.ReadRegistry();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading registry content: {Message}", ex.Message);
            _registryObjectList = Enumerable.Empty<RegistryObjectDto>();
        }

        return _registryObjectList;
    }

    public bool SetDocumentRegistryContentWithDtos(List<RegistryObjectDto>? registryObjectDtos)
    {
        if (registryObjectDtos == null) return false;

        _registry.WriteRegistry(registryObjectDtos);
        _registryObjectList = _registry.ReadRegistry();
        return true;
    }

    public bool InsertOrUpdateDocumentRegistryContentWithDtos(RegistryObjectDto registryObjectDto)
    {
        return InsertOrUpdateDocumentRegistryContentWithDtos(new List<RegistryObjectDto>() { registryObjectDto });
    }

    public bool DeleteDocumentEntryFromRegistry(RegistryObjectDto registryObjectDto)
    {
        if (registryObjectDto == null) return false;

        var deleteResponse = _registry.DeleteRegistryItem(registryObjectDto.Id);
        _registryObjectList = _registry.ReadRegistry();


        return deleteResponse;
    }

    public bool UpdateDocumentRegistryContentWithDtos(List<RegistryObjectDto> registryObjectDtos)
    {
        if (registryObjectDtos.Count == 0) return false;
        _registryObjectList ??= GetDocumentRegistryContentAsDtos();

        _registry.UpdateRegistry(registryObjectDtos);
        _registryObjectList = _registry.ReadRegistry();

        return true;
    }

    public bool InsertOrUpdateDocumentRegistryContentWithDtos(List<RegistryObjectDto> registryObjectDtos)
    {
        if (registryObjectDtos.Count == 0) return false;
        _registryObjectList ??= GetDocumentRegistryContentAsDtos();

        _registry.InsertOrUpdateRegistry(registryObjectDtos);
        _registryObjectList = _registry.ReadRegistry();

        return true;
    }
}
