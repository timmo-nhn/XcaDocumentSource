using XcaXds.Commons.Enums;

namespace XcaXds.Commons.Models.Hl7.DataType;

public class PID
{
    public string SetId { get; set; }
    public List<string> PatientIdentifiers { get; set; }
    public string PatientName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Gender { get; set; }
}
