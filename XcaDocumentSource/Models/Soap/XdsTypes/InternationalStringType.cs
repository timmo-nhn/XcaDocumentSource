using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class InternationalStringType
{
    [XmlElement("LocalizedString", Order = 0)]
    public LocalizedStringType[] LocalizedString;
    public string? GetFirstValue()
    {
        return LocalizedString.FirstOrDefault()?.Value;
    }


}
