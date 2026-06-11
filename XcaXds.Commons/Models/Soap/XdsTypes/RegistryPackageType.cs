using System.Xml.Serialization;
using XcaXds.Shared.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class RegistryPackageType : RegistryObjectType
{
    [XmlArray]
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
