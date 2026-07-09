using Hl7.Fhir.Model;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared;

namespace XcaXds.Commons.DataManipulators.Fhir;

public static class XdsErrorToOperationOutcomeMapper
{
    public static RegistryErrorList? GetXdsErrorsFromOperationOutcome(OperationOutcome? operationOutcome)
    {
        if (operationOutcome == null) return null;

        var xdsErrors = operationOutcome.Issue
        .Where(iss =>
            iss.Severity == OperationOutcome.IssueSeverity.Fatal ||
            iss.Severity == OperationOutcome.IssueSeverity.Warning ||
            iss.Severity == OperationOutcome.IssueSeverity.Error)
        .Select(iss => new RegistryErrorType()
        {
            CodeContext = iss.Diagnostics ?? "",
            ErrorCode = iss.Code.ToString() ?? "Unknown",
            Location = string.Join(", ", iss.Location),
            Severity = MapRegistrySeverity(iss.Severity),
        }).ToArray();

        if (xdsErrors.Length == 0)
        {

        }

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
        if (outcome.IssuesOfSeverity(OperationOutcome.IssueSeverity.Fatal, OperationOutcome.IssueSeverity.Error))
        {
            return Constants.Xds.ResponseStatusTypes.Failure;
        }

        if (outcome.IssuesOfSeverity(OperationOutcome.IssueSeverity.Warning))
        {
            return Constants.Xds.ResponseStatusTypes.PartialSuccess;
        }

        return Constants.Xds.ResponseStatusTypes.Success;
    }
}
