using System.ComponentModel;
using System.ServiceModel;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class RegisterDocumentSetbResponse
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Rs, Order = 0)]
    public RegistryResponseType RegistryResponse;

    public RegisterDocumentSetbResponse()
    {
    }

    public RegisterDocumentSetbResponse(RegistryResponseType registryResponse)
    {
        RegistryResponse = registryResponse;
    }
}
