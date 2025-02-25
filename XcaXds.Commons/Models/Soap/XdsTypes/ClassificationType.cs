using System.Xml.Serialization;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class ClassificationType : RegistryObjectType
{
    [XmlAttribute(AttributeName = "classificationScheme", DataType = "anyURI")]
    public string ClassificationScheme;

    [XmlAttribute(AttributeName = "classifiedObject", DataType = "anyURI")]
    public string ClassifiedObject;

    [XmlAttribute(AttributeName = "classificationNode", DataType = "anyURI")]
    public string ClassificationNode;

    [XmlAttribute(AttributeName = "nodeRepresentation")]
    public string NodeRepresentation;
}
