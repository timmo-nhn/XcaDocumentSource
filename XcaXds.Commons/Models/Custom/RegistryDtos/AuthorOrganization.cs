using System.ComponentModel.DataAnnotations.Schema;

namespace XcaXds.Commons.Models.Custom.RegistryDtos;

[ComplexType]
public class AuthorOrganization
{
    public string? Id { get; set; }
    public string? OrganizationName { get; set; }
    public string? AssigningAuthority { get; set; }
}