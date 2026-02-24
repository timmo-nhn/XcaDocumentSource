using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Interfaces;


public interface IRegistry
{
    IEnumerable<RegistryObjectDto> ReadRegistry();
    bool WriteRegistry(List<RegistryObjectDto> dtos);
    bool UpdateRegistry(List<RegistryObjectDto> dtos);          // Bulk inserts without checking for existing items
	bool InsertOrUpdateRegistry(List<RegistryObjectDto> dtos)   // Checks for existing items and updates them, otherwise inserts new items
	{
        throw new NotImplementedException(); 
    }
	bool DeleteRegistryItem(string id);
}