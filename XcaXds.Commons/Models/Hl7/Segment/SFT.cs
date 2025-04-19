
using XcaXds.Commons.Models.Hl7.DataType;
namespace XcaXds.Commons.Models.Hl7.Segment;

public class SFT
{
    public XON SoftwareVendorOrganization { get; set; }
    public string SoftwareCertifiedVersion { get; set; }
    public string SoftwareProductName { get; set; }
    public string SoftwareBinaryId { get; set; }
    public string SoftwareProductInformation { get; set; }
    public DateTime SoftwareInstallDate { get; set; }

}
