using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Tests.FakesAndDoubles;

public sealed class InMemoryRegistry : IRegistry
{
    public List<RegistryObjectDto> DocumenRegistry = new();

    public bool DeleteRegistryItem(string id)
    {
        var removedCount = DocumenRegistry.RemoveAll(ro => ro.Id == id);

        return removedCount > 0;
    }

    public IEnumerable<RegistryObjectDto> ReadRegistry()
    {
        return DocumenRegistry;
    }

    public bool UpdateRegistry(List<RegistryObjectDto> dtos)
    {
        DocumenRegistry.AddRange(dtos);
        return true;
    }

    public bool WriteRegistry(List<RegistryObjectDto> dtos)
    {
        DocumenRegistry = dtos;
        return true;
    }
}
