namespace XcaXds.Commons.Models.Hl7;

public class Hd : Hl7Object
{
    [Hl7(Sequence = 1)]
    public string NamespaceId { get; set; }
    [Hl7(Sequence = 2)]
    public string UniversalId { get; set; }
    [Hl7(Sequence = 3)]
    public string UniversalIdType { get; set; }
}
