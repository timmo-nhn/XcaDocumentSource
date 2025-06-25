using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using System.Web;

namespace XcaXds.OpenDipsRegistryRepository.Services;

public class FhirEndpointsDtoTransformerService
{
    internal volatile List<Resource> _fhirResources = new();

    private readonly ILogger<FhirEndpointsDtoTransformerService> _logger;

    private readonly IFhirEndpointsService _fhirEndpointsService;

    public string FhirServerUrl { get; set; }


    public FhirEndpointsDtoTransformerService(string fhirServer, ILogger<FhirEndpointsDtoTransformerService> logger, IFhirEndpointsService fhirEndpointsService)
    {
        _logger = logger;
        _fhirEndpointsService = fhirEndpointsService;
        FhirServerUrl = fhirServer;
    }


    public void SetFhirUrl(string url)
    {
        FhirServerUrl = url;
    }


    public async Task<Bundle> GetDocumentReference(string patientId, string apiKey)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);

        queryParams["Patient.Identifier"] = patientId;

        var resources = (Bundle)await _fhirEndpointsService.FetchFromFhirEndpointAsync(FhirServerUrl + "/DocumentReference" + $"?{queryParams.ToString()}", apiKey);

        return resources;
    }

    public async Task<Resource> GetResource(string resourceUrl, string apiKey)
    {
        var resources = await _fhirEndpointsService.FetchFromFhirEndpointAsync($"{FhirServerUrl}/{resourceUrl}", apiKey);

        return resources;
    }

    public void AddResource(Resource resource)
    {
        _fhirResources.Add(resource);
    }

    public Resource? GetResource(string identifier)
    {
        return _fhirResources.Find(rs => rs.Id == identifier);
    }


    public List<string> GetResourceIdentifiersFromDocumentEntries(Bundle bundle)
    {
        var externalFhirResources = bundle.Entry.SelectMany(ent =>
        {
            var documentReference = ent.Resource as DocumentReference;

            var identifierList = new List<string>();

            identifierList.AddRange(documentReference?.Author?.Select(aut => aut.Reference));
            identifierList.Add(documentReference?.Custodian?.Reference);
            identifierList.Add(documentReference?.Subject?.Reference);
            identifierList.Add(documentReference?.Authenticator?.Reference);

            return identifierList;
        })
        .Where(str => str != null)
        .Distinct()
        .ToList();

        return externalFhirResources;
    }

}
