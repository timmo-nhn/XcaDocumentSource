using System.ComponentModel;
using System.ServiceModel;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

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
