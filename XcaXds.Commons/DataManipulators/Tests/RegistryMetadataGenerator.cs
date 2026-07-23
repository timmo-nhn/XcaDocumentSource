using System.Security.Cryptography;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;

namespace XcaXds.Commons.DataManipulators.Tests;

public static class RegistryMetadataGenerator
{
    public static List<DocumentReferenceDto> GenerateRandomizedTestData(
        string? homeCommunityId,
        string? repositoryUniqueId,
        Test_DocumentReference? jsonTestData,
        int? entriesToGenerate = 10,
        string? patientIdentifier = null,
        bool noDeprecatedDocuments = false
        )
    {
        if (jsonTestData?.PossibleDocumentEntryValues == null) return [];

        if (noDeprecatedDocuments == true)
        {
            jsonTestData.PossibleDocumentEntryValues.AvailabilityStatuses = [Constants.Xds.StatusValues.Approved];
        }

        jsonTestData.PossibleSubmissionSetValues?.Authors ??= jsonTestData.PossibleDocumentEntryValues.Authors;

        var patientIdentifierPid = Hl7Object.Parse<PID>(patientIdentifier) is { PatientId: not null } pidPid ? pidPid : null;
        var patientIdentifierCx = Hl7Object.Parse<CX>(patientIdentifier) is { IdNumber: not null } pidCx ? pidCx : null;

        var sourcePatientInfoForPatient = jsonTestData.PossibleDocumentEntryValues.SourcePatientInfos?.FirstOrDefault(spi =>
        (spi?.PatientId?.Id == patientIdentifierPid?.PatientId?.IdNumber && 
        spi?.PatientId?.System == patientIdentifierPid?.PatientId?.AssigningAuthority?.UniversalId) 
        ||
        (spi?.PatientId?.Id == patientIdentifierCx?.IdNumber &&
        spi?.PatientId?.System == patientIdentifierCx?.AssigningAuthority?.UniversalId)
        ||
        (spi?.PatientId?.Id == patientIdentifier));

        if (sourcePatientInfoForPatient != null)
        {
            jsonTestData.PossibleDocumentEntryValues.SourcePatientInfos = [sourcePatientInfoForPatient];
        }

        var generatedTestRegistryObjects = TestDataGenerator.GenerateRegistryObjectsFromTestData(jsonTestData, (int)entriesToGenerate!);

        foreach (var generatedTestObject in generatedTestRegistryObjects)
        {
            var documentContent = generatedTestObject.Document?.Data;

            if (generatedTestObject?.DocumentEntry?.SourcePatientInfo?.PatientId?.Id != null && generatedTestObject.DocumentEntry.Id != null && documentContent != null)
            {
                generatedTestObject.DocumentEntry.Title = "XcaDS - " + generatedTestObject.DocumentEntry.Title;
                generatedTestObject.DocumentEntry.Size = documentContent.Length.ToString();
                generatedTestObject.DocumentEntry.Hash = BitConverter.ToString(SHA1.HashData(documentContent)).Replace("-", "").ToLowerInvariant();
                generatedTestObject.DocumentEntry.HomeCommunityId = homeCommunityId;
                generatedTestObject.DocumentEntry.RepositoryUniqueId = repositoryUniqueId;
            }
        }

        return generatedTestRegistryObjects;
    }
}
