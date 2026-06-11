using System.Runtime.Serialization;
using System.Xml.Serialization;
using XcaXds.Shared.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[KnownType(typeof(RegistryPackageType))]
[KnownType(typeof(ExtrinsicObjectType))]
[KnownType(typeof(AssociationType))]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Lcm)]
public class SubmitObjectsRequest : RegistryRequestType
{
    [XmlArray(Namespace = Constants.Xds.Namespaces.Rim)]
    [XmlArrayItem("Identifiable", IsNullable = false)]
    [XmlArrayItem("RegistryObject", typeof(RegistryObjectType), IsNullable = false)]
    [XmlArrayItem("AdhocQuery", typeof(AdhocQueryType), IsNullable = false)]
    [XmlArrayItem("RegistryPackage", typeof(RegistryPackageType), IsNullable = false)]
    [XmlArrayItem("ExtrinsicObject", typeof(ExtrinsicObjectType), IsNullable = false)]
    [XmlArrayItem("Association", typeof(AssociationType), IsNullable = false)]
    [XmlArrayItem("ExternalIdentifier", typeof(ExternalIdentifierType), IsNullable = false)]
    [XmlArrayItem("Classification", typeof(ClassificationType), IsNullable = false)]
    [XmlArrayItem("ObjectRef", typeof(ObjectRefType), IsNullable = false)]
    public IdentifiableType[]? RegistryObjectList { get; set; }
}
