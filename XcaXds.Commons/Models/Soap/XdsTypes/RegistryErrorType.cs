using System.Xml.Serialization;
using XcaXds.Shared;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Xds.Namespaces.Rs)]
public partial class RegistryErrorType
{
    public RegistryErrorType() { }

    [XmlAttribute(AttributeName = "codeContext")]
    public string? CodeContext;

    [XmlAttribute(AttributeName = "errorCode")]
    public string? ErrorCode;

    [XmlAttribute(AttributeName = "severity")]
    public string? Severity;

    [XmlAttribute(AttributeName = "location")]
    public string? Location;

    [XmlText]
    public string? Value;

    public int GetSeverityLevel()
    {
        switch (Severity)
        {
            case Constants.Xds.ErrorSeverity.Error:
                return 3;
            case Constants.Xds.ErrorSeverity.Warning:
                return 2;
            default:
                return 0;
        }
    }
}