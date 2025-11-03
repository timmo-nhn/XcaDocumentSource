using System.ComponentModel.DataAnnotations.Schema;

namespace XcaXds.Source.Models.DatabaseDtos.Types;

public class DbSourcePatientInfo
{
    public string? PatientId { get; set; }
    public string? PatientSystem { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? BirthTime { get; set; }
    public string? Gender { get; set; }
}