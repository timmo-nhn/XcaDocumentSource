namespace XcaGatewayService.Models.Hl7;

//http://www.hl7.eu/refactored/dtCX.html
public class Cx : Hl7Object
{
    [Hl7(Sequence = 1)]
    public string IdNumber { get; set; }
    [Hl7(Sequence = 2)]
    public string IdentifierCheckDigit { get; set; }
    [Hl7(Sequence = 3)]
    public string CheckDigitScheme { get; set; }
    [Hl7(Sequence = 4)]
    public Hd AssigningAuthority { get; set; }
    [Hl7(Sequence = 5)]
    public string IdentifierTypeCode { get; set; }
    [Hl7(Sequence = 6)]
    public Hd AssigningFacility { get; set; }
}
