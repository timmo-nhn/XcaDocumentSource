namespace XcaXds.Commons.Models.Hl7.DataType;

public class CWE: Hl7Object
{
    [Hl7(Sequence = 1)]
    public string Identifier { get; set; }
    [Hl7(Sequence = 2)]
    public string Text { get; set; }
}
