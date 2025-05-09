using System.ComponentModel;
using System.ServiceModel;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

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
