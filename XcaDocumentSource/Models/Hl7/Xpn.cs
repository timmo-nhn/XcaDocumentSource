namespace XcaGatewayService.Models.Hl7;

//http://www.hl7.eu/refactored/dtXPN.html
//http://www.hl7.eu/refactored/segPID.html
public class Xpn : Hl7Object
{
    [Hl7(Sequence = 1)]
    public string FamilyName { get; set; }
    [Hl7(Sequence = 2)]
    public string GivenName { get; set; }
    [Hl7(Sequence = 3)]
    public string FurtherGivenNames { get; set; }
    [Hl7(Sequence = 4)]
    public string Suffix { get; set; }
    [Hl7(Sequence = 5)]
    public string Prefix { get; set; }
    [Hl7(Sequence = 6)]
    public string Degree { get; set; }
}
