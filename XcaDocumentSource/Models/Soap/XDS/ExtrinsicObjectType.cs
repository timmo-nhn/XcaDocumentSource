using System.ComponentModel;
using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class ExtrinsicObjectType : RegistryObjectType
{
    public ExtrinsicObjectType()
    {
        MimeType = Constants.MimeTypes.Binary;
        IsOpaque = false;
    }

    [XmlElement(Order = 0)]
    public VersionInfoType ContentVersionInfo;

    [XmlElement(Namespace = Constants.Xds.Namespaces.Xdsb, DataType = "base64Binary", Order = 1)]
    public byte[] Document;

    [XmlAttribute(AttributeName = "mimeType")]
    [DefaultValue(Constants.MimeTypes.Binary)]
    public string MimeType;

    [XmlAttribute(AttributeName = "isOpaque")]
    [DefaultValue(false)]
    public bool IsOpaque;
}
