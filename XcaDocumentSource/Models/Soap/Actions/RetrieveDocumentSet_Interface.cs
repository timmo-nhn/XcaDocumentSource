using System.ServiceModel;

namespace XcaGatewayService.Models.Soap;

// ITI-43
[ServiceContract(Namespace = "urn:ihe:iti:xds-b:2007", ConfigurationName = "DocumentSharing.Xds.IRetrieveDocumentSet")]
public interface IRetrieveDocumentSet
{
    [OperationContract(Action = "urn:ihe:iti:2007:RetrieveDocumentSet", ReplyAction = "urn:ihe:iti:2007:RetrieveDocumentSetResponse")]
    [XmlSerializerFormat(SupportFaults = true)]
    RetrieveDocumentSetbResponse RetrieveDocumentSet(RetrieveDocumentSetbRequest request);

    [OperationContract(Action = "urn:ihe:iti:2007:RetrieveDocumentSet", ReplyAction = "urn:ihe:iti:2007:RetrieveDocumentSetResponse")]
    Task<RetrieveDocumentSetbResponse> RetrieveDocumentSetAsync(RetrieveDocumentSetbRequest request);
}