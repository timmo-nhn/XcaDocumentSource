using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;

namespace XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;

public class FhirProvideBundleStrategy : IAtnaLogStrategy
{
    private readonly AtnaLogGeneratorService _atnaLogGeneratorService;
    public FhirProvideBundleStrategy(AtnaLogGeneratorService atnaLogGeneratorService)
    {
        _atnaLogGeneratorService = atnaLogGeneratorService;
    }
    public bool CanHandle(string contentType, string method)
    {
        return contentType.IsAnyOf(Constants.MimeTypes.FhirJson) && method == "POST";
    }

    public async Task<AtnaLogBuilderResult> BuildAsync(HttpContext context)
    {
        return AtnaLogBuilderResult.Success($"{context.TraceIdentifier} - Successfully enqueued AuditMessage for request {context.TraceIdentifier}");
    }
}
