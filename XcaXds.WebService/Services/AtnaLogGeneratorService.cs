using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using Hl7.FhirPath.Sprache;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using static XcaXds.Commons.Commons.Constants.Xds.AssociationType;

namespace XcaXds.WebService.Services;

public class AtnaLogGeneratorService
{
    private readonly ILogger<AtnaLogGeneratorService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly IAtnaLogQueue _queue;
    private readonly RegistryWrapper _registryWrapper;
    private readonly AtnaLogEnricherService _atnaLogEnricherService;

    public AtnaLogGeneratorService(ILogger<AtnaLogGeneratorService> logger, ApplicationConfig appConfig, IAtnaLogQueue queue, RegistryWrapper registryWrapper, AtnaLogEnricherService atnaLogEnricherService)
    {
        _logger = logger;
        _appConfig = appConfig;
        _queue = queue;
        _registryWrapper = registryWrapper;
        _atnaLogEnricherService = atnaLogEnricherService;
    }

    public void CreateAuditLogForSoapRequestResponse(SoapEnvelope requestEnvelope, SoapEnvelope? responseEnvelope)
    {
        _queue.Enqueue(() => GetAuditEventFromSoapRequestResponse(requestEnvelope, responseEnvelope));
    }

    public void CreateAuditLogForFhirDeleteDocumentsRequest(AdditionalParameters additionalParameters, DocumentEntryDto? deletedEntry, OperationOutcome operationOutcome, JwtSecurityToken? token)
    {
        _queue.Enqueue(() => GetAuditEventFromDocumentEntryOperationOutcomeAndJwt(additionalParameters, deletedEntry, operationOutcome, token));
    }

    public void CreateAuditLogForFhirPatchDocumentSecurityLabelRequest(AdditionalParameters httpContext, List<CodedValue>? oldSecurityLabel, DocumentEntryDto? updatedEntry, JwtSecurityToken? token)
    {
        var newSecurityLabel = updatedEntry?.ConfidentialityCode;
        // Clone to avoid mutation after we enqueue (controller updates the same DTO instance).
        var oldCopy = CloneCodedValues(oldSecurityLabel);
        var newCopy = CloneCodedValues(newSecurityLabel);
        var updatedCopy = CloneDocumentEntryDto(updatedEntry);

        _queue.Enqueue(() => GetAuditEventFromPatchedDocumentEntryAndJwt(httpContext, updatedCopy, token, updatedEntry?.Id, oldCopy, newCopy));
    }

    private AuditEvent GetAuditEventFromDocumentEntryOperationOutcomeAndJwt(AdditionalParameters additionalParameters, DocumentEntryDto? deletedEntry, OperationOutcome operationOutcome, JwtSecurityToken? token)
    {
        var documentId = operationOutcome.Issue.FirstOrDefault()?.Location.FirstOrDefault();

        // If a resource that doesnt exist was attempted to be deleted
        if (deletedEntry == null && documentId != null)
        {
            deletedEntry = new DocumentEntryDto() { Id = documentId };
        }

        var extrinsicObject = RegistryMetadataTransformer.TransformDocumentReferenceDtoListToRegistryObjects([deletedEntry]).FirstOrDefault();

        var soapEnvelope = _atnaLogEnricherService.GetMockSoapEnvelopeFromJwtAndBundle(additionalParameters, token?.RawData, null, [extrinsicObject]);

        soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti62Action);
        soapEnvelope.Header.MessageId = additionalParameters.TraceIdentifier;

        var responseEnvelope = new SoapEnvelope()
        {
            Body = new()
            {
                RegistryResponse = new()
                {
                    RegistryErrorList = XdsErrorToOperationOutcomeMapper.GetXdsErrorsFromOperationOutcome(operationOutcome),
                }
            }
        };

        responseEnvelope.Body.RegistryResponse.EvaluateStatusCode();

        var auditEvent = GetAuditEventFromSoapRequestResponse(soapEnvelope, responseEnvelope);

        return auditEvent;
    }

    private AuditEvent GetAuditEventFromPatchedDocumentEntryAndJwt(
        AdditionalParameters additionalParameters,
        DocumentEntryDto? updatedEntry,
        JwtSecurityToken? token,
        string? documentReferenceId,
        List<CodedValue>? oldSecurityLabel,
        List<CodedValue>? newSecurityLabel)
    {
        var extrinsicObject = RegistryMetadataTransformer.TransformDocumentReferenceDtoListToRegistryObjects([updatedEntry])
            .OfType<ExtrinsicObjectType>()
            .FirstOrDefault();

        // For Patch (metadata update) we want the audit event to be categorized as Amend (Update).
        // The existing BALP mapping derives Update/Amend from an XDS Replace (RPLC) association.
        var targetObject = extrinsicObject?.Id
            ?? updatedEntry?.Id?.WithUrnUuid()
            ?? updatedEntry?.UniqueId
            ?? "urn:uuid:unknown";

        var association = new AssociationType
        {
            AssociationTypeData = Replace,
            SourceObject = $"urn:uuid:{Guid.NewGuid():D}",
            TargetObject = targetObject
        };

        IdentifiableType[] registryObjects = extrinsicObject == null
            ? [association]
            : [extrinsicObject, association];

        var soapEnvelope = _atnaLogEnricherService.GetMockSoapEnvelopeFromJwtAndBundle(additionalParameters, token?.RawData, null, registryObjects);

        // ITI-42 is used here to represent an update/amend of registry metadata.
        soapEnvelope.SetAction(Constants.Xds.OperationContract.Iti42Action);
        soapEnvelope.Header.MessageId = additionalParameters.TraceIdentifier;

        var responseEnvelope = new SoapEnvelope()
        {
            Body = new()
            {
                RegistryResponse = new()
                {
                    Status = Constants.Xds.ResponseStatusTypes.Success
                }
            }
        };

        var auditEvent = GetAuditEventFromSoapRequestResponse(soapEnvelope, responseEnvelope);

        // Add explicit patch details (document id + old/new securityLabel) as AuditEvent.entity.detail.
        if (!string.IsNullOrWhiteSpace(documentReferenceId) || (oldSecurityLabel?.Count > 0) || (newSecurityLabel?.Count > 0))
        {
            var detail = new List<AuditEvent.DetailComponent>();
            AddDetail(detail, "documentReference.id", documentReferenceId);
            AddDetail(detail, "securityLabel.old", FormatSecurityLabel(oldSecurityLabel));
            AddDetail(detail, "securityLabel.new", FormatSecurityLabel(newSecurityLabel));

            auditEvent.Entity.Add(new AuditEvent.EntityComponent
            {
                What = string.IsNullOrWhiteSpace(documentReferenceId)
                    ? new ResourceReference { Display = "DocumentReference" }
                    : new ResourceReference($"DocumentReference/{documentReferenceId.NoUrn()}", "DocumentReference"),
                Detail = detail
            });
        }

        return auditEvent;
    }

    private static string? FormatSecurityLabel(List<CodedValue>? codings)
    {
        if (codings == null || codings.Count == 0) return null;

        // Keep it human readable and compact in `AuditEvent.entity.detail.valueString`.
        // Example: "urn:oid:2.16...|TREAT|Treatment".
        return string.Join(", ", codings
            .Where(c => c != null)
            .Select(c => $"{c.CodeSystem}|{c.Code}|{c.DisplayName}".Trim('|')));
    }

    private static List<CodedValue>? CloneCodedValues(List<CodedValue>? src)
    {
        if (src == null) return null;
        return src.Select(c => new CodedValue
        {
            Code = c.Code,
            CodeSystem = c.CodeSystem,
            DisplayName = c.DisplayName
        }).ToList();
    }

    private static DocumentEntryDto? CloneDocumentEntryDto(DocumentEntryDto? src)
    {
        if (src == null) return null;

        return new DocumentEntryDto
        {
            Id = src.Id,
            UniqueId = src.UniqueId,
            AvailabilityStatus = src.AvailabilityStatus,
            Author = src.Author,
            ClassCode = src.ClassCode,
            ConfidentialityCode = CloneCodedValues(src.ConfidentialityCode),
            CreationTime = src.CreationTime,
            EventCodeList = src.EventCodeList,
            FormatCode = src.FormatCode,
            Hash = src.Hash,
            HealthCareFacilityTypeCode = src.HealthCareFacilityTypeCode,
            HomeCommunityId = src.HomeCommunityId,
            LanguageCode = src.LanguageCode,
            LegalAuthenticator = src.LegalAuthenticator,
            MimeType = src.MimeType,
            ObjectType = src.ObjectType,
            PracticeSettingCode = src.PracticeSettingCode,
            RepositoryUniqueId = src.RepositoryUniqueId,
            Size = src.Size,
            ServiceStartTime = src.ServiceStartTime,
            ServiceStopTime = src.ServiceStopTime,
            SourcePatientInfo = src.SourcePatientInfo,
            Title = src.Title,
            TypeCode = src.TypeCode
        };
    }

    private AuditEvent GetAuditEventFromSoapRequestResponse(SoapEnvelope requestEnvelope, SoapEnvelope? responseEnvelope)
    {
        var auditEvent = new AuditEvent();
        auditEvent.Id = Guid.NewGuid().ToString();

        var samlAssertionXml = requestEnvelope?.Header.Security?.Assertion?.OuterXml;
        Saml2SecurityToken? samlToken = null;
        List<Saml2Attribute>? statements = new();
        Issuer? issuer = null;

        if (!string.IsNullOrWhiteSpace(samlAssertionXml))
        {
            samlToken = SamlExtensions.ReadSamlToken(samlAssertionXml);
            statements = samlToken?.Assertion.Statements
                .OfType<Saml2AttributeStatement>()
                .SelectMany(statement => statement.Attributes)
                .ToList();

            issuer = SamlExtensions.GetIssuerEnumFromSamlTokenIssuer(samlToken?.Issuer);
        }

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
            What = new ResourceReference(requestEnvelope?.Header.MessageId, "SOAP message ID"),
        });

        if (samlToken != null)
        {
            var subjectNameRaw = statements?
                .FirstOrDefault(s => s.Name == Constants.Saml.Attribute.SubjectId)?.Values.FirstOrDefault();

            // Fix unicode escape sequences
            subjectNameRaw = JsonSerializer.Deserialize<string>($"\"{subjectNameRaw}\"");

            var subjectNameCoded = SamlExtensions.GetSamlAttributeValueAsCodedValue(subjectNameRaw);
            var subjectDisplayName = string.IsNullOrWhiteSpace(subjectNameCoded?.Code) ? null : subjectNameCoded.Code;

            var providerIdentifierValue = statements?
                .FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ProviderIdentifier)?.Values.FirstOrDefault();

            var providerIdentifierCoded = SamlExtensions.GetSamlAttributeValueAsCodedValue(providerIdentifierValue);

            var hasSubject = !string.IsNullOrWhiteSpace(subjectDisplayName) ||
                !string.IsNullOrWhiteSpace(providerIdentifierCoded?.Code) ||
                !string.IsNullOrWhiteSpace(providerIdentifierCoded?.CodeSystem);

            var subjectResource = GetSubjectResource(requestEnvelope, statements, subjectDisplayName, providerIdentifierCoded, issuer, hasSubject);

            if (subjectResource != null)
            {
                auditEvent.Contained.Add(subjectResource);
            }


            auditEvent.Entity.Add(new AuditEvent.EntityComponent()
            {
                What = subjectResource == null ? null : new ResourceReference($"#{subjectResource.Id}")
                {
                    Display = "patient"
                },
                Type = new Coding
                {
                    System = "http://terminology.hl7.org/CodeSystem/audit-entity-type",
                    Code = "1",
                    Display = "Person"
                },
                Role = new Coding
                {
                    System = "http://terminology.hl7.org/CodeSystem/object-role",
                    Code = "1",
                    Display = "Patient"
                }
            });


            var orgnrParent = statements?.FirstOrDefault(s => s.Name == "helseid://claims/client/claims/orgnr_parent")
                ?.Values
                .FirstOrDefault();
            var clientName = statements?.FirstOrDefault(s => s.Name == "helseid://claims/client/client_name")
                ?.Values
                .FirstOrDefault();
            var clientId = statements?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.EhelseClientId)
                ?.Values
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(orgnrParent) || !string.IsNullOrWhiteSpace(clientName) || !string.IsNullOrWhiteSpace(clientId))
            {
                var clientDetail = new List<AuditEvent.DetailComponent>();
                if (!string.IsNullOrWhiteSpace(orgnrParent))
                {
                    clientDetail.Add(new AuditEvent.DetailComponent { Type = "orgnr_parent", Value = new FhirString(orgnrParent) });
                }
                if (!string.IsNullOrWhiteSpace(clientId))
                {
                    clientDetail.Add(new AuditEvent.DetailComponent { Type = "client_id", Value = new FhirString(clientId) });
                }
                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    clientDetail.Add(new AuditEvent.DetailComponent { Type = "client_name", Value = new FhirString(clientName) });
                }

                auditEvent.Entity.Add(new AuditEvent.EntityComponent
                {
                    What = new ResourceReference { Display = "client" },
                    Role = new Coding
                    {
                        System = "http://terminology.hl7.org/CodeSystem/object-role",
                        Code = "13",
                        Display = "Security Resource"
                        //Code = "25",	// Is this a better code to use?
                        //Display = "Data Source"
                    },
                    Detail = clientDetail
                });
            }

            var purposeOfUseValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements?.FirstOrDefault(s => s.Name.IsAnyOf(Constants.Saml.Attribute.PurposeOfUse, Constants.Saml.Attribute.PurposeOfUse_Helsenorge))?.Values.FirstOrDefault());

            if (purposeOfUseValue != null && (!string.IsNullOrWhiteSpace(purposeOfUseValue.Code) || !string.IsNullOrWhiteSpace(purposeOfUseValue.CodeSystem) || !string.IsNullOrWhiteSpace(purposeOfUseValue.DisplayName)))
            {
                auditEvent.PurposeOfEvent = new List<CodeableConcept>()
                {
                    new CodeableConcept()
                    {
                        Coding = new List<Coding>()
                        {
                            new Coding()
                            {
                                Code = string.IsNullOrWhiteSpace(purposeOfUseValue.Code) ? null : purposeOfUseValue.Code,
                                System = string.IsNullOrWhiteSpace(purposeOfUseValue.CodeSystem) ? null : purposeOfUseValue.CodeSystem.WithUrnOid(),
                                Display = string.IsNullOrWhiteSpace(purposeOfUseValue.DisplayName) ? null : purposeOfUseValue.DisplayName
                            }
                        }
                    }
                };
            }


            if (issuer == Issuer.HelseId && hasSubject)
            {
                HumanName? healthcarePersonHumanName = null;
                var subjectNameParts = subjectDisplayName?.Split().ToList();
                if (subjectNameParts != null && subjectNameParts.Count > 0)
                {
                    healthcarePersonHumanName = new HumanName
                    {
                        Family = subjectNameParts.LastOrDefault(),
                    };

                    if (subjectNameParts.Count > 1)
                    {
                        healthcarePersonHumanName.Given = subjectNameParts.Take(subjectNameParts.Count - 1).ToList();
                    }
                }

                var subjectUser = new Practitioner
                {
                    Id = "practitioner-1",
                    Identifier = (providerIdentifierCoded != null && (!string.IsNullOrWhiteSpace(providerIdentifierCoded.Code) || !string.IsNullOrWhiteSpace(providerIdentifierCoded.CodeSystem)))
                        ? new List<Identifier>
                        {
                            new Identifier
                            {
                                Value = string.IsNullOrWhiteSpace(providerIdentifierCoded.Code) ? null : providerIdentifierCoded.Code,
                                System = string.IsNullOrWhiteSpace(providerIdentifierCoded.CodeSystem) ? null : providerIdentifierCoded.CodeSystem.WithUrnOid()
                            }
                        }
                        : null,
                    Name = healthcarePersonHumanName == null ? null : [healthcarePersonHumanName]
                };

                var practitionerRole = new PractitionerRole
                {
                    Id = "who-1",
                    Identifier = (!string.IsNullOrWhiteSpace(samlToken.Assertion.Subject.NameId?.Value) || !string.IsNullOrWhiteSpace(samlToken.Assertion.Issuer.Value))
                        ? new List<Identifier>
                        {
                            new Identifier
                            {
                                Value = string.IsNullOrWhiteSpace(samlToken.Assertion.Subject.NameId?.Value) ? null : samlToken.Assertion.Subject.NameId.Value,
                                System = string.IsNullOrWhiteSpace(samlToken.Assertion.Issuer.Value) ? null : samlToken.Assertion.Issuer.Value
                            }
                        }
                        : null
                };
                auditEvent.Contained.Add(practitionerRole);

                var pointOfCareStatement = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ChildOrganization)?.Values.FirstOrDefault());
                var pointOfCareName = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.TrustChildOrgName)?.Values.FirstOrDefault());

                var pointOfCare = new Organization()
                {
                    Id = "org-pointofcare-1",
                    Identifier = (pointOfCareStatement != null && (!string.IsNullOrWhiteSpace(pointOfCareStatement.Code) || !string.IsNullOrWhiteSpace(pointOfCareStatement.CodeSystem)))
                        ? new List<Identifier>()
                        {
                            new Identifier()
                            {
                                Value = string.IsNullOrWhiteSpace(pointOfCareStatement.Code) ? null : pointOfCareStatement.Code,
                                System = string.IsNullOrWhiteSpace(pointOfCareStatement.CodeSystem) ? null : pointOfCareStatement.CodeSystem.WithUrnOid()
                            }
                        }
                        : null,
                    Name = string.IsNullOrWhiteSpace(pointOfCareName?.Code) ? null : pointOfCareName.Code
                };
                auditEvent.Contained.Add(pointOfCare);
                practitionerRole.Location = [new ResourceReference() { Reference = $"#{pointOfCare.Id}" }];

                var legalEntityStatement = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.OrganizationId)?.Values.FirstOrDefault());
                var legalEntityName = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.Organization)?.Values.FirstOrDefault());

                var legalEntity = new Organization()
                {
                    Id = "org-legalentity-1",
                    Identifier = (legalEntityStatement != null && (!string.IsNullOrWhiteSpace(legalEntityStatement.Code) || !string.IsNullOrWhiteSpace(legalEntityStatement.CodeSystem)))
                        ? new List<Identifier>()
                        {
                            new Identifier()
                            {
                                Value = string.IsNullOrWhiteSpace(legalEntityStatement.Code) ? null : legalEntityStatement.Code,
                                System = string.IsNullOrWhiteSpace(legalEntityStatement.CodeSystem) ? null : legalEntityStatement.CodeSystem.WithUrnOid()
                            }
                        }
                        : null,
                    Name = string.IsNullOrWhiteSpace(legalEntityName?.Code) ? null : legalEntityName.Code
                };

                auditEvent.Contained.Add(legalEntity);
                practitionerRole.Organization = new ResourceReference($"#{legalEntity.Id}");

                auditEvent.Contained.Add(subjectUser);
                practitionerRole.Practitioner = new ResourceReference($"#{subjectUser.Id}");

                var agent = new AuditEvent.AgentComponent()
                {
                    Who = new ResourceReference($"#{practitionerRole.Id}")
                    {
                        Identifier = practitionerRole.Identifier?.FirstOrDefault()
                    },
                    Requestor = true,
                    PurposeOfUse = auditEvent.PurposeOfEvent,
                    Network = string.IsNullOrWhiteSpace(_appConfig.IpAddress)
                        ? null
                        : new AuditEvent.NetworkComponent()
                        {
                            Address = _appConfig.IpAddress
                        }
                };

                if (samlToken.Id.Length > 0)
                {
                    agent.Policy = [samlToken.Id];
                }
                auditEvent.Agent.Add(agent);
            }
            else
            {
                var agent = new AuditEvent.AgentComponent()
                {
                    Requestor = true,
                    Network = string.IsNullOrWhiteSpace(_appConfig.IpAddress)
                    ? null
                    : new AuditEvent.NetworkComponent()
                    {
                        Address = _appConfig.IpAddress
                    }
                };

                if (samlToken.Id.Length > 0)
                {
                    agent.Policy = [samlToken.Id];
                }
                auditEvent.Agent.Add(agent);
            }
        }

        auditEvent.Type = GetAuditEventTypeFromSoapEnvelope(requestEnvelope);
        auditEvent.Recorded = DateTimeOffset.Now;
        auditEvent.Outcome = GetEventOutcomeFromSoapRequestResponse(requestEnvelope, responseEnvelope);
        auditEvent.Action = GetActionFromSoapEnvelope(requestEnvelope);

        var detail = new List<AuditEvent.DetailComponent>();

        var adhocQueryType = requestEnvelope?.Body.AdhocQueryRequest?.AdhocQuery?.Id;
        var docRequest = requestEnvelope?.Body.ProvideAndRegisterDocumentSetRequest;
        var xdsDoc = docRequest?.Document?.FirstOrDefault();
        var rol = docRequest?.SubmitObjectsRequest?.RegistryObjectList;

        var xdsDocEntry = (DocumentEntryDto?)RegistryMetadataTransformer.TransformRegistryObjectsToRegistryObjectDtos(rol?.OfType<ExtrinsicObjectType>())?.FirstOrDefault();
        var xdsSubmissionSet = (SubmissionSetDto?)RegistryMetadataTransformer.TransformRegistryObjectsToRegistryObjectDtos(rol?.OfType<RegistryPackageType>())?.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(adhocQueryType))
        {
            auditEvent.Entity.Add(new AuditEvent.EntityComponent
            {
                What = new ResourceReference(adhocQueryType, "adhocQueryType")
            });
        }

        if (xdsDocEntry == null)
        {
            var retrieveDocumentsRequest = requestEnvelope?.Body?.RetrieveDocumentSetRequest?.DocumentRequest?.FirstOrDefault();
            xdsDocEntry = _registryWrapper.GetSingleRegistryObjectAsDto(retrieveDocumentsRequest?.DocumentUniqueId) as DocumentEntryDto;
        }

        if (xdsDocEntry != null)
        {
            var docUniqueId = xdsDoc?.Id ?? xdsDocEntry?.UniqueId;
            var reference = !string.IsNullOrWhiteSpace(docUniqueId) ? $"DocumentReference/{docUniqueId}" : null;

            var title = xdsDocEntry?.Title;
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Clinical document";
            }

            var mimeType = xdsDocEntry?.MimeType;
            var classCode = xdsDocEntry?.ObjectType;

            AddDetail(detail, "documentUniqueId", docUniqueId);
            AddDetail(detail, "mimeType", mimeType);
            AddDetail(detail, "classCode", classCode);
            AddDetail(detail, "title", title);
            AddDetail(detail, "homeCommunityId", _appConfig.HomeCommunityId);

            var submissionSetId = xdsSubmissionSet?.Id;
            AddDetail(detail, "submissionSetId", submissionSetId);

        }
        else
        {
            auditEvent.Agent.Add(new AuditEvent.AgentComponent()
            {
                Requestor = true,
                Network = string.IsNullOrWhiteSpace(_appConfig.IpAddress)
                    ? null
                    : new AuditEvent.NetworkComponent()
                    {
                        Address = _appConfig.IpAddress
                    }
            });
        }

        var device = new Device
        {
            Id = "device-1",
            Identifier = new List<Identifier>()
        };

        var repoUid = _appConfig.RepositoryUniqueId?.WithUrnOid();
        if (!string.IsNullOrWhiteSpace(repoUid))
        {
            device.Identifier.Add(new Identifier(Constants.Oid.System, repoUid)
            {
                Type = new CodeableConcept { Text = "repositoryUniqueId" }
            });
        }

        var homeCid = _appConfig.HomeCommunityId?.WithUrnOid();
        if (!string.IsNullOrWhiteSpace(homeCid))
        {
            device.Identifier.Add(new Identifier(Constants.Oid.System, homeCid)
            {
                Type = new CodeableConcept { Text = "homeCommunityId" }
            });
        }

        // If you have a legal entity Organization from SAML, link it to the Device as owner
        var legalEntityOrgRef = auditEvent.Contained
            .OfType<PractitionerRole>()
            .FirstOrDefault()?
            .Organization;

        if (legalEntityOrgRef != null)
        {
            device.Owner = legalEntityOrgRef;
        }

        auditEvent.Contained.Add(device);

        // Source.Observer should be the system that produced/detected the event (the Device).
        auditEvent.Source = new AuditEvent.SourceComponent
        {
            Observer = new ResourceReference($"#{device.Id}"),
            Type = new List<Coding>
            {
                new Coding
                {
                    Code = "3",
                    System = "http://terminology.hl7.org/CodeSystem/security-source-type",
                    Display = "Web Server"
                }
            }
        };

        return auditEvent;
    }

    private Resource? GetSubjectResource(SoapEnvelope? requestEnvelope, List<Saml2Attribute>? statements, string? subjectDisplayName, CodedValue? providerIdentifierCoded, Issuer? issuer, bool hasSubject)
    {
        // is_provide_bundle, patient_given and patient_family are custom attributes added only by
        // FhirMobileAccessToHealthDocumentsController.ProvideBundle method
        var isProvideBundle = statements?
            .FirstOrDefault(s => s.Name == "is_provide_bundle")?.Values.FirstOrDefault() == "true";

        string? patientGiven = null;
        string? patientFamily = null;
        XPN? subjectNameXpn = null;

        if (isProvideBundle)
        {
            patientGiven = statements?
                .FirstOrDefault(s => s.Name == "patient_given")?.Values.FirstOrDefault();

            patientFamily = statements?
                .FirstOrDefault(s => s.Name == "patient_family")?.Values.FirstOrDefault();

            subjectNameXpn = new(patientGiven, patientFamily);
        }
        else
        {
            var subjectName = subjectDisplayName?.Split() is { Length: >= 2 } namePart
                ? subjectNameXpn = new(namePart.FirstOrDefault(), string.Join(' ', namePart.Skip(1)))
                : subjectNameXpn = new(subjectDisplayName, null);
        }

        var registryPatientIdentifiers = GetRegistryPatientIdentifierForRequest(requestEnvelope).ToList();

        var patientResourceId = statements?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ResourceId20)?.Values.FirstOrDefault();
        var subjectId = statements?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ProviderIdentifier)?.Values.FirstOrDefault();

        var resourceIdCx = Hl7Object.Parse<CX>(patientResourceId);
        var subjectIdCx = SamlExtensions.GetSamlAttributeValueAsCx(subjectId);

        if (!string.IsNullOrWhiteSpace(resourceIdCx?.IdNumber))
        {
            var subjectPid = new PID(resourceIdCx, subjectNameXpn);
            registryPatientIdentifiers.Add(subjectPid);
        }

        var allPatientIdentifiers = registryPatientIdentifiers
            .DistinctBy(pid => new { pid?.PatientIdentifier?.IdNumber, pid?.PatientIdentifier?.AssigningAuthority?.UniversalId })
            .OfType<PID>()
            .ToList();

        // The SAML-token contains a subject identifier (providerIdentifier) and a resource identifier (resourceId)
        // In a normal Helsenorge-scenario, the identifiers are similar, because the subject is opening documents on themself.
        // However, when a person - in Helsenorge, with Power Of Attorney (representasjonsforhold) - opens documents on someones behalf,
        // the SAML token will only contain the identifier of the queried patient (resource-id), and not their name.
        // Therfore we need to check this to avoid incorrectly resolving the subject name  to the resource Id.
        var requestIsForAnotherPerson = subjectIdCx != null && subjectIdCx.Equals(resourceIdCx) == false;

        Patient? patientResource = null;

        if (allPatientIdentifiers.Count > 0 == false) return patientResource;

        patientResource = new Patient
        {
            Id = "patient-1",
        };

        _logger.LogDebug($"AtnaLogGenerator Resolved {allPatientIdentifiers.Count} identifiers from request");

        foreach (var identifier in allPatientIdentifiers)
        {
            if (identifier == null || string.IsNullOrWhiteSpace(identifier.PatientIdentifier?.AssigningAuthority?.UniversalId) || string.IsNullOrWhiteSpace(identifier.PatientIdentifier?.IdNumber)) continue;

            _logger.LogDebug($"AtnaLogGenerator found Patient Identifier: {identifier?.Serialize()}");
            patientResource.Identifier.Add(new Identifier(identifier?.PatientIdentifier?.AssigningAuthority?.UniversalId.WithUrnOid(), identifier?.PatientIdentifier?.IdNumber));
        }

        if (isProvideBundle)
        {
            // For ProvideBundle calls, we have explicit given/family names, because the following scenarios are possible:
            // - JWT (machine-to-machine) token without any logged in user. Patient exists only in the Bundle and is added to SAML assertion from there. There is no subject in the assertion.
            // - JWT (HelseID user token) token where the logged in user is healthcare professional.
            //		The patient exists only in the Bundle and is added to SAML assertion from there. The subject in the assertion will be the healthcare professional, not the patient.

            if (!string.IsNullOrWhiteSpace(patientFamily) || !string.IsNullOrWhiteSpace(patientGiven))
            {
                var patientHumanName = new HumanName
                {
                    Family = string.IsNullOrWhiteSpace(patientFamily) ? null : patientFamily,
                };

                if (!string.IsNullOrWhiteSpace(patientGiven))
                {
                    patientHumanName.Given = [patientGiven];
                }

                patientResource.Name = [patientHumanName];
            }
        }
        else if (issuer == Issuer.Helsenorge && hasSubject && requestIsForAnotherPerson == false)
        {
            var patientHumanName = new HumanName
            {
                Given = [subjectNameXpn.GivenName],
            };

            patientHumanName.Family = subjectNameXpn.FamilyName;

            patientResource.Name = [patientHumanName];
        }

        return patientResource;
    }

    void AddDetail(List<AuditEvent.DetailComponent> detail, string type, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        detail.Add(new AuditEvent.DetailComponent { Type = type, Value = new FhirString(value) });
    }


    /// <summary>
    /// Get the patient identifier related to the registry objects being queried or stored
    /// </summary>
    private PID[] GetRegistryPatientIdentifierForRequest(SoapEnvelope? requestEnvelope)
    {
        if (requestEnvelope == null) return [];

        // ITI-38 or ITI-18 AdhocQuery FindDocuments 
        var requestPatientIdentifier = requestEnvelope.Body.AdhocQueryRequest?.AdhocQuery?
            .GetFirstSlot(Constants.Xds.QueryParameters.FindDocuments.PatientId)?.GetFirstValue();

        if (requestPatientIdentifier != null)
        {
            _logger.LogDebug(requestPatientIdentifier, "Found patient identifier in AdhocQueryRequest");
            return [new() { PatientIdentifier = Hl7Object.Parse<CX>(requestPatientIdentifier) }];
        }

        // ITI-41 or ITI-42
        // HAYO! DeleteDocuments_Jank! ITI-86 or ITI-62 DeleteDocumentSet
        var provideAndRegister = requestEnvelope.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList ?? requestEnvelope.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList;

        if (provideAndRegister != null)
        {
            var registryObjects = RegistryMetadataTransformer.TransformRegistryObjectsToRegistryObjectDtos(provideAndRegister).ToArray();

            return PatientIdPidFromDocumentEntries(registryObjects) ?? [];
        }

        // ITI-39 or ITI-43 RetrieveDocumentSet
        var retrieveDocumentRequest = requestEnvelope.Body.RetrieveDocumentSetRequest;

        if (retrieveDocumentRequest != null)
        {
            var docEntryUniqueId = retrieveDocumentRequest.DocumentRequest;

            if (docEntryUniqueId?.Length < 0)
            {
                var ids = new HashSet<string>(docEntryUniqueId.Select(x => x.DocumentUniqueId ?? string.Empty));

                var registryObjects = ids.Select(_registryWrapper.GetSingleRegistryObjectAsDto).OfType<RegistryObjectDto>().ToArray();


                return PatientIdPidFromDocumentEntries(registryObjects) ?? [];
            }
        }

        return [];
    }

    private PID[]? PatientIdPidFromDocumentEntries(RegistryObjectDto[]? registryObjects)
    {
        if (registryObjects == null || registryObjects.Length == 0) return null;

        var pids = new List<PID>();

        foreach (var registryObject in registryObjects)
        {
            if (registryObject is DocumentEntryDto documentEntry && documentEntry.SourcePatientInfo is { } pid)
            {
                var patientId = new PID();
                if (string.IsNullOrWhiteSpace(pid.FirstName) || string.IsNullOrWhiteSpace(pid.LastName))
                {
                    patientId.PatientName = new(pid.FirstName, pid.LastName);
                }

                if (string.IsNullOrWhiteSpace(pid.PatientId?.Id) || string.IsNullOrWhiteSpace(pid.PatientId?.System))
                {
                    patientId.PatientIdentifier = new(pid.PatientId?.Id, pid.PatientId?.System);
                }
            }
        }

        return pids.OfType<PID>().ToArray();
    }

    private AuditEvent.AuditEventAction? GetActionFromSoapEnvelope(SoapEnvelope? requestEnvelope)
    {
        // Override for $validate-endpoint on FHIR, 
        if (RequestIsExecute(requestEnvelope)) return AuditEvent.AuditEventAction.E;

        switch (requestEnvelope?.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
            case Constants.Xds.OperationContract.Iti38Action:
            case Constants.Xds.OperationContract.Iti43Action:
            case Constants.Xds.OperationContract.Iti39Action:
                return AuditEvent.AuditEventAction.R;

            case Constants.Xds.OperationContract.Iti41Action:
            case Constants.Xds.OperationContract.Iti42Action:
                return GetCreateOrUpdateFromRequest(requestEnvelope);

            case Constants.Xds.OperationContract.Iti86Action:
            case Constants.Xds.OperationContract.Iti62Action:
                return AuditEvent.AuditEventAction.D;

            default:
                return AuditEvent.AuditEventAction.R;
        }
    }

    private bool RequestIsExecute(SoapEnvelope? requestEnvelope)
    {
        List<Saml2Attribute>? statements = new();
        Saml2SecurityToken? samlToken = null;

        var samlTokenRaw = requestEnvelope?.Header.Security?.Assertion?.OuterXml;

        if (!string.IsNullOrWhiteSpace(samlTokenRaw))
        {
            samlToken = SamlExtensions.ReadSamlToken(samlTokenRaw);

            statements = samlToken?.Assertion.Statements
            .OfType<Saml2AttributeStatement>()
            .SelectMany(statement => statement.Attributes)
            .ToList();
        }

        return statements?.FirstOrDefault(s => s.Name == "is_validate_resource")?.Values
            .FirstOrDefault() == "true";
    }

    private AuditEvent.AuditEventAction? GetCreateOrUpdateFromRequest(SoapEnvelope requestEnvelope)
    {
        var registryObjects = requestEnvelope.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList;

        var isReplaceUpdate = registryObjects?.OfType<AssociationType>()
            .Any(assoc => assoc.AssociationTypeData?.IsAnyOf(Replace, Transformation, Addendum, ReplaceWithTransformation) == true) ?? false;

        return isReplaceUpdate ? AuditEvent.AuditEventAction.U : AuditEvent.AuditEventAction.C;
    }

    private AuditEvent.AuditEventOutcome GetEventOutcomeFromSoapRequestResponse(SoapEnvelope? requestEnvelope, SoapEnvelope? responseEnvelope)
    {
        var registryErrors = responseEnvelope?.Body.RegistryResponse?.RegistryErrorList?.RegistryError;
        var soapFault = responseEnvelope?.Body.Fault;

        // If we don't even have a Soap request or response, or if there is a SOAP fault, consider it a major failure (N8)
        if (requestEnvelope == null || responseEnvelope == null || soapFault != null)
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
    private Coding GetAuditEventTypeFromSoapEnvelope(SoapEnvelope? requestEnvelope)
    {
        var action = requestEnvelope?.Header.Action;

        if (RequestIsExecute(requestEnvelope)) return new Coding()
        {
            Code = "verify",
            System = Constants.CodeSystems.Hl7.Lifecycle.IsoHealthRecordLifecycleEvent,
            Display = "Verify Record Lifecycle Event"
        };

        switch (action)
        {
            case Constants.Xds.OperationContract.Iti18Action:
            case Constants.Xds.OperationContract.Iti38Action:
            case Constants.Xds.OperationContract.Iti43Action:
            case Constants.Xds.OperationContract.Iti39Action:
                return new Coding()
                {
                    Code = "access",
                    System = Constants.CodeSystems.Hl7.Lifecycle.IsoHealthRecordLifecycleEvent,
                    Display = "Access/View Record Lifecycle Event"
                };

            case Constants.Xds.OperationContract.Iti41Action:
            case Constants.Xds.OperationContract.Iti42Action:
                var associations = requestEnvelope?.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList?.OfType<AssociationType>();

                var type = associations switch
                {
                    var ass when ass != null && ass.Any(assoc => assoc.AssociationTypeData.IsAnyOf(Transformation, ReplaceWithTransformation))
                        => "transform",

                    var ass when ass != null && ass.Any(assoc => assoc.AssociationTypeData.IsAnyOf(Replace))
                        => "amend",

                    _
                        => "originate"
                };


                return type switch
                {
                    "transform" => new Coding()
                    {
                        Code = "transform",
                        System = Constants.CodeSystems.Hl7.Lifecycle.IsoHealthRecordLifecycleEvent,
                        Display = "Transform/Translate Record Lifecycle Event"
                    },

                    "amend" => new Coding()
                    {
                        Code = "amend",
                        System = Constants.CodeSystems.Hl7.Lifecycle.IsoHealthRecordLifecycleEvent,
                        Display = "Amend (Update) Record Lifecycle Event"
                    },

                    // Default: Assume Hasmember/addition
                    _ => new Coding()
                    {
                        Code = "originate",
                        System = Constants.CodeSystems.Hl7.Lifecycle.IsoHealthRecordLifecycleEvent,
                        Display = "Originate/Retain Record Lifecycle Event"
                    }

                };

            case Constants.Xds.OperationContract.Iti86Action:
            case Constants.Xds.OperationContract.Iti62Action:
                return new Coding()
                {
                    Code = "destroy",
                    System = Constants.CodeSystems.Hl7.Lifecycle.IsoHealthRecordLifecycleEvent,
                    Display = "Destroy/Delete Record Lifecycle Event"
                };

            default:
                return new Coding()
                {
                    Code = "access",
                    System = Constants.CodeSystems.Hl7.Lifecycle.IsoHealthRecordLifecycleEvent,
                    Display = "Access/View Record Lifecycle Event"
                };
        }
    }
}