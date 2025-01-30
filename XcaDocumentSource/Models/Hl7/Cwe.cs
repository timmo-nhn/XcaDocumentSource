namespace XcaGatewayService.Models.Hl7;

//http://www.hl7.eu/refactored/dtCWE.html
public class Cwe: Hl7Object
{
    [Hl7(Sequence = 1)]
    public string Identifier { get; set; }
    [Hl7(Sequence = 2)]
    public string Text { get; set; }
}
