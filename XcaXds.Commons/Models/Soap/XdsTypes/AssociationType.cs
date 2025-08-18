using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class AssociationType : RegistryObjectType
{
    [XmlAttribute(AttributeName = "associationType", DataType = "anyURI")]
    public string AssociationTypeData;

    /// <summary>
    /// Usually the RegistryPackage/Submissionset/Folder
    /// </summary>
    [XmlAttribute(AttributeName = "sourceObject", DataType = "anyURI")]
    public string SourceObject;

    /// <summary>
    /// Usually the ExtrinsicObject/DocumentEntry
    /// </summary>
    [XmlAttribute(AttributeName = "targetObject", DataType = "anyURI")]
    public string TargetObject;
}
