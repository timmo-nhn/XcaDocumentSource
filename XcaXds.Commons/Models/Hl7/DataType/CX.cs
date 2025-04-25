namespace XcaXds.Commons.Models.Hl7.DataType;

public class CX : Hl7Object
{
    [Hl7(Sequence = 1)]
    public string IdNumber { get; set; }

    [Hl7(Sequence = 2)]
    public string IdentifierCheckDigit { get; set; }

    [Hl7(Sequence = 3)]
    public string CheckDigitScheme { get; set; }

    [Hl7(Sequence = 4)]
    public HD AssigningAuthority { get; set; }

    [Hl7(Sequence = 5)]
    public string IdentifierTypeCode { get; set; }

    [Hl7(Sequence = 6)]
    public HD AssigningFacility { get; set; }
}
