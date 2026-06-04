namespace XcaXds.Commons.Models.PolicyEnforcementPoint;

public class AccessControlResult
{
    public bool Permit { get; set; }
    public string PolicyId { get; set; }
    public string[] FailedConditions { get; set; }
    public string Reason { get; set; }
}