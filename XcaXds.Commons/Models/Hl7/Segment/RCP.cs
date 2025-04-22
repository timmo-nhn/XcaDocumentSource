using static XcaXds.Commons.Enums.Hl7;
namespace XcaXds.Commons.Models.Hl7.Segment;

using XcaXds.Commons.Models.Hl7.DataType;
public class RCP
{
    public QueryPriority QueryPriority { get; set; }
    public CQ QuantityLimitedRequest { get; set; }
    public CE ResponseModality { get; set; }
    public TS ExecutionTime { get; set; }
    public ModifyIndicator ModifyIndicator { get; set; }
    public List<SRT> SortBy { get; set; }
    public List<string> SegmentGroupInclusion { get; set; }
}
