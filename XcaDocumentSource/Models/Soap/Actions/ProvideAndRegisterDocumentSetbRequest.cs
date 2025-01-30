using System.ComponentModel;
using System.ServiceModel;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public partial class ProvideAndRegisterDocumentSetbRequest
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Xdsb, Order = 0)]
    public ProvideAndRegisterDocumentSetRequestType ProvideAndRegisterDocumentSetRequest;

    public ProvideAndRegisterDocumentSetbRequest()
    {
    }

    public ProvideAndRegisterDocumentSetbRequest(ProvideAndRegisterDocumentSetRequestType provideAndRegisterDocumentSetRequest)
    {
        ProvideAndRegisterDocumentSetRequest = provideAndRegisterDocumentSetRequest;
    }
}
