﻿using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Extensions;
namespace XcaXds.Source;

public class RepositoryWrapper
{
    public void GetFileFromRepository(SoapEnvelope soapEnvelope)
    {
        throw new NotImplementedException();
    }

    public bool StoreDocument(DocumentType assocDocument, string patientIdPart)
    {
        string baseDirectory = AppContext.BaseDirectory;
        string repositoryPath = Path.Combine(baseDirectory, "..", "..", "..", "Repository", patientIdPart);

        if (!Directory.Exists(repositoryPath))
        {
            Directory.CreateDirectory(repositoryPath);
        }

        // Create a file in the Repository folder
        string filePath = Path.Combine(repositoryPath, assocDocument.Id.NoUrn());
        File.WriteAllBytes(filePath, assocDocument.Value);
        return true;

    }

}
