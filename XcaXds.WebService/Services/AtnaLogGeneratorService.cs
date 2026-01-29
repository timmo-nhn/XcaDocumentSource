using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens.Saml2;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;

namespace XcaXds.WebService.Services;

public class AtnaLogGeneratorService
{
    private readonly ILogger<AtnaLogGeneratorService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly IAtnaLogQueue _queue;

    public AtnaLogGeneratorService(ILogger<AtnaLogGeneratorService> logger, ApplicationConfig appConfig, IAtnaLogQueue queue)
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
        try
        {
            _queue.Enqueue(() => GetAuditEventFromFhirRequestResponse(request, response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating auditlog");
        }
    }

    private AuditEvent GetAuditEventFromFhirRequestResponse(Resource request, Resource response)
    {
        throw new NotImplementedException();
    }

    private AuditEvent GetAuditEventFromSoapRequestResponse(SoapEnvelope requestEnvelope, SoapEnvelope responseEnvelope)
    {
        var auditEvent = new AuditEvent();
        auditEvent.Id = Guid.NewGuid().ToString();

        var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(requestEnvelope.Header.Security.Assertion?.OuterXml);
        var statements = samlToken.Assertion.Statements.OfType<Saml2AttributeStatement>().SelectMany(statement => statement.Attributes).ToList();

        var issuer = PolicyRequestMapperSamlService.GetIssuerEnumFromSamlTokenIssuer(samlToken.Issuer);

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

        auditEvent.Entity.Add(new AuditEvent.EntityComponent()
        {
            What = new ResourceReference(requestEnvelope.Header.MessageId, "SOAP message ID"),
        });

        auditEvent.Type = GetAuditEventTypeFromSoapEnvelope(requestEnvelope);
        auditEvent.Recorded = DateTimeOffset.Now;
        auditEvent.Outcome = GetEventOutcomeFromSoapRequestResponse(requestEnvelope, responseEnvelope);
        auditEvent.Action = GetActionFromSoapEnvelope(requestEnvelope);

        var purposeOfUseValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.PurposeOfUse || s.Name == Constants.Saml.Attribute.PurposeOfUse_Helsenorge)?.Values.FirstOrDefault());
        auditEvent.PurposeOfEvent = new List<CodeableConcept>()
        {
            new CodeableConcept()
            {
                Coding = new List<Coding>()
                {
                    new Coding()
                    {
                        Code = purposeOfUseValue?.Code,
                        System = purposeOfUseValue?.CodeSystem?.WithUrnOid(),
                        Display = purposeOfUseValue?.DisplayName
                    }
                }
            }
        };

        var subjectPersonName = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.SubjectId)?.Values.FirstOrDefault());
        var subjectPersonNameParts = subjectPersonName?.Code?.Split().ToList();

        HumanName? healthcarePersonHumanName = null;

        if (subjectPersonNameParts != null && subjectPersonNameParts.Count != 0)
        {
            // The Family name will only contain the last name item, and Given will contain everything else
            healthcarePersonHumanName = new();
            healthcarePersonHumanName.Family = subjectPersonNameParts.LastOrDefault();
            subjectPersonNameParts.RemoveAt(subjectPersonNameParts.Count - 1);
            healthcarePersonHumanName.Given = subjectPersonNameParts;
        }

        var subjectPersonStatement = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ProviderIdentifier)?.Values.FirstOrDefault());

        var subjectUser = new Practitioner()
        {
            Id = "practitioner-1",
            Identifier = new List<Identifier>()
            {
                new Identifier()
                {
                    Value = subjectPersonStatement?.Code,
                    System = subjectPersonStatement?.CodeSystem?.WithUrnOid()
                }
            },
            Name = healthcarePersonHumanName == null ? null : new List<HumanName>()
            {
                healthcarePersonHumanName
            }
        };

        var practitionerRole = new PractitionerRole
        {
            Id = "who-1",
            Identifier = new List<Identifier>()
            {
                new Identifier
                {
                    Value = samlToken.Assertion.Subject.NameId.Value,
                    System = samlToken.Assertion.Issuer.Value
                }
            }
        };
        auditEvent.Contained.Add(practitionerRole);

        // If healthcare personell, then add fields for which organization
        if (issuer == Issuer.HelseId)
        {
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
                        System = pointOfCareStatement?.CodeSystem?.WithUrnOid()
                    }
                },
                Name = pointOfCareName?.Code
            };
            auditEvent.Contained.Add(pointOfCare);
            practitionerRole.Location = [new ResourceReference() { Reference = $"#{pointOfCare.Id}" }];

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
                        System = legalEntityStatement?.CodeSystem?.WithUrnOid()
                    }
                },
                Name = legalEntityName?.Code
            };

            auditEvent.Contained.Add(legalEntity);
            practitionerRole.Organization = new ResourceReference($"#{legalEntity.Id}");
        }

        auditEvent.Contained.Add(subjectUser);
        practitionerRole.Practitioner = new ResourceReference($"#{subjectUser.Id}");

        auditEvent.Agent.Add(new AuditEvent.AgentComponent()
        {
            Who = new ResourceReference($"#{practitionerRole.Id}")
            {
                Identifier = practitionerRole.Identifier.FirstOrDefault()
            },
            Requestor = true,
            PurposeOfUse = auditEvent.PurposeOfEvent,
            Policy = [samlToken.Id],
            Network = new AuditEvent.NetworkComponent()
            {
                Address = _appConfig.IpAddress
            }
        });

        var device = new Device()
        {
            Id = "device-1",
            Identifier =
            [
                new (Constants.Oid.System, _appConfig.RepositoryUniqueId?.WithUrnOid()),
                new (Constants.Oid.System, _appConfig.HomeCommunityId?.WithUrnOid())
            ]
        };
        auditEvent.Contained.Add(device);

        auditEvent.Source = new AuditEvent.SourceComponent()
        {
            Observer = new ResourceReference($"#{device.Id}") { Identifier = new() { System = Constants.Oid.System, Value = _appConfig.HomeCommunityId?.WithUrnOid() } },
            Type = [
                new Coding() {
                    Code = "3",
                    System = "http://terminology.hl7.org/CodeSystem/security-source-type",
                    Display = "Web Server"
                }
            ],
        };

        if (practitionerRole.Organization != null)
        {
            auditEvent.Source.Observer = practitionerRole.Organization;
        }

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

    private AuditEvent.AuditEventOutcome GetEventOutcomeFromSoapRequestResponse(SoapEnvelope requestEnvelope, SoapEnvelope responseEnvelope)
    {
        var registryErrors = responseEnvelope?.Body?.RegistryResponse?.RegistryErrorList?.RegistryError;
        var soapFault = responseEnvelope?.Body?.Fault;

        if (soapFault != null)
        {
            return AuditEvent.AuditEventOutcome.N8;
        }

        if (registryErrors != null && registryErrors.Length > 0)
        {
            return AuditEvent.AuditEventOutcome.N4;
        }

        return AuditEvent.AuditEventOutcome.N0;
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