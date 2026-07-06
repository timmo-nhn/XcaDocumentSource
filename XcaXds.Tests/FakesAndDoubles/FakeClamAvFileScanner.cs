using nClam;
using System.Text;
using XcaXds.Commons.Interfaces;

namespace XcaXds.Tests.FakesAndDoubles;

public class FakeClamAvFileScanner : IVirusScanner
{
    private static readonly byte[] EicarTestFile = Encoding.UTF8.GetBytes("X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*");

    public Task<VirusScanResult> ScanFile(byte[] fileContent)
    {
        if (fileContent.SequenceEqual(EicarTestFile))
            return Task.FromResult<VirusScanResult>(VirusScanResult<ClamScanResult>.Failure("Document contains virus: EICAR test file detected", new ClamScanResult("Virus Found")));

        return Task.FromResult<VirusScanResult>(VirusScanResult<ClamScanResult>.Success("Document is clean", new ClamScanResult("File ok")));
    }
}