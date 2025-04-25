using static XcaXds.Commons.Enums.Hl7;

namespace XcaXds.Commons.Models.Hl7.DataType;

public class MSG
{
    public MessageType MessageType { get; set; }
    public EventType EventType { get; set; }
    public MessageStructure MessageStructure { get; set; }

}