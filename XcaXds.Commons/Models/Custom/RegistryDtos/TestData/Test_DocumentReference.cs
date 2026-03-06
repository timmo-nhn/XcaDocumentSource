namespace XcaXds.Commons.Models.Custom.RegistryDtos.TestData;

public class Test_DocumentReference
{
    public Test_DocumentEntryValues PossibleDocumentEntryValues { get; set; } = new();
    public Test_SubmissionSetValues PossibleSubmissionSetValues { get; set; } = new();
    public string[]? Documents { get; set; }
}