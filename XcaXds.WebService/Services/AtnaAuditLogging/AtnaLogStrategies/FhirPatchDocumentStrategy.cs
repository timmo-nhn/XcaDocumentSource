using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Helpers;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;

namespace XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;

public class FhirPatchDocumentStrategy : IAtnaLogStrategy
{
    private readonly AtnaLogGeneratorService _atnaLogGeneratorService;
    public FhirPatchDocumentStrategy(AtnaLogGeneratorService atnaLogGeneratorService)
    {
        _atnaLogGeneratorService = atnaLogGeneratorService;
    }

    public bool CanHandle(string path, string? contentType, string method)
    {
        return contentType.IsAnyOf(Constants.MimeTypes.FhirJson) && method == "PATCH";
    }

    public async Task<AtnaLogBuilderResult> BuildAsync(HttpContext context, Stream requestBody)
    {
        // Atna log generation
        var oldLabel = (context.Items.TryGetValue("oldSecurityLabel", out var label) ? label : null) as List<CodedValue>;
        var pathedEntry = (context.Items.TryGetValue("patchedDocumentEntry", out var entry) ? entry : null) as DocumentEntryDto;
        var token = JwtExtractor.ExtractJwt(context.Request.Headers, out var _);

        _atnaLogGeneratorService.CreateAuditLogForFhirPatchDocumentSecurityLabelRequest(
            new AdditionalParameters(context.Request.Method, context.TraceIdentifier, context.Request.Path.Value),
            oldLabel,
            pathedEntry,
            token);

        return AtnaLogBuilderResult.Success($"{context.TraceIdentifier} - Successfully enqueued AuditMessage for request {context.TraceIdentifier}");
    }
}
