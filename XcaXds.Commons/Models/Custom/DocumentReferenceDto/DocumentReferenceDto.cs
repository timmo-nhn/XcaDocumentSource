namespace XcaXds.Commons.Models.Custom.DocumentEntry;

public class DocumentReferenceDto
{
    public DocumentEntryDto DocumentEntryMetadata { get; set; }
    public SubmissionSetDto SubmissionSetMetadata { get; set; }
    public AssociationDto Association { get; set; }
    public DocumentDto DocumentEntryDocument { get; set; }
}