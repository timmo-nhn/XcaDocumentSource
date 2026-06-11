using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using XcaXds.Shared.Commons;

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

    [MaxLength(Constants.Properties.MaxArrayLength)]
    [XmlElement("LocalizedString")]
    public LocalizedStringType[]? LocalizedString;
    public string? GetFirstValue()
    {
        return LocalizedString?.FirstOrDefault()?.Value;
    }
}
