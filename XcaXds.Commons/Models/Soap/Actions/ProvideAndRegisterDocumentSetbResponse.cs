using System.ComponentModel;
using System.ServiceModel;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class ProvideAndRegisterDocumentSetbResponse
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Rs, Order = 0)]
    public RegistryResponseType RegistryResponse;

    public ProvideAndRegisterDocumentSetbResponse()
    {
    }

    public ProvideAndRegisterDocumentSetbResponse(RegistryResponseType registryResponse)
    {
        RegistryResponse = registryResponse;
    }
}
