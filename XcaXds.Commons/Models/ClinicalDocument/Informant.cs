using System.Xml.Serialization;
using XcaXds.Commons.Enums;

namespace XcaXds.Commons.Models.ClinicalDocument;

[Serializable]
[XmlType("informant", Namespace = Constants.Xds.Namespaces.Hl7V3)]
public class Informant : InfrastructureRoot
{
    [XmlAttribute("typeCode")]
    public string? typecode { get; set; } = "INF";

    [XmlAttribute("contextControlCode")]
    public string? ContextControlCode { get; set; } = "OP";


    internal AssignedEntity? _assignedEntity;
    internal RelatedEntity? _relatedEntity;


    [XmlElement("assignedEntity")]
    public AssignedEntity? AssignedEntity
    {
        get => _assignedEntity;
        set
        {
            if (_relatedEntity != null)
            {
                throw new InvalidOperationException("Cannot set AssignedEntity when RelatedEntity is already set.");
            }

            _assignedEntity = value;

            if (value != null)
            {
                _relatedEntity = null;  
            }
        }
    }

    [XmlElement("relatedEntity")]
    public RelatedEntity? RelatedEntity
    {
        get => _relatedEntity;
        set
        {
            if (_assignedEntity != null)
            {
                throw new InvalidOperationException("Cannot set RelatedEntity when AssignedEntity is already set.");
            }
            
            _relatedEntity = value;

            if (value != null)
            {
                _assignedEntity = null;  
            }
        }
    }
}


