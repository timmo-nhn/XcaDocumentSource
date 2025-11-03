using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace XcaXds.Source.Models.DatabaseDtos;

[Index(nameof(Id), IsUnique = true)]
public class DbRegistryObject
{
    [Key]
    public string? Id { get; set; } = Guid.NewGuid().ToString();
}
