using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class ExternalLinkType : RegistryObjectType
{
    [XmlAttribute(AttributeName = "externalURI", DataType = "anyURI")]
    public string ExternalUri;
}
