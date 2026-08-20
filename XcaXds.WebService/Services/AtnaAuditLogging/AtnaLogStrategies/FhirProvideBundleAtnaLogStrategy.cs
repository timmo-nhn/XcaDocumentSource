using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared;
using XcaXds.Shared.Extensions;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;
using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;

public class FhirProvideBundleAtnaLogStrategy : IAtnaLogStrategy
{
    private readonly AtnaLogGeneratorService _atnaLogGeneratorService;
    private readonly AtnaLogEnricherService _atnaLogEnricherService;

    public FhirProvideBundleAtnaLogStrategy(AtnaLogGeneratorService atnaLogGeneratorService, AtnaLogEnricherService atnaLogEnricherService)
    {
        _atnaLogGeneratorService = atnaLogGeneratorService;
        _atnaLogEnricherService = atnaLogEnricherService;
    }

    public bool CanHandle(string path, string? contentType, string method)
    {
        return path.StartsWith("/R4/fhir/") && !path.EndsWith("/$validate") && contentType.IsAnyOf(Constants.MimeTypes.FhirJson) && method == "POST";
    }

    public async Task<AtnaLogBuilderResult> BuildAsync(HttpContext context, Stream requestBody, Stream responseBody)
    {
        var jwtToken = context.Request.Headers.Authorization.FirstOrDefault();

        var requestString = await HttpRequestResponseExtensions.GetStreamAsStringAsync(requestBody) ?? throw new InvalidOperationException($"{context.TraceIdentifier} - Request stream is null!");
        var responseString = await HttpRequestResponseExtensions.GetStreamAsStringAsync(responseBody) ?? throw new InvalidOperationException($"{context.TraceIdentifier} - Response stream is null!");

        var fhirParser = new FhirJsonDeserializer();

        var fhirBundle = fhirParser.TryTryDeserializeResource(requestString, out var bundle, out var requestIssues) ? bundle as Bundle : null;
        var fhirResponse = fhirParser.TryTryDeserializeResource(responseString, out var response, out var responseIssues) ? response : null;

        if (requestIssues.Any())
        {
            var allErrors = requestIssues.Concat(responseIssues).Select(iss => iss.Message);
            var operationOutcome = fhirResponse as OperationOutcome;

            if (operationOutcome != null)
            {
                foreach (var item in allErrors)
                {
                    operationOutcome.Issue.Add(new()
                    {
                        Severity = OperationOutcome.IssueSeverity.Error,
                        Code = OperationOutcome.IssueType.Invalid,
                        Diagnostics = item
                    });
                }

                // Write the updated OperationOutcome back to the buffered response body
                // so the enriched issues are included in the response sent to the client
                var updatedResponseBytes = new FhirJsonSerializer().SerializeToBytes(operationOutcome);

                responseBody.SetLength(0);
                await responseBody.WriteAsync(updatedResponseBytes);
                responseBody.Seek(0, SeekOrigin.Begin);

                if (!context.Response.HasStarted)
                {
                    context.Response.ContentLength = updatedResponseBytes.Length;
                }
            }

            return AtnaLogBuilderResult.Fail($"{context.TraceIdentifier} - error while parsing fhir request or response\n {string.Join('\n', allErrors)}");
        }

        // HttpContext-Items returned from FhirMobileAccessToHealthDocumentsController is almost a complete ITI-41 request,
        // since it uses the same services as Xds Registry/Repository based stuff,
        // so use these and convert the ProvideBundle to an ITI-41 and use the Soap-based AtnaLogGenerator-"path"
        var uploadedEntries = (context.Items.TryGetValue("uploadedEntries", out var entries) ? entries : null) as IdentifiableType[];
        var registryResponse = (context.Items.TryGetValue("uploadedEntriesRegistryResponse", out var regrep) ? regrep : null) as SoapEnvelope;
        var pdpDecision = context.Items.TryGetValue("pdpDecision", out var decision) ? decision as AccessControlResponse : null;
        var businessLogicResult = (context.Items.TryGetValue("businessLogicResult", out var blRes) ? blRes : null) as Dictionary<string, int>;

        var additionalParameters = new AdditionalParameters(context.Request.Method, context.TraceIdentifier, pdpDecision, businessLogicResult);

        var registryErrorsFromFhirResponse = XdsErrorToOperationOutcomeMapper.GetXdsErrorsFromOperationOutcome(fhirResponse as OperationOutcome);

        registryResponse?.Body.RegistryResponse?.RegistryErrorList?.RegistryError = [.. registryResponse.Body.RegistryResponse.RegistryErrorList.RegistryError, .. registryErrorsFromFhirResponse?.RegistryError ?? []];

        var mockSoapResponse = _atnaLogEnricherService.GetMockSoapEnvelopeFromJwtAndBundle(
            additionalParameters,
            jwtToken,
            fhirBundle,
            uploadedEntries);

        _atnaLogGeneratorService.CreateAuditLogForSoapRequestResponse(additionalParameters, mockSoapResponse, registryResponse);
        return AtnaLogBuilderResult.Success($"{context.TraceIdentifier} - Successfully enqueued AuditMessage for request {context.TraceIdentifier}");
    }
}