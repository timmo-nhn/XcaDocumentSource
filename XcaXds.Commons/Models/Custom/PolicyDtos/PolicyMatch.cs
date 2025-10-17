using System.Text.Json.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Custom.PolicyDtos;

public class PolicyMatch
{
    public PolicyMatch(string attributeId, bool? compareAttributes, string value)
    {
        AttributeId = attributeId;
        CompareAttributes = compareAttributes;
        Value = value;
    }

    public PolicyMatch()
    { }

    public string? AttributeId { get; set; }
    public bool? CompareAttributes { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CompareRule CompareRule { get; set; }
    public string? Value { get; set; }
}