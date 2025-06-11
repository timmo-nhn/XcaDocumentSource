using XcaXds.Commons.Models.Custom.DocumentEntry;

namespace XcaXds.Source.Source;


public interface IRegistry
{
    List<RegistryObjectDto> ReadRegistry();
    bool WriteRegistry(List<RegistryObjectDto> dtos);
}
