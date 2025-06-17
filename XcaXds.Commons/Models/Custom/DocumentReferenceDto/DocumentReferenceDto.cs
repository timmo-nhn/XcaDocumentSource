namespace XcaXds.Commons.Models.Custom.DocumentEntry;

public class DocumentReferenceDto
{
    public DocumentEntryDto? DocumentEntry { get; set; }
    public SubmissionSetDto? SubmissionSet { get; set; }
    public AssociationDto? Association { get; set; }
    public DocumentDto? Document { get; set; }
}