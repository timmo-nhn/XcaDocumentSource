namespace XcaXds.Source.Models.DatabaseDtos;

public class DbAssociation : DbRegistryObject
{
    public string? AssociationType { get; set; }
    public string? SubmissionSetStatus { get; set; }

    public string? SourceObjectId { get; set; }
    public string? TargetObjectId { get; set; }
}
