using System.ComponentModel;
using System.ServiceModel;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class RetrieveDocumentSetbResponse
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Xdsb, Order = 0)]
    public RetrieveDocumentSetResponseType RetrieveDocumentSetResponse;

    public RetrieveDocumentSetbResponse()
    {
    }

    public RetrieveDocumentSetbResponse(RetrieveDocumentSetResponseType retrieveDocumentSetResponse)
    {
        RetrieveDocumentSetResponse = retrieveDocumentSetResponse;
    }
}
