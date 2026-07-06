namespace XcaXds.Commons.Interfaces;

public interface IVirusScanner
{
    Task<VirusScanResult> ScanFile(byte[] fileContent);
}