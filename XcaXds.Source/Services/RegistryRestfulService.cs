using Microsoft.Extensions.Logging;
using XcaXds.Commons.Models.Custom.DocumentEntryDto;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;
using XcaXds.Commons.Xca;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons;

namespace XcaXds.Source.Services;

public class RegistryRestfulService
{
    private readonly XdsConfig _xdsConfig;
    private readonly RegistryWrapper _registryWrapper;
    private List<RegistryObjectDto> _documentRegistry;
    private readonly ILogger<RegistryService> _logger;
    private readonly RegistryMetadataTransformerService _metadataTransformerService;


    public RegistryRestfulService(XdsConfig xdsConfig, XcaGateway xcaGateway, RegistryWrapper registryWrapper, ILogger<RegistryService> logger, RegistryMetadataTransformerService metadataTransformerService)
    {
        _xdsConfig = xdsConfig;
        _registryWrapper = registryWrapper;
        _logger = logger;
        _metadataTransformerService = metadataTransformerService;
        // Explicitly load the registry upon service creation
        _documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos() ?? new List<RegistryObjectDto>();
    }

    public List<DocumentEntryDto> GetDocumentListForPatient(string patientId)
    {
        var patientIdCx = Hl7Object.Parse<CX>(patientId);

        // Account for searches only including the patient Id (eg api/GetDocumentList?id=13116900216)
        // Add default assigning authority if missing
        patientIdCx.AssigningAuthority ??= new() { UniversalId = Constants.Oid.Fnr, UniversalIdType = Constants.Hl7.UniversalIdType.Iso };

        var patientDocumentReferences = _documentRegistry
            .OfType<DocumentEntryDto>()
            .ByDocumentEntryPatientId(patientIdCx);            

        return patientDocumentReferences.ToList();
    }

}
