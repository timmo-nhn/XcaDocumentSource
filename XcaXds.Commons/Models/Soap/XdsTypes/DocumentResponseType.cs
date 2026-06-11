using System.Xml;
using System.Xml.Serialization;
using XcaXds.Commons.Serializers;
using XcaXds.Shared.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Xdsb)]
public partial class DocumentResponseType
{
    [XmlElement]
    public string? HomeCommunityId { get; set; }

    [XmlElement]
    public string? RepositoryUniqueId { get; set; }

    [XmlElement]
    public string? DocumentUniqueId { get; set; }

    [XmlElement(ElementName = "mimeType")]
    public string? MimeType { get; set; }

    [XmlAnyElement("Document")]
    public XmlElement? Document { get; set; }

    /// <summary>
    /// Sets the document as inline base64 content
    /// </summary>
    public void SetInlineDocument(byte[] data)
    {
        var xmlDoc = new XmlDocument();
        var docElement = xmlDoc.CreateElement("Document", "urn:ihe:iti:xds-b:2007");
        docElement.InnerText = Convert.ToBase64String(data);
        Document = docElement;
    }

    /// <summary>
    /// Sets the document as an XOP Include element
    /// </summary>
    public void SetXopInclude(string href)
    {
        var xmlDoc = new XmlDocument();
        var docElement = xmlDoc.CreateElement("Document", "urn:ihe:iti:xds-b:2007");

        var include = xmlDoc.CreateElement("xop", "Include", "http://www.w3.org/2004/08/xop/include");
        include.SetAttribute("href", href);

        docElement.AppendChild(include);
        Document = docElement;
    }

    public IncludeType GetXmlDocumentAsXopInclude()
    {
        return new SoapXmlSerializer().DeserializeXmlString<IncludeType>(Document?.InnerXml ?? string.Empty);
    }
}