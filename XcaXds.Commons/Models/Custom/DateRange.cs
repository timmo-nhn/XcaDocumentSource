using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XcaXds.Commons.Models.Custom;

public struct DateRange
{
    public DateRange(DateTime? start, DateTime? end)
    {
        Start = start; 
        End = end;
    }

    public DateTime? Start { get; }
    public DateTime? End { get; }
}
