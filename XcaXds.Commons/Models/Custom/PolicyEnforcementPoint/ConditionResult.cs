namespace XcaXds.Commons.Models.Custom.PolicyEnforcementPoint;

public class ConditionResult
{
    public ConditionResult(string  attributeId, bool matches)
    {
        AttributeId = attributeId;
        Matches = matches;
    }

    public ConditionResult()
    {
    }
    public string AttributeId { get; set; }
    public bool Matches { get; set; }
}