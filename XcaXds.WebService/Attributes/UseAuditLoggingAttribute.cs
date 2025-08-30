namespace XcaXds.WebService.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class UseAuditLoggingAttribute : Attribute
{
    public bool Enabled { get; set; } = true;
}
