using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class ValueListType
{
    [XmlElement("Value", Order = 0)]
    public string[] Value;

    public string[] AddValue(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Value ??= [];
            Value = Value.Append(value).ToArray();
        }
        return Value;
    }

}
