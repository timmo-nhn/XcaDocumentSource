using System.ComponentModel.DataAnnotations;
using XcaXds.Shared;

namespace XcaXds.Commons.Models.Custom.RegistryDtos;

public class PatientId
{
    public PatientId(string? id, string? system)
    {
        Id = id;
        System = system;
    }

    public PatientId() { }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? Id { get; set; }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? System { get; set; }
}