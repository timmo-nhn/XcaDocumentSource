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
        if (Id == "string" || string.IsNullOrWhiteSpace(Id))
        {
            Id = Guid.NewGuid().ToString();
        }

        SetDefaultValuesProperties(Rules);
        SetDefaultValuesProperties(Subjects);
        SetDefaultValuesProperties(Roles);
        SetDefaultValuesProperties(Organizations);
        SetDefaultValuesProperties(Resources);
    }

    private void SetDefaultValuesProperties(List<PolicyMatch>? items)
    {
        if (items == null) return;

        foreach (var item in items)
        {
            item.DataType ??= Constants.Xacml.DataType.String;
            item.AttributeId ??= Constants.Xacml.Attribute.SubjectId;
            item.MatchId ??= Constants.Xacml.Functions.StringEqual;
        }
    }
}