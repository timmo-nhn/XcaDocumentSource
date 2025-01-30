using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Xdsb)]
public partial class DocumentResponseType
{
    [XmlElement(Order = 0)]
    public string HomeCommunityId;

    [XmlElement(Order = 1)]
    public string RepositoryUniqueId;

    [XmlElement(Order = 2)]
    public string DocumentUniqueId;

    [XmlElement(Order = 3)]
    public string NewRepositoryUniqueId;

    [XmlElement(Order = 4)]
    public string NewDocumentUniqueId;

    [XmlElement(ElementName = "mimeType", Order = 5)]
    public string MimeType;

    [XmlElement(DataType = "base64Binary", Order = 6)]
    public byte[] Document;
}
