using System.ComponentModel.DataAnnotations.Schema;

namespace XcaXds.Source.Models.DatabaseDtos.Types;

public class DbAuthorInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? OrganizationId { get; set; }
    public string? OrganizationAssigningAuthority { get; set; }
    public string? OrganizationName { get; set; }
    public string? DepartmentId { get; set; }
    public string? DepartmentAssigningAuthority { get; set; }
    public string? DepartmentName { get; set; }
    public string? PersonId { get; set; }
    public string? PersonAssigningAuthority { get; set; }
    public string? PersonFirstName { get; set; }
    public string? PersonLastName { get; set; }
    public string? RoleCode { get; set; }
    public string? RoleCodeSystem { get; set; }
    public string? RoleDisplayName { get; set; }
    public string? SpecialityCode { get; set; }
    public string? SpecialityCodeSystem { get; set; }
    public string? SpecialityDisplayName { get; set; }
}
