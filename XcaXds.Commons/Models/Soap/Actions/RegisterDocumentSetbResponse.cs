using System.ServiceModel;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

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
