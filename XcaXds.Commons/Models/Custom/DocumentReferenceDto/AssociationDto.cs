namespace XcaXds.Commons.Models.Custom.DocumentEntry;

public class AssociationDto : RegistryObjectDto
{
    public string? AssociationType { get; set; }
    public string? SourceObject { get; set; }
    public string? SubmissionSetStatus { get; set; }
    public string? TargetObject { get; set; }
}