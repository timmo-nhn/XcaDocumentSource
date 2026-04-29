using System.Xml.Serialization;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Xdsb)]
public class DocumentRequestType
{
    [XmlElement(Namespace = Constants.Xds.Namespaces.Xdsb)]
    public string? HomeCommunityId;

    [XmlElement(Namespace = Constants.Xds.Namespaces.Xdsb)]
    public string? RepositoryUniqueId;

    [XmlElement(Namespace = Constants.Xds.Namespaces.Xdsb)]
    public string? DocumentUniqueId;
}
