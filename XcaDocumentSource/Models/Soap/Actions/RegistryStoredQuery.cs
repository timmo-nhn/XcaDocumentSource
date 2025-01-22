using System.ServiceModel;
using XcaDocumentSource.Models.Soap.Actions;

namespace XcaDocumentSource.Models.Soap.Xds;

// ITI-18
[ServiceContract(Namespace = "urn:ihe:iti:xds-b:2007", ConfigurationName = "DocumentSharing.Xds.IRegistryStoredQuery")]
public interface IRegistryStoredQuery
{
    [OperationContract(Action = "urn:ihe:iti:2007:RegistryStoredQuery", ReplyAction = "urn:ihe:iti:2007:RegistryStoredQueryResponse")]
    [XmlSerializerFormat(SupportFaults = true)]
    [ServiceKnownType(typeof(RegistryResponseType))]
    [ServiceKnownType(typeof(RegistryRequestType))]
    RegistryStoredQueryResponse RegistryStoredQuery(RegistryStoredQueryRequest request);

    [OperationContract(Action = "urn:ihe:iti:2007:RegistryStoredQuery", ReplyAction = "urn:ihe:iti:2007:RegistryStoredQueryResponse")]
    Task<RegistryStoredQueryResponse> RegistryStoredQueryAsync(RegistryStoredQueryRequest request);
}