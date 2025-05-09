﻿using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;

namespace XcaXds.Commons.Models.ClinicalDocumentArchitecture;

[Serializable]
[XmlType("section", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Section
{
    [XmlAttribute("ID", DataType = "ID")]
    public string? ID { get; set; }

    [XmlAttribute("nullFlavor")]
    public string? NullFlavor { get; set; }

    [XmlAttribute("classCode")]
    public string ClassCode { get; set; }

    [XmlAttribute("moodCode")]
    public string MoodCode { get; set; }

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; }

    [XmlElement("id")]
    public II? Id { get; set; }

    [XmlElement("code")]
    public CE? Code { get; set; }

    [XmlElement("title")]
    public ST? Title { get; set; }

    [XmlElement("text")]
    public ED? Text { get; set; }

    [XmlElement("confidentialityCode")]
    public CE? ConfidentialityCode { get; set; }

    [XmlElement("languageCode")]
    public CS? LanguageCode { get; set; }

    [XmlElement("subject")]
    public Subject? Subject { get; set; }

    [XmlElement("author")]
    public List<Author>? Author { get; set; }

    [XmlElement("informant")]
    public List<Informant>? Informant { get; set; }

    [XmlElement("entry")]
    public List<Entry>? Entry { get; set; }

    [XmlElement("component")]
    public List<ComponentSection>? Component { get; set; }

}