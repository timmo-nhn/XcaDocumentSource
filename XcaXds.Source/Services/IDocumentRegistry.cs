using XcaXds.Commons.Models.Custom.DocumentEntry;

namespace XcaXds.Source.Services;


public interface IDocumentRegistry
{
    List<RegistryObjectDto> ReadRegistry();
    bool WriteRegistry(List<RegistryObjectDto> dtos);
}
