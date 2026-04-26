using System.ComponentModel.DataAnnotations;

namespace XcaXds.Source.Models.DatabaseDtos.Types;

public class DbAuthorInfo
{
    [StringLength(255)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(255)]
    public string? OrganizationId { get; set; }
    [StringLength(255)]
    public string? OrganizationAssigningAuthority { get; set; }
    [StringLength(255)]
    public string? OrganizationName { get; set; }
    [StringLength(255)]
    public string? DepartmentId { get; set; }
    [StringLength(255)]
    public string? DepartmentAssigningAuthority { get; set; }
    [StringLength(255)]
    public string? DepartmentName { get; set; }
    [StringLength(255)]
    public string? PersonId { get; set; }
    [StringLength(255)]
    public string? PersonAssigningAuthority { get; set; }
    [StringLength(255)]
    public string? PersonFirstName { get; set; }
    [StringLength(255)]
    public string? PersonLastName { get; set; }
    [StringLength(255)]
    public string? RoleCode { get; set; }
    [StringLength(255)]
    public string? RoleCodeSystem { get; set; }
    [StringLength(255)]
    public string? RoleDisplayName { get; set; }
    [StringLength(255)]
    public string? SpecialityCode { get; set; }
    [StringLength(255)]
    public string? SpecialityCodeSystem { get; set; }
    [StringLength(255)]
    public string? SpecialityDisplayName { get; set; }
}
