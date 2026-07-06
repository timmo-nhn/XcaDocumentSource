using XcaXds.Commons.Models.Custom;

namespace XcaXds.Commons.Interfaces;

public interface IRepository
{
    byte[]? Read(string documentUniqueId);
    OperationResponse Write(string documentId, byte[] data, string? patientId = null);
    OperationResponse Delete(string? documentId);
    bool SetNewOid(string repositoryUniqueId, out string? oldId) { throw new NotSupportedException(); }
}
