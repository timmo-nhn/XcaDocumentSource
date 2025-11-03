using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XcaXds.Commons.Models.Custom.RegistryDtos;

[ComplexType]
public class CodedValue
{
    [Key]
    public string? Code { get; set; }
    public string? CodeSystem { get; set; }
    public string? DisplayName { get; set; }
}