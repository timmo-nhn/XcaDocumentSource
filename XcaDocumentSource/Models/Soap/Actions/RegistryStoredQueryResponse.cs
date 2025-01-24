using System.ComponentModel;
using System.ServiceModel;
using XcaDocumentSource.Models.Soap.XdsTypes;

namespace XcaDocumentSource.Models.Soap.Actions;

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
