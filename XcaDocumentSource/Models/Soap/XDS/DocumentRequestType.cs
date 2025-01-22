using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Xdsb)]
public class DocumentRequestType
{
    [XmlElement(Order = 0)]
    public string HomeCommunityId;

    [XmlElement(Namespace = Constants.Xds.Namespaces.Xdsb, Order = 1)]
    public string RepositoryUniqueId;

    [XmlElement(Namespace = Constants.Xds.Namespaces.Xdsb, Order = 2)]
    public string DocumentUniqueId;
}
