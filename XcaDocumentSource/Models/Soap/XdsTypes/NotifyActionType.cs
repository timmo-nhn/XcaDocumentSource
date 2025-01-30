using System.ComponentModel;
using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class NotifyActionType : ActionType
{
    public NotifyActionType()
    {
        NotificationOption = "urn:oasis:names:tc:ebxml-regrep:NotificationOptionType:ObjectRefs";
    }

    [XmlAttribute(AttributeName = "notificationOption", DataType = "anyURI")]
    [DefaultValue("urn:oasis:names:tc:ebxml-regrep:NotificationOptionType:ObjectRefs")]
    public string NotificationOption;

    [XmlAttribute(AttributeName = "endPoint", DataType = "anyURI")]
    public string EndPoint;
}
