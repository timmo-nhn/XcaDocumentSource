namespace XcaXds.Commons.Models.Hl7.DataType;

public class XTN : Hl7Object
{
    [Hl7(Sequence = 1)]
    public string TelephoneNumber { get; set; }
    [Hl7(Sequence = 2)]
    public string TelecommunicationUseCode { get; set; }
    [Hl7(Sequence = 3)]
    public string TelecommunicationEquipmentType { get; set; }
    [Hl7(Sequence = 4)]
    public string CommunicationAddress { get; set; }
}
