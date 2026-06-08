using XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogStrategies;

namespace XcaXds.WebService.Services.AtnaAuditLogging.AtnaLogBuilder;

public class AtnaLogBuilder
{
    private readonly ILogger<AtnaLogBuilder> _logger;
    private readonly IEnumerable<IAtnaLogStrategy> _strategies;
    private readonly ApplicationConfig _appConfig;

    public AtnaLogBuilder(ILogger<AtnaLogBuilder> logger, IEnumerable<IAtnaLogStrategy> strategies, ApplicationConfig appConfig)
    {
        _appConfig = appConfig;
        _logger = logger;
        _strategies = strategies;
    }

    public async Task<AtnaLogBuilderResult> BuildAsync(HttpContext context, Stream requestBody, Stream responseBody)
    {
        if (!_strategies.Any())
        {
            throw new InvalidOperationException("Missing strategies");
        }

        var path = context.Request.Path.Value ?? throw new InvalidOperationException($"{context.TraceIdentifier} - Path cannot be null!");
        var contentType = context.Request.ContentType?.Split(";").FirstOrDefault();
        var method = context.Request.Method;

        var combination = $"Path: {path}, Content-Type: {contentType}, Method: {method}";
        _logger.LogInformation(combination);

        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(path, contentType, method));


        if (strategy == null)
        {
            var message = $"No compatible AtnaLogStrategy for combination of {combination}";
            _logger.LogError(message);
            return AtnaLogBuilderResult.Fail(message);
        }

        return await strategy.BuildAsync(context, requestBody, responseBody);
    }
}