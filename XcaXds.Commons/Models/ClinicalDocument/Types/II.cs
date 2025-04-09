using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument.Types;
[Serializable]
public class II
{
    [XmlAttribute("root")]
    public string Root { get; set; }

    [XmlAttribute("extension")]
    public string Extension { get; set; }
}
