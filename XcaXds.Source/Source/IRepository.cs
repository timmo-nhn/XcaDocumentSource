﻿namespace XcaXds.Source.Source;

public interface IRepository
{
    byte[] Read(string documentUniqueId);
    bool Write(string documentId, byte[] data, string patientId = null);
    bool Delete(string documentId);
}
