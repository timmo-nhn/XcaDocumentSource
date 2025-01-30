using System.ComponentModel;
using System.ServiceModel;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public partial class RegistryStoredQueryRequest
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Query, Order = 0)]
    public AdhocQueryRequest AdhocQueryRequest;

    public RegistryStoredQueryRequest()
    {
    }

    public RegistryStoredQueryRequest(AdhocQueryRequest adhocQueryRequest)
    {
        AdhocQueryRequest = adhocQueryRequest;
    }
}
