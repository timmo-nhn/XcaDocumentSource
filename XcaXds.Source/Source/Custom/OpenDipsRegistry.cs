using XcaXds.Commons.Models.Custom.DocumentEntry;
using XcaXds.Source.Source;

namespace XcaXds.Source.Source.Custom;

public class OpenDipsRegistry : IRegistry
{
    public OpenDipsRegistry()
    {

    }

    public List<RegistryObjectDto> ReadRegistry()
    {
        throw new NotImplementedException();
    }

    public bool WriteRegistry(List<RegistryObjectDto> dtos)
    {
        throw new NotImplementedException();
    }
}