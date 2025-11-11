using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens.Saml2;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;

namespace XcaXds.WebService.Services;

public class AuditLogGeneratorService
{
    private readonly ILogger<AuditLogGeneratorService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly IAuditLogQueue _queue;
    public AuditLogGeneratorService(ILogger<AuditLogGeneratorService> logger, ApplicationConfig appConfig, IAuditLogQueue queue)
    {
        _logger = logger;
        _appConfig = appConfig;
        _queue = queue;
    }


    public void CreateAuditLogForSoapRequestResponse(SoapEnvelope requestEnvelope, SoapEnvelope responseEnvelope)
    {
        try
        {
            _queue.Enqueue(() => GetAuditEventFromSoapRequestResponse(requestEnvelope, responseEnvelope));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating auditlog");
        }
    }

    public void CreateAuditLogForFhirRequestResponse(Resource request, Resource response)
    {

    }

    private AuditEvent GetAuditEventFromSoapRequestResponse(SoapEnvelope requestEnvelope, SoapEnvelope responseEnvelope)
    {
        var auditEvent = new AuditEvent();
        auditEvent.Id = requestEnvelope.Header.MessageId?.NoUrn();
        var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(requestEnvelope.Header.Security.Assertion?.OuterXml);
        var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

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

        var purposeOfUseValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.PurposeOfUse)?.Values.FirstOrDefault());

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

        var healthcarePersonStatement = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ProviderIdentifier)?.Values.FirstOrDefault());
        var healthcarePersonName = SamlExtensions.GetSamlAttributeValueAsCodedValue(samlToken.Assertion.Subject.NameId.Value);
        var healthcarePersonNameParts = healthcarePersonName?.Code?.Split().ToList();

        HumanName? healthcarePersonHumanName = null;
        
        if (healthcarePersonNameParts != null && healthcarePersonNameParts.Count != 0)
        {
            // The Family name will only contain the last name item, and Given will contain everything else
            healthcarePersonHumanName = new();
            healthcarePersonHumanName.Family = healthcarePersonNameParts.LastOrDefault();
            healthcarePersonNameParts.RemoveAt(healthcarePersonNameParts.Count);
            healthcarePersonHumanName.Given = healthcarePersonNameParts;
        }

        var practitioner = new Practitioner()
        {
            Id = "practitioner-1",
            Identifier = new List<Identifier>()
            {
                new Identifier()
                {
                    Value = healthcarePersonStatement?.Code,
                    System = healthcarePersonStatement?.CodeSystem
                }
            },
            Name = healthcarePersonHumanName == null ? null : new List<HumanName>()
            {
                healthcarePersonHumanName
            }
        };

        auditEvent.Contained.Add(practitioner);

        var pointOfCareStatement = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ChildOrganization)?.Values.FirstOrDefault());
        var pointOfCareName = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.TrustChildOrgName)?.Values.FirstOrDefault());

        var pointOfCare = new Organization()
        {
            Id = "org-pointofcare-1",
            Identifier = new List<Identifier>()
            {
                new Identifier()
                {
                    Value = pointOfCareStatement?.Code,
                    System = pointOfCareStatement?.CodeSystem
                }
            },
            Name = pointOfCareName?.Code
        };
        auditEvent.Contained.Add(pointOfCare);


        var legalEntityStatement = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.OrganizationId)?.Values.FirstOrDefault());
        var legalEntityName = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.Organization)?.Values.FirstOrDefault());

        var legalEntity = new Organization()
        {
            Id = "org-legalentity-1",
            Identifier = new List<Identifier>()
            {
                new Identifier()
                {
                    Value = legalEntityStatement?.Code,
                    System = legalEntityStatement?.CodeSystem
                }
            },
            Name = legalEntityName?.Code

        };
        auditEvent.Contained.Add(legalEntity);


        var practitionerRole = new PractitionerRole
        {
            Id = "who-1",
            Identifier = new List<Identifier>()
            {
                new Identifier
                {
                    System = $"{samlToken.Assertion.Issuer.Value}",
                    Value = samlToken.Assertion.Subject.NameId.Value
                }
            },
            Organization = new ResourceReference { Reference = $"#{legalEntity.Id}" },
            Location = [new ResourceReference() { Reference = $"#{pointOfCare.Id}" }],
            Practitioner = new ResourceReference() { Reference = $"#{practitioner.Id}" }
        };

        auditEvent.Contained.Add(practitionerRole);

        auditEvent.Agent.Add(new AuditEvent.AgentComponent()
        {
            Who = new ResourceReference { Reference = $"#{practitionerRole.Id}" },
            Requestor = true,
            PurposeOfUse = auditEvent.PurposeOfEvent,
            Policy = [samlToken.Id]
        });

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