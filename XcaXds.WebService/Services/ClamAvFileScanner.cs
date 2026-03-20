using nClam;

namespace XcaXds.WebService.Services;

public class ClamAvFileScanner
{
    private readonly ILogger<ClamAvFileScanner> _logger;
    private readonly ApplicationConfig _config;
    private readonly ClamClient _scanClient;

    public ClamAvFileScanner(ILogger<ClamAvFileScanner> logger, ApplicationConfig config)
    {
        _logger = logger;
        _config = config;
        _scanClient = new ClamClient(_config.ClamAvEndpoint);
    }

    public async Task<ClamScanResult?> ScanFile(byte[] fileContent)
    {
        ClamScanResult? scanResult = null;
        try
        {
            scanResult = await _scanClient.SendAndScanFileAsync(fileContent);
            _logger.LogInformation($"File scanned with result: {scanResult.Result}");
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Error while trying send file to ClamAv server: \"{_scanClient.Server?.ToString() + ":" + _scanClient.Port}\": {ex.ToString()}");
        }

        return scanResult;
    }
}
