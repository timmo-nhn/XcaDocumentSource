using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Soap.Actions;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rs)]
public partial class RegistryResponseType
{
    [XmlArray]
    [XmlArrayItem("Slot", Namespace = Constants.Xds.Namespaces.Rim, IsNullable = false)]
    public SlotType[]? ResponseSlotList { get; set; }

    [XmlElement]
    public RegistryErrorList? RegistryErrorList { get; set; }

    [XmlAttribute(AttributeName = "status", DataType = "anyURI")]
    public string? Status { get; set; }

    [XmlAttribute(AttributeName = "requestId", DataType = "anyURI")]
    public string? RequestId { get; set; }

    public RegistryResponseType AddError(XdsErrorCodes errorCode, string codeContext, string? location = null)
    {
        RegistryErrorList ??= new() { RegistryError = [] };

        var error = new RegistryErrorType()
        {
            CodeContext = codeContext,
            ErrorCode = errorCode.ToString(),
            Severity = Constants.Xds.ErrorSeverity.Error,
            Location = location ?? string.Empty
        };
        RegistryErrorList.RegistryError = [.. RegistryErrorList.RegistryError!, error];
        EvaluateStatusCode();

        return this;
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
        RegistryErrorList.RegistryError = [.. RegistryErrorList.RegistryError!, error];
        EvaluateStatusCode();
    }

    public void AddPartialSuccess(string codeContext)
    {
        Status = Constants.Xds.ResponseStatusTypes.PartialSuccess;
        ResponseSlotList = [new SlotType() { ValueList = new() { Value = [codeContext] } }];
        EvaluateStatusCode();
    }

    public void EvaluateStatusCode()
    {
        if (RegistryErrorList?.RegistryError?.Length > 0)
        {
            var highestSeverity = RegistryErrorList.RegistryError.MaxBy(err => err.GetSeverityLevel());

            RegistryErrorList.HighestSeverity = highestSeverity?.Severity ?? Constants.Xds.ErrorSeverity.Error;
        }

        if (RegistryErrorList?.RegistryError?.Length > 0 && RegistryErrorList.HighestSeverity == Constants.Xds.ErrorSeverity.Error)
        {
            Status = Constants.Xds.ResponseStatusTypes.Failure;
        }
        else
        {
            Status = RegistryErrorList?.RegistryError?.Length > 0
                ? Constants.Xds.ResponseStatusTypes.PartialSuccess
                : Constants.Xds.ResponseStatusTypes.Success ?? Constants.Xds.ResponseStatusTypes.Success;
        }
    }
}
