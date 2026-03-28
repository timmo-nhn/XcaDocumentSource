using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;

namespace XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;

public class AtnaLogBuilderService
{
    private readonly ILogger<AtnaLogBuilderService> _logger;
    private readonly IEnumerable<IAtnaLogStrategy> _strategies;
    private readonly ApplicationConfig _appConfig;
    public AtnaLogBuilderService(ILogger<AtnaLogBuilderService> logger, IEnumerable<IAtnaLogStrategy> strategies, ApplicationConfig appConfig)
    {
        _appConfig = appConfig;
        _logger = logger;
        _strategies = strategies;
    }

    public async Task<AtnaLogBuilderResult> BuildAsync(HttpContext context)
    {
        if (!_strategies.Any())
        {
            throw new InvalidOperationException("Missing strategies");
        }

        var contentType = context.Request.ContentType?.Split(";").FirstOrDefault();
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new InvalidOperationException("Missing Content-Type");
        }
        var method = context.Request.Method;

        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(contentType, method));
        if (strategy == null)
        {
            _logger.LogError($"Unknown content type: {contentType}");
            return AtnaLogBuilderResult.Fail($"Unknown content type: {contentType}");
        }

        _logger.LogInformation($"Content type: {contentType}");
        return await strategy.BuildAsync(context);
    }
}
