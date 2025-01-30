namespace XcaGatewayService.Models.Hl7;

//IHE XDS documentation describes the CE data type, but the CF data type is similar
//http://www.hl7.eu/refactored/dtCF.html
public class Ce: Hl7Object
{
    [Hl7(Sequence = 1)]
    public string Identifier { get; set; }
    [Hl7(Sequence = 2)]
    public string Text { get; set; }
    [Hl7(Sequence = 3)]
    public string CodingSystem { get; set; }
    [Hl7(Sequence = 4)]
    public string AlternateIdentifier { get; set; }
    [Hl7(Sequence = 5)]
    public string AlternateText { get; set; }
    [Hl7(Sequence = 6)]
    public string AlternateCodingSystem { get; set; }
}
