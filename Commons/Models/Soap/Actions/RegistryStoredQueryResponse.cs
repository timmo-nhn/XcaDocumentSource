using System.ComponentModel;
using System.ServiceModel;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class RegistryStoredQueryResponse
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Query, Order = 0)]
    public AdhocQueryResponse AdhocQueryResponse;

    public RegistryStoredQueryResponse()
    {
    }

    public RegistryStoredQueryResponse(AdhocQueryResponse adhocQueryResponse)
    {
        AdhocQueryResponse = adhocQueryResponse;
    }
}
