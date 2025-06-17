using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Models.Custom.RestfulRegistry;

public class DocumentListEntry
{
    public DocumentEntryDto DocumentReference { get; set; }
    public LinkToDocument LinkToDocument { get; set; }
}