using Microsoft.Extensions.Logging;
using XcaXds.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.DocumentEntry;
using XcaXds.Commons.Models.Custom.RestfulRegistry;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Services;
using XcaXds.Commons.Xca;

namespace XcaXds.Source.Services;

public class RestfulRegistryService
{
    private readonly ApplicationConfig _appConfig;
    private readonly RegistryWrapper _registryWrapper;
    private readonly ILogger<XdsRegistryService> _logger;
    private readonly RegistryMetadataTransformerService _metadataTransformerService;


    public RestfulRegistryService(ApplicationConfig appConfig, XcaGateway xcaGateway, RegistryWrapper registryWrapper, ILogger<XdsRegistryService> logger, RegistryMetadataTransformerService metadataTransformerService)
    {
        _appConfig = appConfig;
        _registryWrapper = registryWrapper;
        _logger = logger;
        _metadataTransformerService = metadataTransformerService;
    }

    public DocumentListResponse GetDocumentListForPatient(string? patientId, string? status)
    {
        var documentListResponse = new DocumentListResponse();

        if (string.IsNullOrWhiteSpace(patientId))
        {
            documentListResponse.AddError("RequiredParameterMissing", "Parameter 'id' is required.");
            return documentListResponse;
        }

        var allowedStatuses = new[] { "Approved", "Deprecated" };

        if (!string.IsNullOrWhiteSpace(status) && !allowedStatuses.Contains(status))
        {
            documentListResponse.AddError("RequiredParameterMissing", @$"Status must be one of ""Approved"" or ""Deprecated"", got {status}.");
            return documentListResponse;

        }



        var patientIdCx = Hl7Object.Parse<CX>(patientId);

        // Account for searches only including the patient Id and not assigning authority (eg api/GetDocumentList?id=13116900216)
        // Add default assigning authority if missing
        patientIdCx.AssigningAuthority ??= new() { UniversalId = Constants.Oid.Fnr, UniversalIdType = Constants.Hl7.UniversalIdType.Iso };

        var documentRegistry = _registryWrapper.GetDocumentRegistryContentAsDtos();

        var patientDocumentReferences = documentRegistry
            .OfType<DocumentEntryDto>()
            .ByDocumentEntryPatientId(patientIdCx);

        var documentList = new DocumentListResponse()
        {

        };

        documentList.DocumentListEntries = patientDocumentReferences
            .Select(dr => new DocumentListEntry() 
            { 
                DocumentReference = dr,
                LinkToDocument = new()
                {
                    
                }
            }).ToList();

        return documentList;
    }

    public object GetDocument(string homecommunity_id, string repository_id, string document_id)
    {
        throw new NotImplementedException();
    }
}
