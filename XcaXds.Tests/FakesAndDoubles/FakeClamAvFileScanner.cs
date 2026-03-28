using nClam;
using System.Text;
using XcaXds.WebService.Services;

namespace XcaXds.Tests.FakesAndDoubles;


public class FakeClamAvFileScanner : IClamAvFileScanner
{
    private static readonly byte[] EicarTestFile = Encoding.UTF8.GetBytes("X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*");

    // Constructor for ClamScanResult 
    //        [......]
    //    if (text.EndsWith("ok"))
    //    {
    //        Result = ClamScanResults.Clean;
    //    }
    //    else if (text.EndsWith("error"))
    //    {
    //        Result = ClamScanResults.Error;
    //    }
    //    else if (text.EndsWith("found"))
    //    {
    //        Result = ClamScanResults.VirusDetected;
    //        [......]
    //    }
    //}
    //        [......]

    public async Task<ClamScanResult?> ScanFile(byte[] fileContent)
    {
        ClamScanResult result = new("File ok");

        if (fileContent.SequenceEqual(EicarTestFile))
        {
            // everything encoded in constructor
            result = new ClamScanResult("Virus Found");
        }
        return await Task.FromResult(result);
    }
}