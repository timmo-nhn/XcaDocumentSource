using XcaXds.Commons.Models.Custom.DocumentEntry;
using XcaXds.Commons.Models.Hl7.DataType;

namespace XcaXds.Commons.Extensions;

public static class JsonFindDocuments
{
    public static IEnumerable<DocumentEntryDto> ByDocumentEntryPatientId(
        this IEnumerable<DocumentEntryDto> source, CX? patientId)
    {
        if (string.IsNullOrWhiteSpace(patientId?.IdNumber)) return Enumerable.Empty<DocumentEntryDto>();
        return source
            .Where(eo => eo?.SourcePatientInfo?.PatientId?.Id == patientId.IdNumber &&
            eo?.SourcePatientInfo?.PatientId?.System.NoUrn() == patientId.AssigningAuthority.UniversalId.NoUrn());
    }
}