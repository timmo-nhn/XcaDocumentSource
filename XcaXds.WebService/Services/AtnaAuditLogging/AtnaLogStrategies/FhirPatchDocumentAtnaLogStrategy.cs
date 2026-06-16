using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Shared.Constants;
using XcaXds.Shared.Extensions;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;
using XcaXds.WebService.Services.PolicyEnforcementPoint;

namespace XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;

public class FhirPatchDocumentAtnaLogStrategy : IAtnaLogStrategy
{
    private readonly AtnaLogGeneratorService _atnaLogGeneratorService;

    public FhirPatchDocumentAtnaLogStrategy(AtnaLogGeneratorService atnaLogGeneratorService)
    {
        _atnaLogGeneratorService = atnaLogGeneratorService;
    }

    public bool CanHandle(string path, string? contentType, string method)
    {
        return contentType.IsAnyOf(Constants.MimeTypes.FhirJson) && method == "PATCH";
    }

    public async Task<AtnaLogBuilderResult> BuildAsync(HttpContext context, Stream requestBody, Stream responseBody)
    {
        // Atna log generation
        var oldLabel = (context.Items.TryGetValue("oldSecurityLabel", out var label) ? label : null) as List<CodedValue>;
        var pathedEntry = (context.Items.TryGetValue("patchedDocumentEntry", out var entry) ? entry : null) as DocumentEntryDto;
        var pdpDecision = context.Items.TryGetValue("pdpDecision", out var decision) ? decision as AccessControlResponse : null;

        var token = JwtExtractor.ExtractJwt(context.Request.Headers, out var _);
        var businessLogicResult = (context.Items.TryGetValue("businessLogicResult", out var cast) ? cast : null) as Dictionary<string, int>;

        var additionalParameters = new AdditionalParameters(context.Request.Method, context.TraceIdentifier, pdpDecision, businessLogicResult);

        _atnaLogGeneratorService.CreateAuditLogForFhirPatchDocumentSecurityLabelRequest(
            additionalParameters,
            oldLabel,
            pathedEntry,
            token);

        return AtnaLogBuilderResult.Success($"{context.TraceIdentifier} - Successfully enqueued AuditMessage for request {context.TraceIdentifier}");
    }
}