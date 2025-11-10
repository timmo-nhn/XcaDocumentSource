using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Custom;

[XmlRoot("Registry")]
public class XmlDocumentRegistry
{
    [XmlElement("RegistryPackage", typeof(RegistryPackageType), Namespace = Constants.Soap.Namespaces.Rim)]
    [XmlElement("ExtrinsicObject", typeof(ExtrinsicObjectType), Namespace = Constants.Soap.Namespaces.Rim)]
    [XmlElement("Association", typeof(AssociationType), Namespace = Constants.Soap.Namespaces.Rim)]
    public IEnumerable<IdentifiableType> RegistryObjectList { get; set; }

    public XmlDocumentRegistry()
    {
        RegistryObjectList = [];
    }
}
