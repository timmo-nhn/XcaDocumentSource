using System.Xml;
using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Serializers;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Xdsb)]
public partial class DocumentResponseType
{
    [XmlElement(Order = 0)]
    public string? HomeCommunityId { get; set; }

    [XmlElement(Order = 1)]
    public string? RepositoryUniqueId { get; set; }

    [XmlElement(Order = 2)]
    public string? DocumentUniqueId { get; set; }

    [XmlElement(ElementName = "mimeType", Order = 3)]
    public string? MimeType { get; set; }

    [XmlAnyElement("Document", Order = 4)]
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