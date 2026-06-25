using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Shared;
using XcaXds.Shared.Extensions;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;
using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;

public class FhirValidateBundleAtnaLogStrategy : IAtnaLogStrategy
{
    private readonly AtnaLogGeneratorService _atnaLogGeneratorService;
    private readonly AtnaLogEnricherService _atnaLogEnricherService;

    public FhirValidateBundleAtnaLogStrategy(AtnaLogGeneratorService atnaLogGeneratorService, AtnaLogEnricherService atnaLogEnricherService)
    {
        _atnaLogGeneratorService = atnaLogGeneratorService;
        _atnaLogEnricherService = atnaLogEnricherService;
    }

    public bool CanHandle(string path, string? contentType, string method)
    {
        return path.StartsWith("/R4/fhir/") && path.EndsWith("/$validate") && contentType.IsAnyOf(Constants.MimeTypes.FhirJson) && method == "POST";
    }

    public async Task<AtnaLogBuilderResult> BuildAsync(HttpContext context, Stream requestBody, Stream responseBody)
    {
        var jwtToken = context.Request.Headers.Authorization.FirstOrDefault();

        var requestString = await HttpRequestResponseExtensions.GetStreamAsStringAsync(requestBody) ?? throw new InvalidOperationException($"{context.TraceIdentifier} - Request stream is null!");
        var responseString = await HttpRequestResponseExtensions.GetStreamAsStringAsync(responseBody) ?? throw new InvalidOperationException($"{context.TraceIdentifier} - Response stream is null!");

        var fhirParser = new FhirJsonDeserializer();

        var fhirBundle = fhirParser.Deserialize<Bundle>(requestString) ?? throw new InvalidOperationException($"{context.TraceIdentifier} - Input is not valid FHIR Bundle");
        var fhirResponse = fhirParser.Deserialize<Resource>(responseString) ?? throw new InvalidOperationException($"{context.TraceIdentifier} - Input is not valid FHIR Bundle");

        // HttpContext-Items returned from FhirMobileAccessToHealthDocumentsController is almost a complete ITI-41 request,
        // since it uses the same services as Xds Registry/Repository based stuff,
        // so use these and convert the ProvideBundle to an ITI-41 and use the Soap-based AtnaLogGenerator-"path"
        var pdpDecision = context.Items.TryGetValue("pdpDecision", out var decision) ? decision as AccessControlResponse : null;

        var additionalParameters = new AdditionalParameters(context.Request.Method, context.TraceIdentifier, pdpDecision, null, context.Request.Path);

        var registryErrorsFromFhirResponse = XdsErrorToOperationOutcomeMapper.GetXdsErrorsFromOperationOutcome(fhirResponse as OperationOutcome);


        var mockSoapRequest = _atnaLogEnricherService.GetMockSoapEnvelopeFromJwtAndBundle(
            additionalParameters,
            jwtToken,
            fhirBundle,
            null);

        var mockSoapResponse = _atnaLogEnricherService.GetMockSoapEnvelopeFromJwtAndBundle(
            additionalParameters,
            jwtToken,
            fhirResponse,
            null);

        _atnaLogGeneratorService.CreateAuditLogForSoapRequestResponse(additionalParameters, mockSoapRequest, mockSoapResponse);
        return AtnaLogBuilderResult.Success($"{context.TraceIdentifier} - Successfully enqueued AuditMessage for request {context.TraceIdentifier}");
    }
}