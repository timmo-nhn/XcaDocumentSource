using XcaXds.Commons.Models.Custom.DocumentEntry;

namespace XcaXds.Commons.Interfaces;


public interface IRegistry
{
    List<RegistryObjectDto> ReadRegistry();
    bool WriteRegistry(List<RegistryObjectDto> dtos);
}
