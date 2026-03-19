using Hl7.Fhir.Model;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Custom.DomainResults;

public sealed class ProvideBundleResult
{
    public OperationOutcome Outcome { get; set; } = new();

    // Needed by controller for ATNA logging
    public ProvideAndRegisterDocumentSetRequestType? ProvideAndRegisterRequest { get; set; }
    public SoapEnvelope? RegistryResponse { get; set; }

    public List<RegistryErrorType>? Errors { get; set; }

    public bool Success =>
        !Outcome.Issue.Any(i => i.Severity == OperationOutcome.IssueSeverity.Error);

}
