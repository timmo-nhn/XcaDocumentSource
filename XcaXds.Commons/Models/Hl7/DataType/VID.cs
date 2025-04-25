using static XcaXds.Commons.Enums.Hl7;

namespace XcaXds.Commons.Models.Hl7.DataType;

public class VID
{
    public VersionId VersionId { get; set; }
    public CWE InternationalizationCode { get; set; }
    public CWE InternationalVersionId { get; set; }
}