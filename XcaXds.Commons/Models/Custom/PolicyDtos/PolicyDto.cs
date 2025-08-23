using Abc.Xacml.Policy;
using System.Reflection.Metadata;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Custom.PolicyDtos;

public class PolicyDto
{
    public string? Id { get; set; }
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

        if (Subjects != null && Subjects.Count != 0)
        {
            foreach (var subject in Subjects)
            {
                subject.DataType ??= Constants.Xacml.DataType.String;
                subject.AttributeId ??= Constants.Xacml.Attribute.SubjectId;
                subject.MatchId ??= Constants.Xacml.Functions.StringEqual;
            }
        }

        if (Roles != null && Roles.Count != 0)
        {
            foreach (var role in Roles)
            {
                role.DataType ??= Constants.Xacml.DataType.String;
                role.AttributeId ??= Constants.Xacml.Attribute.Role;
                role.MatchId ??= Constants.Xacml.Functions.StringEqual;
            }
        }

        if (Organizations != null && Organizations.Count != 0)
        {
            foreach (var organization in Organizations)
            {
                organization.DataType ??= Constants.Xacml.DataType.String;
                organization.AttributeId ??= "urn:oasis:names:tc:xspa:1.0:subject:organization:code";
                organization.MatchId ??= Constants.Xacml.Functions.StringEqual;
            }
        }

        if (Resources != null && Resources.Count != 0)
        {
            foreach (var resource in Resources)
            {
                resource.DataType ??= Constants.Xacml.DataType.String;
                resource.AttributeId ??= Constants.Xacml.Attribute.ResourceId;
                resource.MatchId ??= Constants.Xacml.Functions.StringEqual;
            }
        }
    }
}