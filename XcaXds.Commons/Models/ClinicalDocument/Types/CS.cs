using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

public class CS
{
    [XmlAttribute("code")]
    public string Code { get; set; }
}
