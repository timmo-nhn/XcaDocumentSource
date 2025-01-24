using System.ComponentModel;
using System.ServiceModel;

namespace XcaDocumentSource.Models.Soap.XdsTypes;

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
