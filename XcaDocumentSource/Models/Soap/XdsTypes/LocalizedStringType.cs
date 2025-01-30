using Microsoft.Extensions.Localization;
using System.ComponentModel;
using System.Xml.Schema;
using System.Xml.Serialization;
using XcaGatewayService.Commons;

namespace XcaGatewayService.Models.Soap;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public class LocalizedStringType
{
    public LocalizedStringType()
    {
        Charset = "UTF-8";
    }

    [XmlAttribute(AttributeName = "lang", Form = XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
    public string Lang;

    [XmlAttribute(AttributeName = "charset")]
    [DefaultValue("UTF-8")]
    public string Charset;

    [XmlAttribute(AttributeName = "value")]
    public string Value;


}
