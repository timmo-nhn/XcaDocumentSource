using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Tests.FakesAndDoubles;

public class InMemoryRegistry : IRegistry
{
    public List<RegistryObjectDto> DocumentRegistry = new();

    public OperationResponse DeleteRegistryItem(string id)
    {
        var removedCount = DocumentRegistry.RemoveAll(ro => ro.Id == id);

        return removedCount > 0 ? OperationResponse.Success($"{id} successfully removed") : OperationResponse.Failure($"{id} not found");
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

    public OperationResponse AddItemsToRegistry(List<RegistryObjectDto> dtos)
    {
        DocumentRegistry.AddRange(dtos);
        return OperationResponse.Success("Updated OK");
    }

    public OperationResponse WriteRegistry(List<RegistryObjectDto> dtos)
    {
        DocumentRegistry = dtos;
        return OperationResponse.Success("Write OK");
    }

    IEnumerable<RegistryObjectDto>? IRegistry.GetRegistryItemsAndRelated(string identifier)
    {
        throw new NotImplementedException();
    }

    public RegistryObjectDto? GetSingleRegistryItem(string identifier)
    {
        throw new NotImplementedException();
    }
}