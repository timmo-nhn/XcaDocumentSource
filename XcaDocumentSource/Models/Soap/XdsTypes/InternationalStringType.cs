using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class InternationalStringType
{
    [XmlElement("LocalizedString", Order = 0)]
    public LocalizedStringType[] LocalizedString;
}
