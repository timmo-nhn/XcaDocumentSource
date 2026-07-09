using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.DataManipulators.Fhir;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;
using XcaXds.Shared.Enums;
using XcaXds.Shared.Extensions;
using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Services;
using XcaXds.WebService.Services.PolicyEnforcementPoint;
using XcaXds.WebService.Services.XdsRegistry;
using static XcaXds.Shared.Constants.Xds.AssociationType;

namespace XcaXds.WebService.Services.AtnaAuditLogging;

public class AtnaLogGeneratorService
{
    private readonly ILogger<AtnaLogGeneratorService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly IAtnaLogQueue _queue;
    private readonly RegistryWrapper _registryWrapper;
    private readonly AtnaLogEnricherService _atnaLogEnricherService;
    private readonly TerminologyService _terminologyService;
    private readonly RegistryMetadataTransformerService _registryMetadataTransformerService;

    public const string IsoHealthRecordLifecycleEvent = "http://terminology.hl7.org/CodeSystem/iso-21089-lifecycle";

    public AtnaLogGeneratorService(
        ILogger<AtnaLogGeneratorService> logger,
        ApplicationConfig appConfig,
        IAtnaLogQueue queue,
        RegistryWrapper registryWrapper,
        AtnaLogEnricherService atnaLogEnricherService,
        TerminologyService terminologyService,
        RegistryMetadataTransformerService registryMetadataTransformerService)
    {
        _logger = logger;
        _appConfig = appConfig;
        _queue = queue;
        _registryWrapper = registryWrapper;
        _atnaLogEnricherService = atnaLogEnricherService;
        _terminologyService = terminologyService;
        _registryMetadataTransformerService = registryMetadataTransformerService;
    }

    public void CreateAuditLogForSoapRequestResponse(AdditionalParameters additionalParameters, SoapEnvelope requestEnvelope, SoapEnvelope? responseEnvelope)
    {
        _queue.Enqueue(() => GetAuditEventFromSoapRequestResponse(additionalParameters, requestEnvelope, responseEnvelope));
    }

    public void CreateAuditLogForFhirDeleteDocumentsRequest(AdditionalParameters additionalParameters,
        OperationOutcome operationOutcome, JwtSecurityToken? token)
    {
        _queue.Enqueue(() => GetAuditEventFromDocumentEntryOperationOutcomeAndJwt(additionalParameters, operationOutcome, token));
    }

    public void CreateAuditLogForFhirPatchDocumentSecurityLabelRequest(AdditionalParameters httpContext,
        List<CodedValue>? oldSecurityLabel, DocumentEntryDto? updatedEntry, JwtSecurityToken? token)
    {
        var newSecurityLabel = updatedEntry?.ConfidentialityCode;
        // Clone to avoid mutation after we enqueue (controller updates the same DTO instance).
        var oldCopy = CloneCodedValues(oldSecurityLabel);
        var newCopy = CloneCodedValues(newSecurityLabel);
        var updatedCopy = CloneDocumentEntryDto(updatedEntry);

        _queue.Enqueue(() => GetAuditEventFromPatchedDocumentEntryAndJwt(httpContext, updatedCopy, token, updatedEntry?.Id, oldCopy, newCopy));
    }

    private AuditEvent GetAuditEventFromDocumentEntryOperationOutcomeAndJwt(AdditionalParameters additionalParameters,
        OperationOutcome operationOutcome, JwtSecurityToken? token)
    {
        var documentId = operationOutcome.Issue.FirstOrDefault()?.Location.FirstOrDefault();

        // If a resource that doesnt exist was attempted to be deleted
        var deletedEntry = additionalParameters.DeletedRegistryObjects;

        if (deletedEntry == null && documentId != null)
        {
            deletedEntry = [new DocumentEntryDto() { Id = documentId }];
        }

        var deletedRegistryObjects = RegistryMetadataTransformerService
            .TransformDocumentReferenceDtoListToRegistryObjectsStateless(deletedEntry);

        var soapEnvelope = _atnaLogEnricherService.GetMockSoapEnvelopeFromJwtAndBundle(
            additionalParameters, 
            token?.RawData, 
            null,
            null);

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

        var auditEvent = GetAuditEventFromSoapRequestResponse(additionalParameters, soapEnvelope, responseEnvelope);

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
        var extrinsicObject = RegistryMetadataTransformerService
            .TransformDocumentReferenceDtoListToRegistryObjectsStateless([updatedEntry])
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

        var soapEnvelope =
            _atnaLogEnricherService.GetMockSoapEnvelopeFromJwtAndBundle(additionalParameters, token?.RawData, null,
                registryObjects);

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

        var auditEvent = GetAuditEventFromSoapRequestResponse(additionalParameters, soapEnvelope, responseEnvelope);

        // Add explicit patch details (document id + old/new securityLabel) as AuditEvent.entity.detail.
        if (!string.IsNullOrWhiteSpace(documentReferenceId) || (oldSecurityLabel?.Count > 0) ||
            (newSecurityLabel?.Count > 0))
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

    private AuditEvent GetAuditEventFromSoapRequestResponse(AdditionalParameters additionalParameters, SoapEnvelope requestEnvelope, SoapEnvelope? responseEnvelope)
    {
        var auditEvent = new AuditEvent()
        {
            Id = Guid.NewGuid().ToString()
        };

        var samlAssertionXml = requestEnvelope?.Header.Security?.Assertion?.OuterXml;
        Saml2SecurityToken? samlToken = null;
        List<Saml2Attribute>? statements = new();
        AppliesTo? issuer = null;

        if (!string.IsNullOrWhiteSpace(samlAssertionXml))
        {
            samlToken = SamlExtensions.ReadSamlToken(samlAssertionXml);
            statements = samlToken?.GetAllStatements().ToList();

            issuer = SamlExtensions.GetIssuerEnumFromSamlToken(samlToken);
        }

        auditEvent.Meta = new Meta()
        {
            Profile =
            [
                "https://profiles.ihe.net/ITI/BALP/StructureDefinition/IHE.BasicAudit.SAMLaccessTokenUse.Minimal"
            ],
            Security = new List<Coding>()
            {
                new Coding()
                {
                    Code = "HTEST",
                    System = $"http://terminology.hl7.org/CodeSystem/v3-ActReason"
                }
            }
        };

        auditEvent.Entity.Add(new AuditEvent.EntityComponent()
        {
            What = new ResourceReference(requestEnvelope?.Header.MessageId, "SOAP message ID"),
        });

        if (samlToken != null && requestEnvelope != null && statements != null)
        {
            AddSamlTokenValuesToAuditEvent(auditEvent, requestEnvelope, samlToken, statements, issuer, additionalParameters);
        }

        auditEvent.Type = GetAuditEventTypeFromSoapEnvelope(requestEnvelope, additionalParameters);
        auditEvent.Recorded = DateTimeOffset.Now;
        auditEvent.Outcome = GetAuditEventOutcomeFromSoapRequestResponse(responseEnvelope);
        auditEvent.OutcomeDesc = GetAuditEventOutcomeDescriptionFromSoapRequestResponse(responseEnvelope, additionalParameters.AccessControlResponse, additionalParameters.AppliedBusinessLogic);
        auditEvent.Action = GetAuditEventActionFromSoapEnvelope(requestEnvelope, additionalParameters);

        var adhocQueryType = requestEnvelope?.Body.AdhocQueryRequest?.AdhocQuery?.Id;
        var docRequest = requestEnvelope?.Body.ProvideAndRegisterDocumentSetRequest;
        var rol = docRequest?.SubmitObjectsRequest?.RegistryObjectList ?? 
            RegistryMetadataTransformerService.TransformDocumentReferenceDtoListToRegistryObjectsStateless(additionalParameters.DeletedRegistryObjects);
        var soapAction = requestEnvelope?.Header.Action;

        var documentEntry = (DocumentEntryDto?)_registryMetadataTransformerService
            .TransformRegistryObjectsToRegistryObjectDtos(rol?.OfType<ExtrinsicObjectType>())?.FirstOrDefault();
        var submissionSet = (SubmissionSetDto?)_registryMetadataTransformerService
            .TransformRegistryObjectsToRegistryObjectDtos(rol?.OfType<RegistryPackageType>())?.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(adhocQueryType))
        {
            auditEvent.Entity.Add(new AuditEvent.EntityComponent
            {
                What = new ResourceReference(adhocQueryType, "adhocQueryType")
            });
        }

        if (!string.IsNullOrWhiteSpace(soapAction))
        {
            var (code, display) = SoapExtensions.GetTransactionCodeFromSoapAction(soapAction);
            auditEvent.Subtype.Add(new Coding(Constants.Xds.OperationContract.System, code)
            {
                Display = display
            });
        }

        if (documentEntry == null)
        {
            var retrieveDocumentsRequest = requestEnvelope?.Body?.RetrieveDocumentSetRequest?.DocumentRequest?.FirstOrDefault();
            documentEntry = _registryWrapper.GetSingleRegistryObjectAsDto(retrieveDocumentsRequest?.DocumentUniqueId) as DocumentEntryDto;
        }

        if (documentEntry != null)
        {
            var docUniqueId = documentEntry?.UniqueId;
            var docId = documentEntry?.Id;

            var title = documentEntry?.Title;
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Untitled clinical document";
            }

            var mimeType = documentEntry?.MimeType;
            var classCode = documentEntry?.ObjectType;
            var homeCommunityId = _appConfig.HomeCommunityId;
            var submissionSetId = submissionSet?.Id;
            var sourceId = submissionSet?.SourceId;

            var detail = new List<AuditEvent.DetailComponent>();

            AddDetail(detail, "documentUniqueId", docUniqueId);
            AddDetail(detail, "mimeType", mimeType);
            AddDetail(detail, "classCode", classCode);
            AddDetail(detail, "title", title);
            AddDetail(detail, "homeCommunityId", homeCommunityId);
            AddDetail(detail, "submissionSetId", submissionSetId);
            AddDetail(detail, "sourceId", sourceId);

            auditEvent.Entity.Add(new AuditEvent.EntityComponent
            {
                What = new ResourceReference("DocumentReference/" + docId, title),
                Type = new Coding()
                {
                    Code = "2",
                    System = "http://terminology.hl7.org/CodeSystem/audit-entity-type",
                    Display = "System Object"
                },
                Role = new Coding()
                {
                    Code = "3",
                    System = "http://terminology.hl7.org/CodeSystem/object-role",
                    Display = "Report"
                },
                Detail = detail,
            });
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

    private void AddSamlTokenValuesToAuditEvent(AuditEvent auditEvent, SoapEnvelope requestEnvelope, Saml2SecurityToken samlToken, List<Saml2Attribute> statements, AppliesTo? issuer, AdditionalParameters additionalParameters)
    {

        // Fix Unicode escape sequences
        var subjectNameRaw = statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.SubjectId)?.Values.FirstOrDefault();
        subjectNameRaw = JsonSerializer.Deserialize<string>($"\"{subjectNameRaw}\"");

        var subjectNameCoded = SamlExtensions.GetSamlAttributeValueAsCodedValue(subjectNameRaw);
        var subjectDisplayName = string.IsNullOrWhiteSpace(subjectNameCoded?.Code) ? null : subjectNameCoded.Code;

        var providerIdentifierValue = statements.GetValue(Constants.Saml.Attribute.ProviderIdentifier);

        var providerIdentifierCoded = SamlExtensions.GetSamlAttributeValueAsCodedValue(providerIdentifierValue);

        var hasSubject = !string.IsNullOrWhiteSpace(subjectDisplayName) ||
                         !string.IsNullOrWhiteSpace(providerIdentifierCoded?.Code) ||
                         !string.IsNullOrWhiteSpace(providerIdentifierCoded?.CodeSystem);

        if (issuer == AppliesTo.Helsenorge)
        {
            var subjectResource = GetSubjectResource(statements, subjectDisplayName, issuer, hasSubject, additionalParameters);

            if (subjectResource?.Identifier != null)
            {
                auditEvent.Contained.Add(subjectResource);

                auditEvent.Entity.Add(new AuditEvent.EntityComponent()
                {
                    What = subjectResource == null
                        ? null
                        : new ResourceReference($"#{subjectResource.Id}")
                        {
                            Display = "user"
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
                        Code = "6",
                        Display = "User"
                    },
                });
            }
        }

        var resourceResource = GetResourceResource(requestEnvelope, statements, additionalParameters);

        if (resourceResource != null)
        {
            auditEvent.Contained.Add(resourceResource);

            auditEvent.Entity.Add(new AuditEvent.EntityComponent()
            {
                What = resourceResource == null
                    ? null
                    : new ResourceReference($"#{resourceResource.Id}")
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
                },
            });
        }


        var orgnrParent = statements?.FirstOrDefault(s => s.Name == "helseid://claims/client/claims/orgnr_parent")
            ?.Values
            .FirstOrDefault();
        var clientName = statements?.FirstOrDefault(s => s.Name == "helseid://claims/client/client_name")
            ?.Values
            .FirstOrDefault();
        var clientId = statements?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.EhelseClientId)
            ?.Values
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(orgnrParent) || !string.IsNullOrWhiteSpace(clientName) ||
            !string.IsNullOrWhiteSpace(clientId))
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

        var purposeOfUseValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements
            ?.FirstOrDefault(s => s.Name.IsAnyOf(
                Constants.Saml.Attribute.PurposeOfUse,
                Constants.Saml.Attribute.PurposeOfUse_Helsenorge))
            ?.Values
             .FirstOrDefault());

        var purposeOfUseIsNotNull = purposeOfUseValue != null && (!string.IsNullOrWhiteSpace(purposeOfUseValue.Code) || !string.IsNullOrWhiteSpace(purposeOfUseValue.CodeSystem) || !string.IsNullOrWhiteSpace(purposeOfUseValue.DisplayName));

        if (purposeOfUseIsNotNull)
        {
            auditEvent.PurposeOfEvent = new List<CodeableConcept>()
            {
                new CodeableConcept()
                {
                    Coding = new List<Coding>()
                    {
                        new Coding()
                        {
                            Code = purposeOfUseValue!.Code,
                            System = purposeOfUseValue.CodeSystem?.WithUrnOid(),
                            Display = purposeOfUseValue.DisplayName
                        }
                    }
                }
            };
        }

        if (issuer == AppliesTo.HelseId && hasSubject)
        {
            HumanName? healthcarePersonHumanName = null;
            var subjectNameParts = subjectDisplayName?.Split().ToList();
            if (subjectNameParts is { Count: > 0 })
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

            var practitionerUser = new Practitioner
            {
                Id = "practitioner-1",
                Identifier = (providerIdentifierCoded != null &&
                              (!string.IsNullOrWhiteSpace(providerIdentifierCoded.Code) ||
                               !string.IsNullOrWhiteSpace(providerIdentifierCoded.CodeSystem)))
                    ? new List<Identifier>
                    {
                        new Identifier
                        {
                            Value = string.IsNullOrWhiteSpace(providerIdentifierCoded.Code)
                                ? null
                                : providerIdentifierCoded.Code,
                            System = string.IsNullOrWhiteSpace(providerIdentifierCoded.CodeSystem)
                                ? null
                                : providerIdentifierCoded.CodeSystem.WithUrnOid()
                        }
                    }
                    : null,
                Name = healthcarePersonHumanName == null ? null : [healthcarePersonHumanName]
            };

            auditEvent.Contained.Add(practitionerUser);
            auditEvent.Entity.Add(new AuditEvent.EntityComponent()
            {
                What = new ResourceReference($"#{practitionerUser.Id}")
                {
                    Display = "user"
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
                    Code = "15",
                    Display = "Practitioner"
                },
            });

            var practitionerRole = new PractitionerRole
            {
                Id = "who-1",
                Identifier = (!string.IsNullOrWhiteSpace(samlToken.Assertion.Subject.NameId?.Value) ||
                              !string.IsNullOrWhiteSpace(samlToken.Assertion.Issuer.Value))
                    ? new List<Identifier>
                    {
                        new Identifier
                        {
                            Value = string.IsNullOrWhiteSpace(samlToken.Assertion.Subject.NameId?.Value)
                                ? null
                                : samlToken.Assertion.Subject.NameId.Value,
                            System = string.IsNullOrWhiteSpace(samlToken.Assertion.Issuer.Value)
                                ? null
                                : samlToken.Assertion.Issuer.Value
                        }
                    }
                    : null,
                Practitioner = new ResourceReference($"#{practitionerUser.Id}")
            };

            auditEvent.Contained.Add(practitionerRole);

            var pointOfCareStatement = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements
                ?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ChildOrganization)?.Values
                .FirstOrDefault());

            var pointOfCareName = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements
                ?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.TrustChildOrgName)?.Values
                .FirstOrDefault());

            var pointOfCare = new Organization()
            {
                Id = "org-pointofcare-1",
                Identifier = (pointOfCareStatement != null &&
                              (!string.IsNullOrWhiteSpace(pointOfCareStatement.Code) ||
                               !string.IsNullOrWhiteSpace(pointOfCareStatement.CodeSystem)))
                    ? new List<Identifier>()
                    {
                        new Identifier()
                        {
                            Value = string.IsNullOrWhiteSpace(pointOfCareStatement.Code)
                                ? null
                                : pointOfCareStatement.Code,
                            System = string.IsNullOrWhiteSpace(pointOfCareStatement.CodeSystem)
                                ? null
                                : pointOfCareStatement.CodeSystem.WithUrnOid()
                        }
                    }
                    : null,
                Name = string.IsNullOrWhiteSpace(pointOfCareName?.Code) ? null : pointOfCareName.Code
            };
            auditEvent.Contained.Add(pointOfCare);
            practitionerRole.Location = [new ResourceReference() { Reference = $"#{pointOfCare.Id}" }];

            var legalEntityStatement = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements
                ?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.OrganizationId)?.Values.FirstOrDefault());
            var legalEntityName = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements
                ?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.Organization)?.Values.FirstOrDefault());

            var legalEntity = new Organization()
            {
                Id = "org-legalentity-1",
                Identifier = (legalEntityStatement != null &&
                              (!string.IsNullOrWhiteSpace(legalEntityStatement.Code) ||
                               !string.IsNullOrWhiteSpace(legalEntityStatement.CodeSystem)))
                    ? new List<Identifier>()
                    {
                        new Identifier()
                        {
                            Value = string.IsNullOrWhiteSpace(legalEntityStatement.Code)
                                ? null
                                : legalEntityStatement.Code,
                            System = string.IsNullOrWhiteSpace(legalEntityStatement.CodeSystem)
                                ? null
                                : legalEntityStatement.CodeSystem.WithUrnOid()
                        }
                    }
                    : null,
                Name = string.IsNullOrWhiteSpace(legalEntityName?.Code) ? null : legalEntityName.Code
            };

            auditEvent.Contained.Add(legalEntity);
            practitionerRole.Organization = new ResourceReference($"#{legalEntity.Id}");

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

    private static string? GetAuditEventOutcomeDescriptionFromSoapRequestResponse(SoapEnvelope? responseEnvelope, AccessControlResponse? accessControlresponse, Dictionary<string, int>? appliedBusinessLogic)
    {
        var registryErrors = SoapExtensions.RegistryErrorsFromSoapEnvelope(responseEnvelope)?.RegistryError.Select(re => re.CodeContext).ToArray();

        var noConditionsApplied = accessControlresponse?.Diagnostics.All(d => d.Decision == Decision.NotApplicable);

        if (noConditionsApplied == true)
        {
            return "PDP: No eligible access control policies for this request (NotApplicable)";
        }

        var failedConditions = accessControlresponse?.Diagnostics
            .Where(d => d.Decision == Decision.Deny)
            .SelectMany(d => d.FailedConditions ?? [])
            .Select(d => "Invalid Parameter: " + d.AttributeId)
            .ToArray();

        if (registryErrors?.Length > 0)
            return string.Join(",\n", registryErrors);

        if (failedConditions?.Length > 0)
            return string.Join(",\n", failedConditions);

        if (appliedBusinessLogic?.Count > 0)
            return "Business Logic Applied: " + string.Join("\n", appliedBusinessLogic.Keys);

        return null;
    }

    private static Person? GetSubjectResource(List<Saml2Attribute>? statements,
        string? subjectDisplayName, AppliesTo? issuer, bool hasSubject, AdditionalParameters additionalParameters)
    {
        // patient_given and patient_family are custom attributes added only by
        // FhirMobileAccessToHealthDocumentsController.ProvideBundle method
        var isProvideBundle =
            additionalParameters.UrlPath?.StartsWith("/R4/fhir") == true &&
            additionalParameters.HttpMethod == "POST";

        string? patientGiven = null;
        string? patientFamily = null;
        XPN? subjectNameXpn = null;

        if (isProvideBundle)
        {
            patientGiven = statements?.GetValue("patient_given");
            patientFamily = statements?.GetValue("patient_family");

            subjectNameXpn = new(patientGiven, patientFamily);
        }
        else
        {
            if (subjectDisplayName?.Split() is { Length: >= 2 } namePart)
            {
                subjectNameXpn = new(namePart.FirstOrDefault(), string.Join(' ', namePart.Skip(1)));
            }
            else
            {
                subjectNameXpn = new(subjectDisplayName, null);
            }
        }

        var subjectIdCx = SamlExtensions.GetSamlAttributeValueAsCx(statements?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ProviderIdentifier)?.Values
            .FirstOrDefault());

        var subjectPatientResource = new Person
        {
            Id = "subject-1",
        };

        if (subjectIdCx != null)
        {
            subjectPatientResource.Identifier.Add(new Identifier()
            {
                System = subjectIdCx.AssigningAuthority?.UniversalId?.WithUrnOid(),
                Value = subjectIdCx.IdNumber
            });
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

                subjectPatientResource.Name = [patientHumanName];
            }
        }
        else if ((issuer == AppliesTo.Helsenorge || issuer == AppliesTo.Machine) && hasSubject)
        {
            var patientHumanName = new HumanName
            {
                Given = [subjectNameXpn.GivenName],
                Family = subjectNameXpn.FamilyName
            };

            subjectPatientResource.Name = [patientHumanName];
        }

        return subjectPatientResource;
    }

    private Patient? GetResourceResource(SoapEnvelope? requestEnvelope, List<Saml2Attribute>? statements, AdditionalParameters additionalParameters)
    {
        var resourcePatientResource = new Patient
        {
            Id = "resource-1",
        };

        var patientResourceId = statements?.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ResourceId20)?.Values
            .FirstOrDefault();

        var resourceIdCx = Hl7Object.Parse<CX>(patientResourceId);
        var resstr = resourceIdCx?.Serialize();

        if (!string.IsNullOrWhiteSpace(resourceIdCx?.IdNumber))
        {
            var resourcePid = new PID(resourceIdCx, null);
        }

        var registryResourcePatientIdentifiers = GetRegistryPatientIdentifierForRequest(requestEnvelope, additionalParameters).ToList();

        var allSubjectPatientIdentifiers = registryResourcePatientIdentifiers
            .DistinctBy(pid => new { pid?.PatientIdentifier?.IdNumber, pid?.PatientIdentifier?.AssigningAuthority?.UniversalId })
            .ToList();

        _logger.LogDebug($"AtnaLogGenerator resolved {allSubjectPatientIdentifiers.Count} resource identifiers from request");

        if (!(allSubjectPatientIdentifiers.Count > 0)) return resourcePatientResource;

        foreach (var identifier in allSubjectPatientIdentifiers)
        {
            if (string.IsNullOrWhiteSpace(identifier?.PatientIdentifier?.AssigningAuthority?.UniversalId) ||
                string.IsNullOrWhiteSpace(identifier.PatientIdentifier?.IdNumber)) continue;

            _logger.LogDebug($"AtnaLogGenerator found subject patient identifier: {identifier?.Serialize()}");

            resourcePatientResource.Identifier.Add(new Identifier(
                identifier?.PatientIdentifier?.AssigningAuthority?.UniversalId.WithUrnOid(),
                identifier?.PatientIdentifier?.IdNumber));

            if (identifier?.PatientName != null && resourcePatientResource.Name != null)
            {
                resourcePatientResource.Name = [new() { Family = identifier.PatientName.FamilyName, Given = [identifier.PatientName.GivenName] }];
            }
        }

        return resourcePatientResource;
    }

    private static HumanName? GetNamesFromPid(PID pid)
    {
        var family = pid.PatientName?.FamilyName;
        var given = pid.PatientName?.GivenName;

        if (!string.IsNullOrWhiteSpace(family) && !string.IsNullOrWhiteSpace(given))
        {
            return new HumanName(family, [given]);
        }

        return null;
    }

    static void AddDetail(List<AuditEvent.DetailComponent> detail, string type, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        detail.Add(new AuditEvent.DetailComponent { Type = type, Value = new FhirString(value) });
    }


    /// <summary>
    /// Get the patient identifier related to the registry objects being queried or stored
    /// </summary>
    private PID[] GetRegistryPatientIdentifierForRequest(SoapEnvelope? requestEnvelope, AdditionalParameters additionalParameters)
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
        var provideAndRegister =
            requestEnvelope.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList ??
            requestEnvelope.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList;

        if (provideAndRegister != null)
        {
            var registryObjects = _registryMetadataTransformerService
                .TransformRegistryObjectsToRegistryObjectDtos(provideAndRegister).ToArray();

            return PatientIdPidFromDocumentEntries(registryObjects) ?? [];
        }

        // ITI-39 or ITI-43 RetrieveDocumentSet
        var retrieveDocumentRequest = requestEnvelope.Body.RetrieveDocumentSetRequest;

        if (retrieveDocumentRequest != null)
        {
            var docEntryUniqueId = retrieveDocumentRequest.DocumentRequest;

            if (!(docEntryUniqueId?.Length > 0)) return [];

            var ids = new HashSet<string>(docEntryUniqueId.Select(x => x.DocumentUniqueId).OfType<string>());

            var registryObjects = ids.Select(_registryWrapper.GetSingleRegistryObjectAsDto)
                .OfType<RegistryObjectDto>().ToArray();

            return PatientIdPidFromDocumentEntries(registryObjects) ?? [];
        }

        // ITI-86 or ITI-62 DeleteDocumentSet
        if (additionalParameters.DeletedRegistryObjects?.Length > 0)
        {
            var registryObjects = additionalParameters.DeletedRegistryObjects;

            return PatientIdPidFromDocumentEntries(registryObjects) ?? [];
        }

        return [];
    }

    private static PID[]? PatientIdPidFromDocumentEntries(RegistryObjectDto[]? registryObjects)
    {
        if (registryObjects == null || registryObjects.Length == 0) return null;

        var pids = new List<PID>();

        foreach (var registryObject in registryObjects)
        {
            if (registryObject is DocumentEntryDto documentEntry && documentEntry.SourcePatientInfo is { } pid)
            {
                var patientId = new PID();

                if (pid.BirthTime.HasValue)
                {
                    patientId.BirthDate = pid.BirthTime.Value.ToUniversalTime();
                }

                if (!string.IsNullOrWhiteSpace(pid.FirstName) || !string.IsNullOrWhiteSpace(pid.LastName))
                {
                    patientId.PatientName = new XPN(pid.FirstName, pid.LastName);
                }

                if (!string.IsNullOrWhiteSpace(pid.PatientId?.Id) || !string.IsNullOrWhiteSpace(pid.PatientId?.System))
                {
                    patientId.PatientIdentifier = new CX(pid.PatientId?.Id, pid.PatientId?.System);
                }

                pids.Add(patientId);
            }
        }

        return pids.OfType<PID>().ToArray();
    }

    private static AuditEvent.AuditEventAction? GetAuditEventActionFromSoapEnvelope(SoapEnvelope? requestEnvelope, AdditionalParameters additionalParameters)
    {
        // Override for $validate-endpoint on FHIR, 
        if (RequestIsExecute(additionalParameters)) return AuditEvent.AuditEventAction.E;

        return (requestEnvelope?.Header.Action) switch
        {
            Constants.Xds.OperationContract.Iti18Action or
            Constants.Xds.OperationContract.Iti38Action or
            Constants.Xds.OperationContract.Iti43Action or
            Constants.Xds.OperationContract.Iti39Action =>
                (AuditEvent.AuditEventAction?)AuditEvent.AuditEventAction.R,

            Constants.Xds.OperationContract.Iti41Action or
            Constants.Xds.OperationContract.Iti42Action =>
                GetCreateOrUpdateFromRequest(requestEnvelope),

            Constants.Xds.OperationContract.Iti86Action or
            Constants.Xds.OperationContract.Iti62Action =>
                (AuditEvent.AuditEventAction?)AuditEvent.AuditEventAction.D,

            _ => (AuditEvent.AuditEventAction?)AuditEvent.AuditEventAction.R,
        };
    }

    private static bool RequestIsExecute(AdditionalParameters additionalParameters)
    {
        return additionalParameters.UrlPath != null &&
            additionalParameters.UrlPath.StartsWith("/R4/fhir") &&
            additionalParameters.UrlPath.EndsWith("/$validate");
    }

    private static AuditEvent.AuditEventAction? GetCreateOrUpdateFromRequest(SoapEnvelope requestEnvelope)
    {
        var registryObjects = requestEnvelope.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest
            ?.RegistryObjectList;

        var isReplaceUpdate = registryObjects?.OfType<AssociationType>()
            .Any(assoc =>
                assoc.AssociationTypeData?.IsAnyOf(Replace, Transformation, Addendum, ReplaceWithTransformation) ==
                true) ?? false;

        return isReplaceUpdate ? AuditEvent.AuditEventAction.U : AuditEvent.AuditEventAction.C;
    }

    private static AuditEvent.AuditEventOutcome GetAuditEventOutcomeFromSoapRequestResponse(SoapEnvelope? responseEnvelope)
    {
        var registryErrors = SoapExtensions.RegistryErrorsFromSoapEnvelope(responseEnvelope)?.RegistryError;
        var soapFault = responseEnvelope?.Body.Fault;

        // If we don't even have a Soap request or response, or if there is a SOAP fault, consider it a major failure (N8)
        if (responseEnvelope == null || soapFault != null)
        {
            return AuditEvent.AuditEventOutcome.N8;
        }

        if (registryErrors is { Length: > 0 })
        {
            return AuditEvent.AuditEventOutcome.N4;
        }

        return AuditEvent.AuditEventOutcome.N0;
    }

    /// <summary>
    /// <a href="https://hl7.org/fhir/R4/valueset-audit-event-type.html"/>
    /// </summary>
    private Coding GetAuditEventTypeFromSoapEnvelope(SoapEnvelope? requestEnvelope, AdditionalParameters additionalParameters)
    {
        var action = requestEnvelope?.Header.Action;

        if (RequestIsExecute(additionalParameters))
            return new Coding()
            {
                Code = "verify",
                System = IsoHealthRecordLifecycleEvent,
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
                    System = IsoHealthRecordLifecycleEvent,
                    Display = "Access/View Record Lifecycle Event"
                };

            case Constants.Xds.OperationContract.Iti41Action:
            case Constants.Xds.OperationContract.Iti42Action:
                var associations = requestEnvelope?.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest
                    ?.RegistryObjectList?.OfType<AssociationType>();

                var type = associations switch
                {
                    var ass when ass != null && ass.Any(assoc =>
                            assoc.AssociationTypeData.IsAnyOf(Transformation, ReplaceWithTransformation))
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
                        System = IsoHealthRecordLifecycleEvent,
                        Display = "Transform/Translate Record Lifecycle Event"
                    },

                    "amend" => new Coding()
                    {
                        Code = "amend",
                        System = IsoHealthRecordLifecycleEvent,
                        Display = "Amend (Update) Record Lifecycle Event"
                    },

                    // Default: Assume Hasmember/addition
                    _ => new Coding()
                    {
                        Code = "originate",
                        System = IsoHealthRecordLifecycleEvent,
                        Display = "Originate/Retain Record Lifecycle Event"
                    }
                };

            case Constants.Xds.OperationContract.Iti86Action:
            case Constants.Xds.OperationContract.Iti62Action:
                return new Coding()
                {
                    Code = "destroy",
                    System = IsoHealthRecordLifecycleEvent,
                    Display = "Destroy/Delete Record Lifecycle Event"
                };

            default:
                return new Coding()
                {
                    Code = "access",
                    System = IsoHealthRecordLifecycleEvent,
                    Display = "Access/View Record Lifecycle Event"
                };
        }
    }
}