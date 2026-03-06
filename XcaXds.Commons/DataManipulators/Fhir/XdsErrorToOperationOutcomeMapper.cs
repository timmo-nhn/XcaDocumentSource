using Hl7.Fhir.Model;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.DataManipulators.Fhir;

public static class XdsErrorToOperationOutcomeMapper
{
    public static RegistryErrorList GetXdsErrorsFromOperationOutcome(OperationOutcome operationOutcome)
    {
        var xdsErrors = operationOutcome.Issue
        .Where(iss => iss.Severity != OperationOutcome.IssueSeverity.Success)
        .Select(iss => new RegistryErrorType()
        {
            CodeContext = iss.Diagnostics ?? "",
            ErrorCode = iss.Code.ToString() ?? "Unknown",
            Location = string.Join(", ", iss.Location),
            Severity = MapRegistrySeverity(iss.Severity),
        });

        var registryErrorList = new RegistryErrorList()
        {
            RegistryError = xdsErrors.ToArray(),
            HighestSeverity = MapResponseStatus(operationOutcome)
        };

        return registryErrorList;
    }

    private static string MapRegistrySeverity(OperationOutcome.IssueSeverity? severity)
    {
        return severity switch
        {
            OperationOutcome.IssueSeverity.Fatal =>
                Constants.Xds.ErrorSeverity.Error,

            OperationOutcome.IssueSeverity.Error =>
                Constants.Xds.ErrorSeverity.Error,

            OperationOutcome.IssueSeverity.Warning =>
                Constants.Xds.ErrorSeverity.Error,

            OperationOutcome.IssueSeverity.Information =>
                Constants.Xds.ErrorSeverity.Warning,

            _ =>
                Constants.Xds.ErrorSeverity.Warning,

        };
    }

    private static string MapResponseStatus(OperationOutcome outcome)
    {
        if (outcome.Issue.Any(i =>
            i.Severity == OperationOutcome.IssueSeverity.Fatal ||
            i.Severity == OperationOutcome.IssueSeverity.Error))
        {
            return Constants.Xds.ResponseStatusTypes.Failure;
        }

        if (outcome.Issue.Any(i =>
            i.Severity == OperationOutcome.IssueSeverity.Warning))
        {
            return Constants.Xds.ResponseStatusTypes.PartialSuccess;
        }

        return Constants.Xds.ResponseStatusTypes.Success;
    }
}
