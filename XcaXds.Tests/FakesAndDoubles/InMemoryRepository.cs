using Hl7.Fhir.Model;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Tests.FakesAndDoubles;

public sealed class InMemoryRepository : IRepository
{
    public List<DocumentDto> DocumentRepository = new();

    public OperationResponse Delete(string? documentId)
    {
        var removeCount = DocumentRepository.RemoveAll(doc => doc.DocumentId == documentId);
        return removeCount > 0 ? OperationResponse.Success($"{documentId} successfully removed") : OperationResponse.Failure($"{documentId} not found");
    }

    public byte[]? Read(string documentUniqueId)
    {
        return DocumentRepository.FirstOrDefault(doc => doc.DocumentId == documentUniqueId)?.Data;
    }

    public OperationResponse Write(string documentId, byte[] data, string? patientId = null)
    {
        DocumentRepository.Add(new() { DocumentId = documentId, Data = data });
        return OperationResponse.Success("Write OK");
    }
}
