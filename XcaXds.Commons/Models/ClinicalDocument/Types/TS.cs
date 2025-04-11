using System.Globalization;
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class TS : ANY
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
                    EffectiveTime = DateTime.ParseExact(value, Constants.Hl7.Dtm.DtmYmdFormat, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    EffectiveTime = default;
                }
            }
        }
    }
}
