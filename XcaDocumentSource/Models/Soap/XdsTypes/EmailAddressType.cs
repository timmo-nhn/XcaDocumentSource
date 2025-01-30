using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;


[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class EmailAddressType
{
    [XmlAttribute(AttributeName = "address")]
    public string Address;

    [XmlAttribute(AttributeName = "type")]
    public string Type;
}
