using System.ComponentModel;
using System.ServiceModel;
using System.Xml.Serialization;
using XcaGatewayService.Commons;


namespace XcaGatewayService.Models.Soap;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public partial class RetrieveDocumentSetbRequest
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Xdsb, Order = 0)]
    [XmlArrayItem("DocumentRequest", IsNullable = false)]
    public DocumentRequestType[] RetrieveDocumentSetRequest;

    public RetrieveDocumentSetbRequest()
    {
    }

    public RetrieveDocumentSetbRequest(DocumentRequestType[] retrieveDocumentSetRequest)
    {
        RetrieveDocumentSetRequest = retrieveDocumentSetRequest;
    }
}
