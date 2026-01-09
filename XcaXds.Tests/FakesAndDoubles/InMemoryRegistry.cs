using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Tests.FakesAndDoubles;

public sealed class InMemoryRegistry : IRegistry
{
    public List<RegistryObjectDto> DocumenRegistry = new();

    public bool DeleteRegistryItem(string id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<RegistryObjectDto> ReadRegistry()
    {
        return DocumenRegistry;
    }

    public bool UpdateRegistry(List<RegistryObjectDto> dtos)
    {
        throw new NotImplementedException();
    }

    public bool WriteRegistry(List<RegistryObjectDto> dtos)
    {
        DocumenRegistry.AddRange(dtos);
        return true;
    }
}
