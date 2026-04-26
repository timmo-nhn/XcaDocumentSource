using System.ComponentModel.DataAnnotations;

namespace XcaXds.Source.Models.DatabaseDtos;

public class DbAssociation : DbRegistryObject
{
    [StringLength(255)]
    public string? AS_AssociationType { get; set; }
    [StringLength(255)]
    public string? AS_SubmissionSetStatus { get; set; }

    [StringLength(255)]
    public string? AS_SourceObjectId { get; set; }
    [StringLength(255)]
    public string? AS_TargetObjectId { get; set; }
}
