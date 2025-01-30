using System.Xml.Serialization;

namespace XcaXds.Commons.Models.Soap.XdsTypes;


[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class EmailAddressType
{
    [XmlAttribute(AttributeName = "address")]
    public string Address;

    [XmlAttribute(AttributeName = "type")]
    public string Type;
}
