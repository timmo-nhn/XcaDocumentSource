using System.ComponentModel;
using System.ServiceModel;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class CrossGatewayQueryResponse
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Query, Order = 0)]
    public AdhocQueryResponse AdhocQueryResponse;

    public CrossGatewayQueryResponse()
    {
    }

    public CrossGatewayQueryResponse(AdhocQueryResponse adhocQueryResponse)
    {
        AdhocQueryResponse = adhocQueryResponse;
    }
}
