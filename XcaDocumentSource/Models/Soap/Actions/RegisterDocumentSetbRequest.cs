using System.ComponentModel;
using System.ServiceModel;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public partial class RegisterDocumentSetbRequest
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Xdsb, Order = 0)]
    public RegisterDocumentSetRequestType RegisterDocumentSetRequest;

    public RegisterDocumentSetbRequest()
    {
    }

    public RegisterDocumentSetbRequest(RegisterDocumentSetRequestType RegisterDocumentSetRequest)
    {
        RegisterDocumentSetRequest = RegisterDocumentSetRequest;
    }
}
