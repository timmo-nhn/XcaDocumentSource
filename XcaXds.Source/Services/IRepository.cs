namespace XcaXds.Source.Services;

public interface IRepository
{
    byte[] Read(byte[] data);
    bool Write(string documentId, byte[] data, string patientId);
    bool Delete(string documentId);
}
