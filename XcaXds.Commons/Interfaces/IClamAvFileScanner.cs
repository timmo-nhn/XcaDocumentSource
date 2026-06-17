using nClam;

namespace XcaXds.Commons.Interfaces;

public interface IClamAvFileScanner
{
    public Task<ClamScanResult?> ScanFile(byte[] fileContent);
}