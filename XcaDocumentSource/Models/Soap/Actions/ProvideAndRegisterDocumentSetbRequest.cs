using System.ComponentModel;
using System.ServiceModel;

namespace XcaDocumentSource.Models.Soap.XdsTypes;

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
