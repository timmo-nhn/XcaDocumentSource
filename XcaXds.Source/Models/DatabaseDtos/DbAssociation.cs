namespace XcaXds.Source.Models.DatabaseDtos;

public class DbAssociation : DbRegistryObject
{
    public string? AS_AssociationType { get; set; }
    public string? AS_SubmissionSetStatus { get; set; }

    public string? AS_SourceObjectId { get; set; }
    public string? AS_TargetObjectId { get; set; }
}
