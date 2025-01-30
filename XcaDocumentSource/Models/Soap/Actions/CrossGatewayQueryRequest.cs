using System.ComponentModel;
using System.ServiceModel;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class CrossGatewayQueryRequest
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Query, Order = 0)]
    public AdhocQueryRequest AdhocQueryRequest;

    public CrossGatewayQueryRequest()
    {
    }

    public CrossGatewayQueryRequest(AdhocQueryRequest adhocQueryRequest)
    {
        AdhocQueryRequest = adhocQueryRequest;
    }
}
