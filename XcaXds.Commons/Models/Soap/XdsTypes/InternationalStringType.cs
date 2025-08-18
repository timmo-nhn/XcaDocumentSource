using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class InternationalStringType
{
    [XmlElement("LocalizedString", Order = 0)]
    public LocalizedStringType[] LocalizedString;
    public string? GetFirstValue()
    {
        return LocalizedString?.FirstOrDefault()?.Value;
    }
}
