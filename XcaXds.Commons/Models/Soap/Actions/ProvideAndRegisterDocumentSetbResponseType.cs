using System.Xml.Serialization;

namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable()]
[XmlType(AnonymousType = true, Namespace = "urn:ihe:iti:xds-b:2007")]
public class ProvideAndRegisterDocumentSetbResponseType
{
    [XmlElement]
    public RegistryResponseType? RegistryResponse { get; set; }

    public ProvideAndRegisterDocumentSetbResponseType()
    {
    }

    public ProvideAndRegisterDocumentSetbResponseType(RegistryResponseType registryResponse)
    {
        RegistryResponse = registryResponse;
    }
}
