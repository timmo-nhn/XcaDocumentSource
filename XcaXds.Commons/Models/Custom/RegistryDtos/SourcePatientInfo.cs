namespace XcaXds.Commons.Models.Custom.RegistryDtos;

public class SourcePatientInfo
{
    public PatientId? PatientId { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public DateTime? BirthTime { get; set; }
    public string? Gender { get; set; }
}