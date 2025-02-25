using System.ServiceModel;
using XcaXds.Commons.Models.Soap.XdsTypes;

[MessageContract(IsWrapped = false)]  
public class RegisterDocumentSetbRequest
{
    [MessageBodyMember(Namespace = "urn:oasis:names:tc:ebxml-regrep:xsd:lcm:3.0", Order = 0)]
    public SubmitObjectsRequest SubmitObjectsRequest { get; set; }

    public RegisterDocumentSetbRequest() { }

    public RegisterDocumentSetbRequest(SubmitObjectsRequest submitObjectsRequest)
    {
        SubmitObjectsRequest = submitObjectsRequest;
    }
}
