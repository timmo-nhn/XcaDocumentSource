using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Tests.FakesAndDoubles;

public class InMemoryRegistry : IRegistry
{
    public List<RegistryObjectDto> DocumentRegistry = new();

    public bool DeleteRegistryItem(string id)
    {
        var removedCount = DocumentRegistry.RemoveAll(ro => ro.Id == id);

        return removedCount > 0;
    }

    public IEnumerable<RegistryObjectDto> GetRegistryItemsForPatient(PatientId patientIdentifier)
    {
        return DocumentRegistry;
    }

    public RegistryObjectDto? GetRegistryItemsAndRelated(string? identifier)
    {
        return ReadRegistry().ToBlockingEnumerable().FirstOrDefault(ro => ro.Id == identifier);
    }

    public IAsyncEnumerable<RegistryObjectDto> ReadRegistry()
    {
        return GetRegistryItemsForPatient(new PatientId()).ToAsyncEnumerable();
    }

    public bool UpdateRegistry(List<RegistryObjectDto> dtos)
    {
        DocumentRegistry.AddRange(dtos);
        return true;
    }

    public bool WriteRegistry(List<RegistryObjectDto> dtos)
    {
        DocumentRegistry = dtos;
        return true;
    }
}
