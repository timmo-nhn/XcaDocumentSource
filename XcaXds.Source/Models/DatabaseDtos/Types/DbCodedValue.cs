using System.ComponentModel.DataAnnotations;

namespace XcaXds.Source.Models.DatabaseDtos.Types;

public class DbCodedValue
{
    [StringLength(255)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(255)]
    public string? Code { get; set; }
    [StringLength(255)]
    public string? CodeSystem { get; set; }
    [StringLength(255)]
    public string? DisplayName { get; set; }
}
