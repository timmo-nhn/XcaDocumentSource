using System.Text.Json.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Custom.PolicyDtos;

public class AbacCondition
{
    public AbacCondition(string attributeId, AttributeCompareRule compareRule, string value)
    {
        AttributeId = attributeId;
        CompareAttributes = true;
        CompareRule = compareRule;
        Value = value;
    }

    public AbacCondition(string attributeId, string value)
    {
        AttributeId = attributeId;
        Value = value;
    }

    public AbacCondition()
    {
    }

    public string? AttributeId { get; set; }
    public bool? CompareAttributes { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AttributeCompareRule CompareRule { get; set; }

    public string? Value { get; set; }
}