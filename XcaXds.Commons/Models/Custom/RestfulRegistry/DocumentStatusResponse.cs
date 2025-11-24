using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Models.Custom.RestfulRegistry;

public class DocumentStatusResponse : RestfulApiResponse
{
    public DocumentStatusDto? Document { get; set; }
}
