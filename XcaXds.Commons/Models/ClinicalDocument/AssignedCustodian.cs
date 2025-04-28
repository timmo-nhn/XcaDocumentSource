using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("assignedCustodian", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class AssignedCustodian
{
    [XmlAttribute("classCode")]
    public string? ClassCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("representedCustodianOrganization")]
    public CustodianOrganization CustodianOrganization { get; set; }
}