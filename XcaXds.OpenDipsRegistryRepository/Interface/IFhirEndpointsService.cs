using Hl7.Fhir.Model;

namespace XcaXds.OpenDipsRegistryRepository.Services;

public interface IFhirEndpointsService
{
    Task<Resource> FetchFromFhirEndpointAsync(string url, string apiKey);
}