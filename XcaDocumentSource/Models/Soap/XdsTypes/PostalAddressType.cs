using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class PostalAddressType
{
    [XmlAttribute(AttributeName = "city")]
    public string City;

    [XmlAttribute(AttributeName = "country")]
    public string Country;

    [XmlAttribute(AttributeName = "postalCode")]
    public string PostalCode;

    [XmlAttribute(AttributeName = "stateOrProvince")]
    public string StateOrProvince;

    [XmlAttribute(AttributeName = "street")]
    public string Street;

    [XmlAttribute(AttributeName = "streetNumber")]
    public string StreetNumber;
}
