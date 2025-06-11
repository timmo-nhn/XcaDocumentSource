using XcaXds.Commons.Models.Custom.DocumentEntry;

namespace XcaXds.Source.Services.Custom;

public class OpenDipsRegistry : IDocumentRegistry
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