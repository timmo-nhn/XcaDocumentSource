using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class SlotType
{
    public SlotType(string name, string value)
    {
        Name = name;
        ValueList = new() { Value = [value] };
    }
    public SlotType()
    {
    }

    [XmlElement(Order = 0)]
    public ValueListType? ValueList;

    [XmlAttribute(AttributeName = "name")]
    public string? Name;

    [XmlAttribute(AttributeName = "slotType", DataType = "anyURI")]
    public string? SlotTypeData;

    public string? GetFirstValue(bool trim = true)
    {
        var firstValue = ValueList?.Value?.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(firstValue))
        {
            if (trim)
            {
                firstValue = firstValue.Split("','").FirstOrDefault();

                return firstValue?.Trim().Trim(['(', ')']).Trim('\'');
            }
            return firstValue;
        }
        return null;
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
                if (curVal != null)
                {
                    var multipleValues = curVal.Split("','").ToList();
                    resultList.AddRange(multipleValues);
                    ValueList.Value[i] = ValueList.Value[i];
                }
            }

			// The \u0027 is to handle cases where the string is encoded with single quotes (such as in GetDocumentAssociations with a list of UUIDs)
			/* Example content from AdhocQuery received from XCA (for GetDocumentAssociations): 
			   <ns2:Slot name="$uuid">
					<ns2:ValueList>
						<ns2:Value>(\u0027urn:uuid:0ae98a90-f5ef-4bde-b717-dc0119a5777f\u0027)</ns2:Value>
						<ns2:Value>(\u0027urn:uuid:2690c091-e46f-4c7e-9742-6572cc455355\u0027)</ns2:Value>
					<ns2:ValueList>
				</ns2:Slot>
			*/

			resultList = resultList.Select(val => val.Trim().Trim(['(', ')']).Trim('\'').Replace("\\u0027", "")).ToList();
            return resultList.ToArray();
        }

        return ValueList.Value;
    }

    public void AddValue(string id)
    {
        ValueList ??= new();
        if (id != null)
        {
            ValueList.Value ??= [];
            ValueList.Value = [.. ValueList.Value, id];
        }
    }
}
