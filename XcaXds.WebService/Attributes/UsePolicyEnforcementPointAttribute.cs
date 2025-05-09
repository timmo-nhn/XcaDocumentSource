namespace XcaXds.WebService.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class UsePolicyEnforcementPointAttribute : Attribute
{
    public bool Enabled { get; set; } = true;
}
