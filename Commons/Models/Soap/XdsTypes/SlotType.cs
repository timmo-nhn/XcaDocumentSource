using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class SlotType
{
    [XmlElement(Order = 0)]
    public ValueListType ValueList;

    [XmlAttribute(AttributeName = "name")]
    public string Name;

    [XmlAttribute(AttributeName = "slotType", DataType = "anyURI")]
    public string SlotTypeData;

    public string? GetFirstValue(bool sanitize = true)
    {
        var firstValue = ValueList?.Value?.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(firstValue))
        {
            if (sanitize)
            {
                return firstValue.Trim().Trim(['(', ')']).Trim('\'');
            }
            return firstValue;
        }
        return string.Empty;
    }

    public string[]? GetValues(bool codeMultipleValues = true)
    {
        if (ValueList?.Value == null)
        {
            return [];
        }

        // https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html#3.18.4.1.2.3.5
        if (codeMultipleValues)
        {
            var resultList = new List<string>();

            for (int i = 0; i < ValueList.Value.Length; i++)
            {
                var curVal = ValueList.Value[i];
                var multipleValues = curVal.Split("','").ToList();
                resultList = [.. resultList, .. multipleValues];
                ValueList.Value[i] = ValueList.Value[i];
            }
            resultList = resultList.Select(val => val.Trim().Trim(['(', ')']).Trim('\'')).ToList();
            return resultList.ToArray();
        }

        return ValueList.Value;
    }
}
