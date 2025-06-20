using System.ServiceModel;
using XcaXds.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;


[MessageContract(IsWrapped = false)]
public class RegisterDocumentSetbRequest
{
    [MessageBodyMember(Namespace = Constants.Xds.Namespaces.Lcm, Order = 0)]
    public SubmitObjectsRequest SubmitObjectsRequest { get; set; }

    public RegisterDocumentSetbRequest() { }

    public RegisterDocumentSetbRequest(SubmitObjectsRequest submitObjectsRequest)
    {
        SubmitObjectsRequest = submitObjectsRequest;
    }
}
