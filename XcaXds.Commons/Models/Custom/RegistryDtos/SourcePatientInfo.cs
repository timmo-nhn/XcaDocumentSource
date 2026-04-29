using System.ComponentModel.DataAnnotations;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Custom.RegistryDtos;

public class SourcePatientInfo
{
    public PatientId? PatientId { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? FirstName { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? LastName { get; set; }

    public DateTime? BirthTime { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? Gender { get; set; }
}