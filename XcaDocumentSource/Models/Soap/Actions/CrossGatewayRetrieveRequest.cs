using System.ComponentModel;
using System.ServiceModel;
using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.XdsTypes;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class CrossGatewayRetrieveRequest
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Xdsb, Order = 0)]
    [XmlArrayItem("DocumentRequest", IsNullable = false)]
    public DocumentRequestType[] RetrieveDocumentSetRequest;

    public CrossGatewayRetrieveRequest()
    {
    }

    public CrossGatewayRetrieveRequest(DocumentRequestType[] retrieveDocumentSetRequest)
    {
        RetrieveDocumentSetRequest = retrieveDocumentSetRequest;
    }
}
