using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class InternationalStringType
{
    public InternationalStringType()
    {
        
    }
    public InternationalStringType(string input)
    {
        LocalizedString = [new() { Value = input }];
    }

    [XmlElement("LocalizedString", Order = 0)]
    public LocalizedStringType[] LocalizedString;
    public string? GetFirstValue()
    {
        return LocalizedString?.FirstOrDefault()?.Value;
    }
}
