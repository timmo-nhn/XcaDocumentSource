using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class AssociationType : RegistryObjectType
{
    [XmlAttribute(AttributeName = "associationType", DataType = "anyURI")]
    public string? AssociationTypeData { get; set; } = Constants.Xds.AssociationType.HasMember;

    /// <summary>
    /// Usually the RegistryPackage/Submissionset/Folder or existing RegistryObject
    /// </summary>
    [XmlAttribute(AttributeName = "sourceObject", DataType = "anyURI")]
    public string? SourceObject { get; set; }

    /// <summary>
    /// Usually the ExtrinsicObject/DocumentEntry or RegistryObject that will replace the existing one (SourceObject)
    /// </summary>
    [XmlAttribute(AttributeName = "targetObject", DataType = "anyURI")]
    public string? TargetObject { get; set; }
}
