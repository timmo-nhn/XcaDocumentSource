using Microsoft.Extensions.Logging;
using XcaXds.Commons.Interfaces;

namespace XcaXds.Commons.Helpers;

public class NinParserFactory
{
    private readonly ILogger<NinParserFactory> _logger;
    public static IEnumerable<INinParser>? _ninParsers;

    public NinParserFactory(IEnumerable<INinParser> ninParsers, ILogger<NinParserFactory> logger)
    {
        _ninParsers = ninParsers;
        _logger = logger;
    }

    public INinParser? CreateNinParser(string? nin)
    {
        if (string.IsNullOrWhiteSpace(nin)) return null;

        var strategy = _ninParsers?.FirstOrDefault(s => s.CanHandle(nin));

        if (strategy == null)
            _logger.LogInformation($"No suitable Nin Parser found for NIN: {nin}");

        return strategy;
    }
}
