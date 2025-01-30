namespace XcaGatewayService.Models.Hl7;

//http://www.hl7.eu/refactored/dtXAD.html
public class Xad : Hl7Object
{
    [Hl7(Sequence = 1)]
    public Sad StreetAddress { get; set; }
    [Hl7(Sequence = 2)]
    public string OtherDesignation { get; set; }
    [Hl7(Sequence = 3)]
    public string City { get; set; }
    [Hl7(Sequence = 4)]
    public string StateOrProvince { get; set; }
    [Hl7(Sequence = 5)]
    public string ZipOrPostalCode { get; set; }
    [Hl7(Sequence = 6)]
    public string Country { get; set; }
}
