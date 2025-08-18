using System.ComponentModel;
using System.ServiceModel;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

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
