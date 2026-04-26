using System.ComponentModel.DataAnnotations;

namespace XcaXds.Source.Models.DatabaseDtos.Types;

public class DbLegalAuthenticator
{
    [StringLength(255)]
    public string? Id { get; set; }
    [StringLength(255)]
    public string? IdSystem { get; set; }
    [StringLength(255)]
    public string? FirstName { get; set; }
    [StringLength(255)]
    public string? LastName { get; set; }
}