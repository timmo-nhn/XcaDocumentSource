using System.ServiceModel;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

// ITI-38
[ServiceContract(Namespace = "urn:ihe:iti:xds-b:2007", ConfigurationName = "DocumentSharing.Xds.ICrossGatewayQuery")]
public interface ICrossGatewayQuery
{
    [OperationContract(Action = "urn:ihe:iti:2007:CrossGatewayQuery", ReplyAction = "urn:ihe:iti:2007:CrossGatewayQueryResponse")]
    [XmlSerializerFormat(SupportFaults = true)]
    [ServiceKnownType(typeof(RegistryResponseType))]
    [ServiceKnownType(typeof(RegistryRequestType))]
    CrossGatewayQueryResponse CrossGatewayQuery(CrossGatewayQueryRequest request);

    [OperationContract(Action = "urn:ihe:iti:2007:CrossGatewayQuery", ReplyAction = "urn:ihe:iti:2007:CrossGatewayQueryResponse")]
    Task<CrossGatewayQueryResponse> CrossGatewayQueryAsync(CrossGatewayQueryRequest request);
}