using NHapi.Model.V21.Datatype;

namespace XcaXds.Commons.Models.Hl7.Segment;

public class QPD
{
    public CE MessageQueryName { get; set; }
    public string QueryTag { get; set; }
    public object Parameters { get; set; }
}
