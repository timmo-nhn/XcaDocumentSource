
using XcaXds.Commons.Models.Hl7.DataType;
using static XcaXds.Commons.Enums.Hl7;

namespace XcaXds.Commons.Models.Hl7.Segment;

public class MSH
{
    public string FieldSeparator { get; set; }
    public string EncodingCharacters { get; set; }
    public HD SendingApplication { get; set; }
    public HD SendingFacility { get; set; }
    public HD ReceivingApplication { get; set; }
    public HD ReceivingFacility { get; set; }
    public TS DateTimeOfMessage { get; set; }
    public string Security { get; set; }
    public MSG MessageType { get; set; }
    public string MessageControlId { get; set; }
    public PT ProcessingId { get; set; }
    public VID VersionId { get; set; }
    public int SequenceNumber { get; set; }
    public string ContinuationPointer { get; set; }
    public VersionId AcceptAcknowledgementType { get; set; }
    public VersionId ApplicationAcknowledgementType { get; set; }
    public string CountryCode { get; set; }
    public AlternateCharacterSets AlternateCharacterSets { get; set; }
    public CWE PrincipalLanguageOfMessage { get; set; }
    public AlternateCharacterSetsHandlingSheme AlternateCharacterSetsHandlingSheme { get; set; }
    public EI MessageProfileIdentifier { get; set; }
    public XON SendingResponsibleOrganization { get;set }
    public XON ReceivingResponsibleOrganization { get; set; }
    public HD SendingNetworkAddress { get; set; }
    public HD ReceivingNetworkAddress { get;set; }

}
