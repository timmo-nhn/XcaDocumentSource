using XcaXds.Commons.Models.Hl7.DataType;

namespace XcaXds.Commons.Interfaces;

public interface INinParser
{
    public CX? ParseNinToCxWithAssigningAuthority(string? inputNin);
    public DateTime? ParseNinToDateTime(CX? patientCx);
    public DateTime? ParseNinToDateTime(string? patientIdentifier);
    public int GetAgeFromPatientId(string? patientId);
}