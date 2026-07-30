using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Interfaces;


public interface IRegistry
{
    /// <summary>
    /// Read items and related items (Associations/Submissionset) from the Registry 
    /// </summary>
    IEnumerable<RegistryObjectDto>? GetRegistryItemsAndRelated(string identifier);

    /// <summary>
    /// Read a single item from the registry by its unique identifier
    /// </summary>
    RegistryObjectDto? GetSingleRegistryItem(string identifier);

    /// <summary>
    /// Read data for a specific patient from the registry
    /// </summary>
    IEnumerable<RegistryObjectDto> GetRegistryItemsForPatient(PatientId patientIdentifier);

    /// <summary>
    /// Read everything from the registry
    /// </summary>
    IAsyncEnumerable<RegistryObjectDto> ReadRegistry();

    /// <summary>
    /// Write a list of entities to the registry
    /// </summary>
    OperationResponse WriteRegistry(List<RegistryObjectDto> dtos);

    /// <summary>
    /// Bulk inserts without checking for existing items
    /// </summary>
    OperationResponse AddItemsToRegistry(List<RegistryObjectDto> dtos);

    /// <summary>
    /// Checks for existing items and updates them, otherwise inserts new items
    /// </summary>
    OperationResponse InsertOrUpdateRegistry(List<RegistryObjectDto> dtos)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete a single registry item
    /// </summary>
    OperationResponse DeleteRegistryItem(string id);
}