using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class TelephoneNumberType
{
    [XmlAttribute(AttributeName = "areaCode")]
    public string AreaCode;

    [XmlAttribute(AttributeName = "countryCode")]
    public string CountryCode;

    [XmlAttribute(AttributeName = "extension")]
    public string Extension;

    [XmlAttribute(AttributeName = "number")]
    public string Number;

    [XmlAttribute(AttributeName = "phoneType")]
    public string PhoneType;

}
