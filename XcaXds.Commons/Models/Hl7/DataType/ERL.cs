using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XcaXds.Commons.Models.Hl7.DataType;

public class ERL
{
    public string SegmentId { get; set; }
    public int SegmentSequence { get; set; }
    public int FieldPosition { get; set; }
    public int FieldRepetition { get; set; }
    public int ComponentNumber { get; set; }
    public int SubcomponentNumber { get; set; }
}
