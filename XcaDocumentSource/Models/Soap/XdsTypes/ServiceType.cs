using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class ServiceType : RegistryObjectType
{
    [XmlElement("ServiceBinding", Order = 0)]
    public ServiceBindingType[] ServiceBinding;
}
