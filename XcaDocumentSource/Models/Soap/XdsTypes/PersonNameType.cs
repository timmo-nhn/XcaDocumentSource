using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class PersonNameType
{
    [XmlAttribute(AttributeName = "firstName")]
    public string FirstName;

    [XmlAttribute(AttributeName = "middleName")]
    public string MiddleName;

    [XmlAttribute(AttributeName = "lastName")]
    public string LastName;
}
