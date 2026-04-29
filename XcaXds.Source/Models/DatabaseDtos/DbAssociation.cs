using System.ComponentModel.DataAnnotations;

namespace XcaXds.Source.Models.DatabaseDtos;

public class DbAssociation : DbRegistryObject
{
    [MaxLength(255)]
    public string? AS_AssociationType { get; set; }
    [MaxLength(255)]
    public string? AS_SubmissionSetStatus { get; set; }

    [MaxLength(255)]
    public string? AS_SourceObjectId { get; set; }
    [MaxLength(255)]
    public string? AS_TargetObjectId { get; set; }
}
