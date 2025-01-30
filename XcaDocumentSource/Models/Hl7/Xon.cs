namespace XcaGatewayService.Models.Hl7;

//http://www.hl7.eu/refactored/dtXON.html
public class Xon : Hl7Object
{
    [Hl7(Sequence = 1)]
    public string OrganizationName { get; set; }
    [Hl7(Sequence = 2)]
    public string OrganizationNameTypeCode { get; set; } //Type CWE - Not implemented
    [Hl7(Sequence = 3)]
    public string IdNumber { get; set; }
    [Hl7(Sequence = 4)]
    public string IdentifierCheckDigit { get; set; }
    [Hl7(Sequence = 5)]
    public string CheckDigitScheme { get; set; }
    [Hl7(Sequence = 6)]
    public Hd AssigningAuthority { get; set; }
    [Hl7(Sequence = 7)]
    public string IdentifierTypeCode { get; set; }
    [Hl7(Sequence = 8)]
    public Hd AssigningFacility { get; set; }
    [Hl7(Sequence = 9)]
    public string NameRepresentationCode { get; set; }
    [Hl7(Sequence = 10)]
    public string OrganizationIdentifier { get; set; }
}
