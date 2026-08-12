using nClam;
using XcaXds.Commons.Interfaces;

namespace XcaXds.WebService.Services;

public class ClamAvFileScanner : IVirusScanner
{
    private readonly ILogger<ClamAvFileScanner> _logger;
    private readonly ApplicationConfig _config;
    private readonly ClamClient _scanClient;

    public ClamAvFileScanner(ILogger<ClamAvFileScanner> logger, ApplicationConfig config)
    {
        _logger = logger;
        _config = config;
        _scanClient = new ClamClient(_config.VirusScannerEndpoint ?? throw new InvalidOperationException("ClamAV server endpoint is not configured."));
    }

    public async Task<VirusScanResult> ScanFile(byte[] fileContent)
    {
        ClamScanResult? clamResult = null;
        try
        {
            clamResult = await _scanClient.SendAndScanFileAsync(fileContent);
        }
        catch (Exception ex)
        {
            var failure = VirusScanResult<ClamScanResult>.Failure($"Error while trying to send file to ClamAV server \"{_scanClient.Server}:{_scanClient.Port}\": {ex}");
            _logger.LogError(failure.Message);
            return failure;
        }

        VirusScanResult<ClamScanResult> result = clamResult.Result switch
        {
            ClamScanResults.Clean => VirusScanResult<ClamScanResult>.Success("Document is clean", clamResult),
            ClamScanResults.VirusDetected => VirusScanResult<ClamScanResult>.Failure($"Document contains virus: {clamResult.RawResult}", clamResult),
            _ => VirusScanResult<ClamScanResult>.Failure("Error while scanning for virus", clamResult)
        };

        _logger.LogInformation(result.Message);
        return result;
    }
}
