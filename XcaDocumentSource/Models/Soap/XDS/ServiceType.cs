using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class ServiceType : RegistryObjectType
{
    [XmlElement("ServiceBinding", Order = 0)]
    public ServiceBindingType[] ServiceBinding;
}
