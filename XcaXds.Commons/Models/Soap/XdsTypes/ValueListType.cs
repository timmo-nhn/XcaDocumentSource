using System.Xml.Serialization;
using XcaXds.Shared;
using XcaXds.WebService.Attributes;
namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class ValueListType
{
    [XmlElement("Value")]
    [StringArrayConstraints(Constants.Properties.MaxArrayLength, Constants.Properties.MaxStringLength)]
    public string[]? Value { get; set; }

    public string[]? AddValue(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Value ??= [];
            Value = Value.Append(value).ToArray();
        }
        return Value;
    }
}