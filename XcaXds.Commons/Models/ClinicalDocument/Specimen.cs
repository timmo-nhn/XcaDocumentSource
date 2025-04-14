using System.Xml.Serialization;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("specimen", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Specimen
{
    [XmlAttribute("typeCode")]
    public string? TypeCode { get; set; }

    [XmlElement("specimenRole")]
    public SpecimenRole SpecimenRole { get; set; }
}