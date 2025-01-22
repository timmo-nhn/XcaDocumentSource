using System.ComponentModel;
using System.Xml.Serialization;

namespace XcaDocumentSource.Models.Soap.Xds;


[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Rs)]
public partial class RegistryErrorType
{
    public RegistryErrorType()
    {
        Severity = Constants.Xds.ErrorSeverity.Error;
    }

    [XmlAttribute(AttributeName = "codeContext")]
    public string CodeContext;

    [XmlAttribute(AttributeName = "errorCode")]
    public string ErrorCode;

    [XmlAttribute(AttributeName = "severity", DataType = "anyURI")]
    [DefaultValue(Constants.Xds.ErrorSeverity.Error)]
    public string Severity;

    [XmlAttribute(AttributeName = "location")]
    public string Location;

    [XmlText]
    public string Value;
}
