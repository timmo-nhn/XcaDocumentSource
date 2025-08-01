using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Models.Custom.RestfulRegistry;

public class DocumentResponse : RestfulApiResponse
{
    public DocumentDto? Document { get; set; }
}
