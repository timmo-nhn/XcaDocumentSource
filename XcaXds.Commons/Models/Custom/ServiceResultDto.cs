using System.Linq;
using Hl7.Fhir.Model;

namespace XcaXds.Commons.Models.Custom;

/// <summary>
/// Small DTO(Data Transfer Object)-class to make communicating statuses between API controllers and services easier
/// </summary>
/// <typeparam name="T"></typeparam>
public class ServiceResultDto<T>
{
    public T? Value { get; set; }

    public OperationOutcome? OperationOutcome { get; set; }
    public bool Success =>
        OperationOutcome == null || !OperationOutcome.Issue.Any() || OperationOutcome.Issue
        .All(issue => 
            issue.Severity == OperationOutcome.IssueSeverity.Information ||
            issue.Severity == OperationOutcome.IssueSeverity.Success ||
            issue.Severity == OperationOutcome.IssueSeverity.Warning );
}
