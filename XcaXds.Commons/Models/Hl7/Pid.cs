using XcaXds.Commons.Enums;

namespace XcaXds.Commons.Models.Hl7;


//http://www.hl7.eu/refactored/segPID.html#106
public class Pid
{
    public PidType Type { get; set; }
    public object Value { get; set; }

    public Pid(PidType type, object value)
    {
        Type = type;
        Value = value;
    }
}
