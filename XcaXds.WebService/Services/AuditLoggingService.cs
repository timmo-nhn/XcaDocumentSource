using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.IdentityModel.Tokens.Saml2;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.WebService.Services;

public class AuditLoggingService
{
    private readonly ILogger<AuditLoggingService> _logger;
    private readonly ApplicationConfig _appConfig;
    public AuditLoggingService(ILogger<AuditLoggingService> logger, ApplicationConfig appConfig)
    {
        _logger = logger;
        _appConfig = appConfig;
    }

    public void CreateAuditLogForSoapRequestResponse(SoapEnvelope requestEnvelope, SoapEnvelope responseEnvelope)
    {
        _ = Task.Run(() =>
        {
            try
            {
                var auditEvent = GetAuditEventFromSoapRequestResponse(requestEnvelope, responseEnvelope);

                var serializer = new FhirJsonSerializer(new() { Pretty = true });

                _logger.LogDebug($"Created FHIR AuditEvent:\n{serializer.SerializeToString(auditEvent)}");

                // Export??
                var atnaJson = serializer.SerializeToString(auditEvent);
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "AuditEvents", $"{auditEvent.Id}.json"), atnaJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating auditlog");
            }
        });
    }

    private AuditEvent GetAuditEventFromSoapRequestResponse(SoapEnvelope requestEnvelope, SoapEnvelope responseEnvelope)
    {
        var auditEvent = new AuditEvent();
        auditEvent.Id = requestEnvelope.Header.MessageId?.NoUrn();
        var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(requestEnvelope.Header.Security.Assertion?.OuterXml);
        var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();
        var purposeOfUse = statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.PurposeOfUse);
        var purposeOfUseValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(purposeOfUse?.Values.FirstOrDefault());

        auditEvent.Meta = new Meta()
        {
            Profile = ["https://profiles.ihe.net/ITI/BALP/StructureDefinition/IHE.BasicAudit.SAMLaccessTokenUse.Minimal"],
            Security = new List<Coding>()
            {
                new Coding()
                {
                    Code = "HTEST",
                    System = "http://terminology.hl7.org/CodeSystem/v3-ActReason"
                }
            }
        };

        auditEvent.Type = GetAuditEventTypeFromSoapEnvelope(requestEnvelope);
        auditEvent.Recorded = DateTimeOffset.Now;
        auditEvent.Outcome = GetEventOutcomeFromSoapRequestResponse(requestEnvelope, responseEnvelope);
        auditEvent.Action = GetActionFromSoapEnvelope(requestEnvelope);
        auditEvent.PurposeOfEvent = new List<CodeableConcept>()
        {
            new CodeableConcept()
            {
                Coding = new List<Coding>()
                {
                    new Coding()
                    {
                        Code = purposeOfUseValue?.Code,
                        System = $"urn:oid:{purposeOfUseValue?.CodeSystem}",
                        Display = purposeOfUseValue?.DisplayName
                    }
                }
            }
        };

        var practitioner = new Practitioner
        {
            Id = "who-1",
            Identifier = new List<Identifier>
            {
                new Identifier
                {
                    System = $"{samlToken.Assertion.Issuer.Value}",
                    Value = samlToken.Assertion.Subject.NameId.Value
                }
            }
        };

        auditEvent.Contained.Add(practitioner);


        auditEvent.Agent = new List<AuditEvent.AgentComponent>
        {
            new AuditEvent.AgentComponent
            {
                Who = new ResourceReference { Reference = $"#{practitioner.Id}" },
                Requestor = true,
                PurposeOfUse = auditEvent.PurposeOfEvent,
                Policy = [samlToken.Id]
            }
        };

        auditEvent.Source = new AuditEvent.SourceComponent()
        {
            Type = [new() { Code = _appConfig.RepositoryUniqueId }]
        };

        return auditEvent;
    }

    private AuditEvent.AuditEventAction? GetActionFromSoapEnvelope(SoapEnvelope requestEnvelope)
    {
        var action = requestEnvelope.Header.Action;

        switch (action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
            case Constants.Xds.OperationContract.Iti38Action:
            case Constants.Xds.OperationContract.Iti43Action:
            case Constants.Xds.OperationContract.Iti39Action:
                return AuditEvent.AuditEventAction.R;

            case Constants.Xds.OperationContract.Iti41Action:
            case Constants.Xds.OperationContract.Iti42Action:
                return AuditEvent.AuditEventAction.C;

            case Constants.Xds.OperationContract.Iti86Action:
            case Constants.Xds.OperationContract.Iti62Action:
                return AuditEvent.AuditEventAction.D;

            default:
                return AuditEvent.AuditEventAction.R;
        }
    }

    private AuditEvent.AuditEventOutcome? GetEventOutcomeFromSoapRequestResponse(SoapEnvelope requestEnvelope, SoapEnvelope responseEnvelope)
    {
        // FIXME??? Simple solution for now...
        var registryErrors = responseEnvelope?.Body?.RegistryResponse?.RegistryErrorList?.RegistryError;
        if (registryErrors == null || registryErrors?.Length == 0)
        {
            return AuditEvent.AuditEventOutcome.N0;
        }
        else
        {
            return AuditEvent.AuditEventOutcome.N4;
        }
    }

    /// <summary>
    /// <a href="https://hl7.org/fhir/R4/valueset-audit-event-type.html"/>
    /// </summary>
    private Coding GetAuditEventTypeFromSoapEnvelope(SoapEnvelope requestEnvelope)
    {
        var action = requestEnvelope.Header.Action;

        switch (action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
            case Constants.Xds.OperationContract.Iti38Action:
            case Constants.Xds.OperationContract.Iti43Action:
            case Constants.Xds.OperationContract.Iti39Action:
                return new Coding()
                {
                    Code = "access",
                    System = Constants.Hl7.CodeSystems.IsoHealthRecordLifecycleEvent,
                    Display = "Access/View Record Lifecycle Event"
                };

            case Constants.Xds.OperationContract.Iti41Action:
            case Constants.Xds.OperationContract.Iti42Action:
                return new Coding()
                {
                    Code = "originate",
                    System = Constants.Hl7.CodeSystems.IsoHealthRecordLifecycleEvent,
                    Display = "Originate/Retain Record Lifecycle Event"
                };

            case Constants.Xds.OperationContract.Iti86Action:
            case Constants.Xds.OperationContract.Iti62Action:
                return new Coding()
                {
                    Code = "destroy",
                    System = Constants.Hl7.CodeSystems.IsoHealthRecordLifecycleEvent,
                    Display = "Destroy/Delete Record Lifecycle Event"
                };

            default:
                return new Coding()
                {
                    Code = "access",
                    System = Constants.Hl7.CodeSystems.IsoHealthRecordLifecycleEvent,
                    Display = "Access/View Record Lifecycle Event"
                };
        }
    }
}