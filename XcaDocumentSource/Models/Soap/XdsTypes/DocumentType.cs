using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.XdsTypes;

[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Xdsb)]
public class DocumentType
{

    [XmlAttribute(AttributeName = "id", DataType = "anyURI")]
    public string Id;

    [XmlText(DataType = "base64Binary")]
    public byte[] Value;
}
