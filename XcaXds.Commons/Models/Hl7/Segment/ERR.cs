using static XcaXds.Commons.Enums.Hl7;
using XcaXds.Commons.Models.Hl7.DataType;

namespace XcaXds.Commons.Models.Hl7.Segment;

public class ERR
{
    public string ErrorCodeAndLocation { get; set; }

    public List<ERL> ErrorLocation { get; set; }

    public CWE Hl7ErrorCode { get; set; }

    public ErrorSeverity Severity { get; set; }

    public CWE ApplicationErrorCode { get; set; }
    
    public List<string> ApplicationErrorParameter { get; set; }

    public string DiagnosticInformation { get; set; }

    public string UserMessage { get; set; }

    public CWE InformPersonIndicator { get; set; }

    public CWE OverrideType { get; set; }

    public List<CWE> OverrideReasonCode { get; set; }

    public List<XTN> HelpDeskContactPoint { get; set; }
}
