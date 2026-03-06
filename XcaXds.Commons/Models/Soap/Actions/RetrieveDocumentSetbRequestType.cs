using System.Xml.Serialization;
using XcaXds.Commons.Models.Soap.XdsTypes;


namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable()]
[XmlType(AnonymousType = true, Namespace = "urn:ihe:iti:xds-b:2007")]
public partial class RetrieveDocumentSetbRequestType
{
    [XmlElement]
    public DocumentRequestType[]? DocumentRequest { get; set; }

    public RetrieveDocumentSetbRequestType()
    {
    }

    public void AddDocumentRequest(DocumentRequestType documentRequest)
    {
        DocumentRequest ??= [];
        DocumentRequest = DocumentRequest.Append(
            new DocumentRequestType
            {
                HomeCommunityId = documentRequest.HomeCommunityId,
                DocumentUniqueId = documentRequest.DocumentUniqueId,
                RepositoryUniqueId = documentRequest.RepositoryUniqueId,
            }).ToArray();
    }

    public RetrieveDocumentSetbRequestType(DocumentRequestType[] retrieveDocumentSetRequest)
    {
        DocumentRequest = retrieveDocumentSetRequest;
    }
}
