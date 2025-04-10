using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

public class TS
{
    [XmlIgnore]
    public DateTime EffectiveTime { get; set; }


    [XmlAttribute("value")]
    public string EffectiveTimeValue
    {
        get => EffectiveTime.ToString(Constants.Hl7.Dtm.DtmFormat);
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    EffectiveTime = DateTime.ParseExact(value, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    EffectiveTime = default;
                }
            }
        }
    }
}
