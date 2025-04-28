using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

[XmlInclude(typeof(IVL_TS))]
[XmlInclude(typeof(EIVL_TS))]
[XmlInclude(typeof(PIVL_TS))]
[XmlInclude(typeof(SXPR_TS))]
[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class SXCM_TS : TS
{
    [XmlAttribute("operator")]
    public string? Operator { get; set; }
}
