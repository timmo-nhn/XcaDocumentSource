using System.ComponentModel.DataAnnotations.Schema;

namespace XcaXds.Source.Models.DatabaseDtos.Types;

public class DbAuthorOrganization
{
    public string? Id { get; set; }
    public string? OrganizationName { get; set; }
    public string? AssigningAuthority { get; set; }
}