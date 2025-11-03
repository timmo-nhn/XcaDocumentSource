using System.ComponentModel.DataAnnotations.Schema;

namespace XcaXds.Source.Models.DatabaseDtos.Types;

public class DbCodedValue
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Code { get; set; }
    public string? CodeSystem { get; set; }
    public string? DisplayName { get; set; }
}
