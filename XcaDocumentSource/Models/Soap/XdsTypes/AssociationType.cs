using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class AssociationType : RegistryObjectType
{
    [XmlAttribute(AttributeName = "associationType", DataType = "anyURI")]
    public string AssociationTypeData;

    [XmlAttribute(AttributeName = "sourceObject", DataType = "anyURI")]
    public string SourceObject;

    [XmlAttribute(AttributeName = "targetObject", DataType = "anyURI")]
    public string TargetObject;
}
