using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

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
