using XcaXds.Commons.Models.Custom.DocumentEntry;

namespace XcaXds.Commons.Models.Custom.RestfulRegistry;

public class DocumentListEntry
{
    public DocumentEntryDto DocumentReference { get; set; }
    public LinkToDocument LinkToDocument { get; set; }
}