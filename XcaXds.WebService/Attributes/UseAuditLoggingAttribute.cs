namespace XcaXds.WebService.Attributes;

public class UseAuditLoggingAttribute : Attribute
{
    public bool Enabled { get; set; } = true;
}
