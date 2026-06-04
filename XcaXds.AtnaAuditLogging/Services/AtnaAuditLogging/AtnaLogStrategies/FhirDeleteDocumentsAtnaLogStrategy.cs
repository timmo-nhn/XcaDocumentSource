using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Runtime.CompilerServices;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;
using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.AtnaAuditLogging.Services.AtnaAuditLogging.AtnaLogStrategies;

public class FhirDeleteDocumentsAtnaLogStrategy : IAtnaLogStrategy
{
    private readonly ILogger<FhirDeleteDocumentsAtnaLogStrategy> _logger;
    private readonly AtnaLogGeneratorService _atnaLogGeneratorService;

    public FhirDeleteDocumentsAtnaLogStrategy(AtnaLogGeneratorService atnaLogGeneratorService, ILogger<FhirDeleteDocumentsAtnaLogStrategy> logger)
    {
        _atnaLogGeneratorService = atnaLogGeneratorService;
        _logger = logger;
    }

    public bool CanHandle(string path, string? contentType, string method)
    {
        return path.StartsWith("/R4/fhir/DocumentReference") && method == "DELETE";
    }

    public async Task<AtnaLogBuilderResult> BuildAsync(HttpContext context, Stream requestBody, Stream responseBody)
    {
        var fhirParser = new FhirJsonDeserializer();

        var request = context.Request;
        var response = context.Response;

        var jwt = JwtExtractor.ExtractJwt(request.Headers, out var ok);

        if (ok == false)
        {
            return AtnaLogBuilderResult.Fail($"{context.TraceIdentifier} - JWT extraction failed! AtnaLog cannot be created for this request");
        }

        var operationOutcome = fhirParser.DeserializeResource(await HttpRequestResponseExtensions.GetStreamAsStringAsync(response.Body)) as OperationOutcome;

        if (operationOutcome == null)
        {
            return AtnaLogBuilderResult.Fail($"{context.TraceIdentifier} - No OperationOutcome in response! AtnaLog cannot be created for this request");
        }

        var deletedEntry = context.Items.TryGetValue("deletedEntry", out var entry) ? entry as DocumentEntryDto : null;
        var pdpDecision = context.Items.TryGetValue("pdpDecision", out var decision) ? decision as AccessControlResponse : null;

        _atnaLogGeneratorService.CreateAuditLogForFhirDeleteDocumentsRequest(
            new AdditionalParameters(
                request.Method,
                context.TraceIdentifier,
                pdpDecision),
            deletedEntry,
            operationOutcome,
            jwt);

        return AtnaLogBuilderResult.Success($"{context.TraceIdentifier} - Successfully enqueued AuditMessage for request {context.TraceIdentifier}");
    }
}
