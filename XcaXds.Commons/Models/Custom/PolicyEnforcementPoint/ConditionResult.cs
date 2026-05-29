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
    public bool Matches { get; set; }
    public string AttributeId { get; set; }
    public string RelatedPolicyId { get; set; }
}