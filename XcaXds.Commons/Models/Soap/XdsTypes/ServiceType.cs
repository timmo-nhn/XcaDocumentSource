using System.Xml.Serialization;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class ServiceType : RegistryObjectType
{
    [XmlElement("ServiceBinding", Order = 0)]
    public ServiceBindingType[] ServiceBinding;
}
