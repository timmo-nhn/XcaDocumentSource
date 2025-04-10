using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;

public class CV : CS
{
    [XmlAttribute("codeSystem")]
    public string CodeSystem { get; set; }

    [XmlAttribute("displayName")]
    public string DisplayName { get; set; }
}
