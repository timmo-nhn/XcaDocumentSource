using System.Xml.Serialization;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlRoot(Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class ClinicalDocument
{
    [XmlAttribute("classCode")]
    public string? classCode { get; set; } = "DOCCLIN";

    [XmlAttribute("moodCode")]
    public string? moodCode { get; set; } = "EVN";


    [XmlElement("realmCode")]
    public List<CS>? RealmCode { get; set; }

    [XmlElement("typeId")]
    public II TypeId { get; set; } = new();

    [XmlElement("templateId")]
    public List<II>? TemplateId { get; set; } = new();

    [XmlElement("id")]
    public II Id { get; set; } = new();

    [XmlElement("code")]
    public CV Code { get; set; } = new();

    [XmlElement("title")]
    public string? Title { get; set; }

    [XmlElement("effectiveTime")]
    public TS EffectiveTime { get; set; } = new();

    [XmlElement("confidentialityCode")]
    public CV ConfidentialityCode { get; set; } = new();

    [XmlElement("languageCode")]
    public CS? LanguageCode { get; set; }

    [XmlElement("setId")]
    public II? SetId { get; set; }

    [XmlElement("versionNumber")]
    public INT? VersionNumber { get; set; }

    [XmlElement("copyTime")]
    public TS? CopyTime { get; set; }

    [XmlElement("recordTarget")]
    public List<RecordTarget> RecordTarget { get; set; } = new();

    [XmlElement("author")]
    public List<Author> Authors { get; set; } = new();

    [XmlElement("dataEnterer")]
    public DataEnterer? DataEnterer { get; set; }

    [XmlElement("informant")]
    public List<Informant>? Informant { get; set; }

    [XmlElement("custodian")]
    public Custodian Custodian { get; set; }

    [XmlElement("informationRecipient")]
    public List<InformationRecipient>? InformationRecipient { get; set; }

    [XmlElement("legalAuthenticator")]
    public LegalAuthenticator LegalAuthenticator { get; set; }

    [XmlElement("authenticator")]
    public List<Authenticator>? Authenticator { get; set; }

    [XmlElement("participant")]
    public List<Participant1>? Participant { get; set; }

    [XmlElement("inFulfillmentOf")]
    public List<InFulfillmentOf>? InFulfillmentOf { get; set; }

    [XmlElement("documentationOf")]
    public List<DocumentationOf>? DocumentationOf { get; set; }

    [XmlElement("relatedDocument")]
    public List<RelatedDocument>? RelatedDocument { get; set; }

    [XmlElement("authorization")]
    public List<Authorization>? Authorization { get; set; }

    [XmlElement("componentOf")]
    public ComponentOf? ComponentOf { get; set; }

    [XmlElement("component")]
    public Component Component { get; set; } = new();
}