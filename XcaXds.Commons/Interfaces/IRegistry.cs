using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Interfaces;


public interface IRegistry
{
    /// <summary>
    /// (Optional) Read items and related items (Associations/Submissionset) from the Registry 
    /// </summary>
    IEnumerable<RegistryObjectDto>? GetRegistryItemsAndRelated(string identifier) { throw new NotSupportedException(); }

    /// <summary>
    /// (Optional) Read a single item from the registry by its unique identifier
    /// </summary>
    RegistryObjectDto? GetSingleRegistryItem(string identifier) { throw new NotSupportedException(); }

    /// <summary>
    /// (Optional) Read data for a specific patient from the registry
    /// </summary>
    IEnumerable<RegistryObjectDto> GetRegistryItemsForPatient(PatientId patientIdentifier) { throw new NotSupportedException(); }

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
    OperationResponse UpdateRegistry(List<RegistryObjectDto> dtos);

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