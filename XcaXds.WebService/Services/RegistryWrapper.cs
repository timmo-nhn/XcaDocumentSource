using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.WebService.Services;


public class RegistryWrapper
{
    private readonly IRegistry _documentRegistry;
    private readonly ILogger<RegistryWrapper> _logger;


    internal static readonly string IndexSeparator = "^^";
    internal volatile IEnumerable<RegistryObjectDto>? _registryObjectList = null;
    internal volatile Dictionary<string, List<DocumentEntryDto>> _registryObjectsByPatientId = new();
    public RegistryWrapper(IRegistry documentRegistry, ILogger<RegistryWrapper> logger)
    {
        _documentRegistry = documentRegistry;
    }

    public void BuildIndex()
    {
        var index = _registryObjectList?
            .OfType<DocumentEntryDto>()
            .Where(de => de.SourcePatientInfo?.PatientId?.Id != null)
            .GroupBy(de => de.SourcePatientInfo!.PatientId!.System + IndexSeparator + de.SourcePatientInfo!.PatientId!.Id)
            .ToDictionary(g => g.Key, g => g.ToList());

        if (index != null)
        {
            _registryObjectsByPatientId = index;
        }
    }

    public IEnumerable<RegistryObjectDto>? GetDocumentRegistryContentAsDtosByPatientId(PatientId? patientIdentifier = null)
    {
        if (patientIdentifier == null || patientIdentifier.Id == null) return null;

        var key = patientIdentifier.System + IndexSeparator + patientIdentifier.Id;

        return _registryObjectsByPatientId.TryGetValue(key, out var list) ? list : Enumerable.Empty<RegistryObjectDto>();
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

    public IEnumerable<RegistryObjectDto> GetDocumentRegistryContentAsDtos(PatientId? patientIdentifier = null)
    {
        if (_registryObjectList != null && patientIdentifier == null)
            return _registryObjectList;

        // Return pre-filtered list
        if (patientIdentifier != null)
            return _documentRegistry.ReadRegistry(patientIdentifier);

        try
        {
            _registryObjectList = _documentRegistry.ReadRegistry();
            BuildIndex();
        }
        catch
        {
            _registryObjectList = Enumerable.Empty<RegistryObjectDto>();
        }

        return _registryObjectList;
    }

    public bool SetDocumentRegistryContentWithDtos(List<RegistryObjectDto>? registryObjectDtos)
    {
        if (registryObjectDtos == null) return false;

        _documentRegistry.WriteRegistry(registryObjectDtos);
        _registryObjectList = _documentRegistry.ReadRegistry();
        BuildIndex();
        return true;
    }

    public bool UpdateDocumentRegistryContentWithDtos(RegistryObjectDto registryObjectDto)
    {
        return UpdateDocumentRegistryContentWithDtos(new List<RegistryObjectDto>() { registryObjectDto });
    }

    public bool InsertOrUpdateDocumentRegistryContentWithDtos(RegistryObjectDto registryObjectDto)
    {
        return InsertOrUpdateDocumentRegistryContentWithDtos(new List<RegistryObjectDto>() { registryObjectDto });
    }

    public bool DeleteDocumentEntryFromRegistry(RegistryObjectDto registryObjectDto)
    {
        if (registryObjectDto == null) return false;

        var deleteResponse = _documentRegistry.DeleteRegistryItem(registryObjectDto.Id);
        _registryObjectList = _documentRegistry.ReadRegistry();
        BuildIndex();


        return deleteResponse;
    }

    public bool UpdateDocumentRegistryContentWithDtos(List<RegistryObjectDto> registryObjectDtos)
    {
        if (registryObjectDtos.Count == 0) return false;
        _registryObjectList ??= GetDocumentRegistryContentAsDtos();

        _documentRegistry.UpdateRegistry(registryObjectDtos);
        _registryObjectList = _documentRegistry.ReadRegistry();
        BuildIndex();

        return true;
    }

    public bool InsertOrUpdateDocumentRegistryContentWithDtos(List<RegistryObjectDto> registryObjectDtos)
    {
        if (registryObjectDtos.Count == 0) return false;
        _registryObjectList ??= GetDocumentRegistryContentAsDtos();

        _documentRegistry.InsertOrUpdateRegistry(registryObjectDtos);
        _registryObjectList = _documentRegistry.ReadRegistry();
        BuildIndex();

        return true;
    }

    public SoapRequestResult<string> SetDocumentRegistryFromRegistryObjects(IdentifiableType[] registryObjects)
    {
        try
        {
            var dtoList = RegistryMetadataTransformer.TransformRegistryObjectsToRegistryObjectDtos(registryObjects);

            SetDocumentRegistryContentWithDtos([.. dtoList]);

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
            var dtoList = RegistryMetadataTransformer
                .TransformRegistryObjectsToRegistryObjectDtos(registryObjects);

            UpdateDocumentRegistryContentWithDtos([.. dtoList]);

            return new SoapRequestResult<string>().Success("Updated OK");
        }
        catch (Exception ex)
        {
            return new SoapRequestResult<string>().Fault($"Error updating registry: {ex.Message}");
        }
    }
}
