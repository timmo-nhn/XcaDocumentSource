using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Custom.PolicyDtos;

public class PolicyDto
{
    public string? Id { get; set; }
    public List<PolicyMatch>? Rules { get; set; }
    public List<PolicyMatch>? Subjects { get; set; }
    public List<PolicyMatch>? Roles { get; set; }
    public List<PolicyMatch>? Organizations { get; set; }
    public List<PolicyMatch>? Resources { get; set; }
    public List<string>? Actions { get; set; }
    public string? Effect { get; set; }

    public void SetDefaultValues()
    {
        if (Subjects != null)
        {
            foreach (var item in Subjects)
            {
                item.AttributeId ??= Constants.Xacml.Attribute.SubjectId;
            }
        }

        if (Roles != null)
        {
            foreach (var item in Roles)
            {
                item.AttributeId ??= Constants.Xacml.Attribute.Role;
            }
        }

        if (Resources != null)
        {
            foreach (var item in Resources)
            {
                item.AttributeId ??= Constants.Xacml.Attribute.ResourceId;
            }
        }
    }
}