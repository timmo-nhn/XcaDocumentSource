using XcaXds.Commons.Models.Custom.DocumentEntry;

namespace XcaXds.Commons.Models.Custom.RestfulRegistry;

public class DocumentResponse : RestfulApiResponse
{
    public DocumentDto Document { get; set; }
}
