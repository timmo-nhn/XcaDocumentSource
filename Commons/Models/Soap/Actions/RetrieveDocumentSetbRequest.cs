using System.ComponentModel;
using System.ServiceModel;
using System.Xml.Serialization;
using XcaXds.Commons.Models.Soap.XdsTypes;


namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "urn:ihe:iti:xds-b:2007")]
[MessageContract(IsWrapped = false)]
public partial class RetrieveDocumentSetbRequest
{
    [XmlElement]
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Xdsb, Order = 0)]
    public DocumentRequestType[] DocumentRequest;

    public RetrieveDocumentSetbRequest()
    {
    }

    public RetrieveDocumentSetbRequest(DocumentRequestType[] retrieveDocumentSetRequest)
    {
        DocumentRequest = retrieveDocumentSetRequest;
    }
}
