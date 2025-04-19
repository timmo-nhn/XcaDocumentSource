namespace XcaXds.Commons.Models.Hl7.Segment;

using NHapi.Model.V25.Segment;
using XcaXds.Commons.Models.Hl7.DataType;

public class UAC
{
    public CWE UserAuthenticationCredentialTypeCode { get; set; }
    public ED UserAuthenticationCredential { get; set; }
}
