using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using XcaXds.AtnaAuditLogging.Services.AtnaAuditLogging.AtnaLogBuilder;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.AtnaAuditLogging.Services.AtnaAuditLogging.AtnaLogStrategies;

public class FhirValidateResourceAtnaLogStrategy : IAtnaLogStrategy
{
    private readonly AtnaLogGeneratorService _atnaLogGeneratorService;
    private readonly AtnaLogEnricherService _atnaLogEnricherService;

    public FhirValidateResourceAtnaLogStrategy(AtnaLogGeneratorService atnaLogGeneratorService, AtnaLogEnricherService atnaLogEnricherService)
    {
        _atnaLogGeneratorService = atnaLogGeneratorService;
        _atnaLogEnricherService = atnaLogEnricherService;
    }

    public bool CanHandle(string path, string? contentType, string method)
    {
        return path.StartsWith("R4/fhir/") && path.EndsWith("/$validate") && contentType.IsAnyOf(Constants.MimeTypes.FhirJson) && method == "POST";
    }

    public async Task<AtnaLogBuilderResult> BuildAsync(HttpContext context, Stream requestBody, Stream responseBody)
    {
        var jwtToken = context.Request.Headers["Authorization"].FirstOrDefault();

        var requestString = await HttpRequestResponseExtensions.GetStreamAsStringAsync(requestBody) ?? throw new InvalidOperationException($"{context.TraceIdentifier} - Request stream is null!");
        ;
        var fhirParser = new FhirJsonDeserializer();

        var fhirBundle = fhirParser.Deserialize<Bundle>(requestString) ?? throw new InvalidOperationException($"{context.TraceIdentifier} - Input is not valid FHIR Bundle");

        var uploadedEntries = (context.Items.TryGetValue("uploadedEntries", out var entries) ? entries : null) as IdentifiableType[];
        var registryResponse = (context.Items.TryGetValue("uploadedEntriesRegistryResponse", out var regrep) ? regrep : null) as SoapEnvelope;
        var pdpDecision = context.Items.TryGetValue("pdpDecision", out var decision) ? decision as AccessControlResponse : null;

        var mockSoapResponse = _atnaLogEnricherService.GetMockSoapEnvelopeFromJwtAndBundle(new AdditionalParameters(context.Request.Method, context.TraceIdentifier, pdpDecision), jwtToken, fhirBundle, uploadedEntries);

        _atnaLogGeneratorService.CreateAuditLogForSoapRequestResponse(
            new AdditionalParameters(
                context.Request.Method,
                context.TraceIdentifier,
                pdpDecision),
            mockSoapResponse,
            registryResponse);
        return AtnaLogBuilderResult.Success($"{context.TraceIdentifier} - Successfully enqueued AuditMessage for request {context.TraceIdentifier}");
    }
}