using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Tests.FakesAndDoubles;

public sealed class InMemoryRepository : IRepository
{
    public List<DocumentDto> DocumentRepository = new();

    public bool Delete(string? documentId)
    {
        var removeCount = DocumentRepository.RemoveAll(doc => doc.DocumentId == documentId);
        return removeCount > 0;
    }

    public byte[]? Read(string documentUniqueId)
    {
        return DocumentRepository.FirstOrDefault(doc => doc.DocumentId == documentUniqueId)?.Data;
    }

    public bool SetNewOid(string repositoryUniqueId, out string oldId)
    {
        throw new NotImplementedException();
    }

    public bool Write(string documentId, byte[] data, string patientId = null)
    {
        DocumentRepository.Add(new() { DocumentId = documentId, Data = data });
        return true;
    }
}
