namespace XcaXds.Commons.Models.Custom.RegistryDtos;

public class PatientId
{
    public PatientId(string? id, string? system)
    {
        Id = id;
        System = system;
    }

    public PatientId() { }

    public string? Id { get; set; }
    public string? System { get; set; }
}