using System.Xml.Serialization;
using XcaXds.Commons;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[XmlRoot]
[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class ClinicalDocument
{

    [XmlElement("typeId")]
    public II TypeId {  get; set; }

    [XmlElement("templateId")]
    public II TemplateId {  get; set; }

    [XmlElement("id")]
    public II Id {  get; set; }

    [XmlElement("code")]
    public CE code { get; set; }

    [XmlAttribute("title")]
    public string Title { get; set; }
}