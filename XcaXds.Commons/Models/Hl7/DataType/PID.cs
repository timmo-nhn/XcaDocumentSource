using XcaXds.Commons.Enums;

namespace XcaXds.Commons.Models.Hl7.DataType;


public class PID
{
    public PidType Type { get; set; }
    public object Value { get; set; }

    public PID(PidType type, object value)
    {
        Type = type;
        Value = value;
    }
}
