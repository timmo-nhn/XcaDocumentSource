using System.ServiceModel;

namespace XcaGatewayService.Models.Soap;

// ITI-42
[ServiceContract(Namespace = "urn:ihe:iti:xds-b:2007", ConfigurationName = "DocumentSharing.Xds.IRegisterDocumentSetb")]
public interface IRegisterDocumentSetb
{
    [OperationContract(Action = "urn:ihe:iti:2007:RegisterDocumentSet-b", ReplyAction = "urn:ihe:iti:2007:RegisterDocumentSet-bResponse")]
    [XmlSerializerFormat(SupportFaults = true)]
    [ServiceKnownType(typeof(RegistryRequestType))]
    RegisterDocumentSetbResponse RegisterDocumentSetb(RegisterDocumentSetbRequest request);

    [OperationContract(Action = "urn:ihe:iti:2007:RegisterDocumentSet-b", ReplyAction = "urn:ihe:iti:2007:RegisterDocumentSet-bResponse")]
    Task<RegisterDocumentSetbResponse> RegisterDocumentSetbAsync(RegisterDocumentSetbRequest request);
}

public interface IRegisterDocumentSetbChannel : IRegisterDocumentSetb, IClientChannel
{
}
