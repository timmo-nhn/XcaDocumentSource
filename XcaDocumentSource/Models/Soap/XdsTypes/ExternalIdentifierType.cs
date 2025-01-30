using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class ExternalIdentifierType : RegistryObjectType
{
    [XmlAttribute(AttributeName = "registryObject", DataType = "anyURI")]
    public string RegistryObject;

    [XmlAttribute(AttributeName = "identificationScheme", DataType = "anyURI")]
    public string IdentificationScheme;

    [XmlAttribute(AttributeName = "value")]
    public string Value;
}
