using System.Xml.Serialization;
using XcaXds.Commons.Enums;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rs)]
public partial class RegistryResponseType
{
    [XmlArray(Order = 0)]
    [XmlArrayItem("Slot", Namespace = Constants.Xds.Namespaces.Rim, IsNullable = false)]
    public SlotType[] ResponseSlotList { get; set; }

    [XmlElement(Order = 1)]
    public RegistryErrorList RegistryErrorList { get; set; }

    [XmlAttribute(AttributeName = "status", DataType = "anyURI")]
    public string Status { get; set; }

    [XmlAttribute(AttributeName = "requestId", DataType = "anyURI")]
    public string RequestId { get; set; }

    public void AddError(XdsErrorCodes errorCode, string codeContext, string? location = null)
    {
        RegistryErrorList ??= new() { RegistryError = [] };

        var error = new RegistryErrorType()
        {
            CodeContext = codeContext,
            ErrorCode = errorCode.ToString(),
            Severity = Constants.Xds.ErrorSeverity.Error,
            Location = location ?? string.Empty
        };
        RegistryErrorList.RegistryError = [.. RegistryErrorList.RegistryError, error];
    }

    public void AddWarning(XdsErrorCodes errorCode, string codeContext, string? location = null)
    {
        RegistryErrorList ??= new() { RegistryError = [] };

        var error = new RegistryErrorType()
        {
            CodeContext = codeContext,
            ErrorCode = errorCode.ToString(),
            Severity = Constants.Xds.ErrorSeverity.Warning,
            Location = location ?? string.Empty
        };
        RegistryErrorList.RegistryError = [.. RegistryErrorList.RegistryError, error];
    }

    public void AddPartialSuccess(string codeContext)
    {
        Status = Constants.Xds.ResponseStatusTypes.PartialSuccess;
        ResponseSlotList = [new SlotType() { ValueList = new() { Value = [codeContext] } }];
    }

    public void SetStatusCode()
    {
        Status = RegistryErrorList?.RegistryError?.Length > 0 ? Constants.Xds.ResponseStatusTypes.PartialSuccess : Constants.Xds.ResponseStatusTypes.Success;
    }
}
