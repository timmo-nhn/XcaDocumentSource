using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Xdsb)]
public partial class RetrieveDocumentSetResponseType
{
    [XmlElement(Namespace = Constants.Xds.Namespaces.Rs)]
    public RegistryResponseType? RegistryResponse { get; set; }

    [XmlElement("DocumentResponse")]
    public DocumentResponseType[]? DocumentResponse { get; set; }

    public void AddDocument(byte[] document, string home, string repoId, string docId, string? mimeType = null)
    {
        if (document == null || home == null || repoId == null || docId == null)
        {
            return;
        }

        var documentResponse = new DocumentResponseType()
        {
            DocumentUniqueId = docId,
            HomeCommunityId = home,
            RepositoryUniqueId = repoId,
            MimeType = mimeType
        };

        documentResponse.SetInlineDocument(document);

        DocumentResponse ??= [];
        DocumentResponse = [.. DocumentResponse, documentResponse];
    }
}
