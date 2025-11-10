using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Interfaces;


public interface IRegistry
{
    IEnumerable<RegistryObjectDto> ReadRegistry();
    bool WriteRegistry(List<RegistryObjectDto> dtos);
    bool UpdateRegistry(List<RegistryObjectDto> dtos);
    bool DeleteRegistryItem(string id);
}