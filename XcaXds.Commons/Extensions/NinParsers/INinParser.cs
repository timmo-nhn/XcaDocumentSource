using XcaXds.Commons.Models.Hl7.DataType;

namespace XcaXds.Commons.Extensions.NinParsers;

/// <summary>
/// Parse a National Identifier Number and get birth dates and standardized coded identifiers
/// 
/// </summary>
public interface INinParser
{
    public bool CanHandle(string inputNin);
    public CX? ParseNinToCxWithAssigningAuthority(string? inputNin);
    public DateTime? ParseNinToDateTime(CX? patientCx);
    public DateTime? ParseNinToDateTime(string? patientIdentifier);
    public int GetAgeFromPatientId(string? patientId);
}