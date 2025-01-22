using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Xdsb)]
public partial class RetrieveDocumentSetResponseType
{
    [XmlElement(Namespace = Constants.Xds.Namespaces.Rs, Order = 0)]
    public RegistryResponseType RegistryResponse;

    [XmlElement("DocumentResponse", Order = 1)]
    public DocumentResponseType[] DocumentResponse;
}
