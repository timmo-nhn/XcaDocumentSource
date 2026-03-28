namespace XcaXds.Source.Models.DatabaseDtos.Types;

public class DbSourcePatientInfo
{
    public required string PatientId { get; init; }
    public required string PatientSystem { get; init; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? BirthTime { get; set; }
    public string? Gender { get; set; }
}