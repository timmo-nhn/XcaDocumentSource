using nClam;

namespace XcaXds.WebService.Services
{
    public interface IClamAvFileScanner
    {
        public Task<ClamScanResult?> ScanFile(byte[] fileContent);
    }
}