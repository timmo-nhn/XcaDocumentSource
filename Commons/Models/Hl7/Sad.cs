namespace XcaXds.Commons.Models.Hl7;

//http://www.hl7.eu/refactored/dtSAD.html
public class Sad : Hl7Object
{
    [Hl7(Sequence = 1)]
    public string StreetOrMailingAddress { get; set; }
    [Hl7(Sequence = 2)]
    public string StreetName { get; set; }
    [Hl7(Sequence = 3)]
    public string HouseNumber { get; set; }
}
