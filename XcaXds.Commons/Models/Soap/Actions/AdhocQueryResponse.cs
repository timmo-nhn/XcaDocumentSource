using System.ComponentModel;
using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Query)]
public partial class AdhocQueryResponse : RegistryResponseType
{
    public AdhocQueryResponse()
    {
        StartIndex = "0";
    }

    // https://gazelle.ihe.net/XDStarClient/rim/RegistryObjectListType.html
    [XmlArray(Namespace = Constants.Xds.Namespaces.Rim)]
    [XmlArrayItem("AdhocQuery", typeof(AdhocQueryType), IsNullable = false)]
    [XmlArrayItem("Identifiable", IsNullable = false)]
    [XmlArrayItem("Association", typeof(AssociationType), IsNullable = false)]
    [XmlArrayItem("Classification", typeof(ClassificationType), IsNullable = false)]
    [XmlArrayItem("ExternalIdentifier", typeof(ExternalIdentifierType), IsNullable = false)]
    [XmlArrayItem("ExtrinsicObject", typeof(ExtrinsicObjectType), IsNullable = false)]
    [XmlArrayItem("ObjectRef", typeof(ObjectRefType), IsNullable = false)]
    [XmlArrayItem("RegistryObject", typeof(RegistryObjectType), IsNullable = false)]
    [XmlArrayItem("RegistryPackage", typeof(RegistryPackageType), IsNullable = false)]
    public IdentifiableType[]? RegistryObjectList { get; set; }

    [XmlAttribute(AttributeName = "startIndex", DataType = "integer")]
    [DefaultValue("0")]
    public string StartIndex = "0";

    [XmlAttribute(AttributeName = "totalResultCount", DataType = "integer")]
    public string? TotalResultCount;
}
