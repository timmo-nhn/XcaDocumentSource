namespace XcaXds.WebService.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ExportsAtnaAuditLogAttribute : Attribute
{
    public bool Enabled { get; set; } = true;
}
