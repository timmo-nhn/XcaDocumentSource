using Hl7.Fhir.Model;
using Microsoft.IdentityModel.Tokens.Saml2;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Source;
using static XcaXds.Commons.Commons.Constants.Xds.AssociationType;
using static XcaXds.Commons.Commons.Constants.Xds.QueryParameters;

namespace XcaXds.WebService.Services;

public class AtnaLogGeneratorService
{
    private readonly ILogger<AtnaLogGeneratorService> _logger;
    private readonly ApplicationConfig _appConfig;
    private readonly IAtnaLogQueue _queue;
    private readonly RegistryWrapper _registryWrapper;

    public AtnaLogGeneratorService(ILogger<AtnaLogGeneratorService> logger, ApplicationConfig appConfig, IAtnaLogQueue queue, RegistryWrapper registryWrapper)
    {
        _logger = logger;
        _appConfig = appConfig;
        _queue = queue;
        _registryWrapper = registryWrapper;
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

        var samlAssertionXml = requestEnvelope?.Header?.Security?.Assertion?.OuterXml;
        Saml2SecurityToken? samlToken = null;
        List<Saml2Attribute> statements = new();
        Issuer? issuer = null;

        if (!string.IsNullOrWhiteSpace(samlAssertionXml))
        {
            samlToken = PolicyRequestMapperSamlService.ReadSamlToken(samlAssertionXml);
            statements = samlToken.Assertion.Statements
                .OfType<Saml2AttributeStatement>()
                .SelectMany(statement => statement.Attributes)
                .ToList();

            issuer = PolicyRequestMapperSamlService.GetIssuerEnumFromSamlTokenIssuer(samlToken.Issuer);
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
            What = new ResourceReference(requestEnvelope.Header.MessageId, "SOAP message ID"),
        });

        if (samlToken != null)
        {
            var subjectNameRaw = statements
                .FirstOrDefault(s => s.Name == Constants.Saml.Attribute.SubjectId)
                ?.Values
                .FirstOrDefault();

            var subjectNameCoded = SamlExtensions.GetSamlAttributeValueAsCodedValue(subjectNameRaw);
            var subjectDisplayName = string.IsNullOrWhiteSpace(subjectNameCoded?.Code) ? null : subjectNameCoded.Code;

            var providerIdentifierValue = statements
                .FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ProviderIdentifier)
                ?.Values
                .FirstOrDefault();

            var providerIdentifierCoded = SamlExtensions.GetSamlAttributeValueAsCodedValue(providerIdentifierValue);

            var hasSubject = !string.IsNullOrWhiteSpace(subjectDisplayName)
                || (!string.IsNullOrWhiteSpace(providerIdentifierCoded?.Code) || !string.IsNullOrWhiteSpace(providerIdentifierCoded?.CodeSystem));

            var patientResourceId = statements
                .FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ResourceId20)
                ?.Values
                .FirstOrDefault();

            // is_provide_bundle, patient_given and patient_family are custom attributes added only by FhirMobileAccessToHealthDocumentsController.ProvideBundle method
            var isProvideBundle = statements
                .FirstOrDefault(s => s.Name == "is_provide_bundle")
                ?.Values
                .FirstOrDefault() == "true";

            string? patientGiven = null;
            string? patientFamily = null;

            if (isProvideBundle)
            {
                patientGiven = statements
                    .FirstOrDefault(s => s.Name == "patient_given")
                    ?.Values
                    .FirstOrDefault();

                patientFamily = statements
                    .FirstOrDefault(s => s.Name == "patient_family")
                    ?.Values
                    .FirstOrDefault();
            }

            var patientIdentifierCx = Hl7Object.Parse<CX>(patientResourceId);

            var registryPatientIdentifiers = GetRegistryPatientIdentifierForRequest(requestEnvelope).Append(patientIdentifierCx)
                .DistinctBy(pid => new { pid?.IdNumber, pid?.AssigningAuthority?.UniversalId })
                .ToList();

            var patientResource = new Patient
            {
                Id = "patient-1",
            };

            _logger.LogDebug($"AtnaLogGenerator Resolved {registryPatientIdentifiers.Count} identifiers from request");

            foreach (var identifier in registryPatientIdentifiers)
            {
                _logger.LogDebug($"AtnaLogGenerator Resolved {identifier?.Serialize()} identifiers from request");
                if (identifier == null || string.IsNullOrWhiteSpace(identifier.AssigningAuthority?.UniversalId) || string.IsNullOrWhiteSpace(identifier.IdNumber)) continue;

                patientResource.Identifier.Add(new Identifier(identifier?.AssigningAuthority?.UniversalId, identifier?.IdNumber));
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
                        Given = string.IsNullOrWhiteSpace(patientGiven) ? null : [patientGiven]
                    };
                    patientResource.Name = [patientHumanName];
                }
            }
            else if (issuer == Issuer.Helsenorge && hasSubject)
            {
                var subjectPersonNameParts = subjectDisplayName?.Split().ToList();
                if (subjectPersonNameParts != null && subjectPersonNameParts.Count != 0)
                {
                    var patientHumanName = new HumanName
                    {
                        Family = subjectPersonNameParts.LastOrDefault(),
                        Given = subjectPersonNameParts.Count > 1 ? subjectPersonNameParts.Take(subjectPersonNameParts.Count - 1).ToList() : null
                    };
                    patientResource.Name = [patientHumanName];
                }
            }

            auditEvent.Contained.Add(patientResource);

            auditEvent.Entity.Add(new AuditEvent.EntityComponent()
            {
                What = new ResourceReference($"#{patientResource.Id}")
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


            var orgnrParent = statements.FirstOrDefault(s => s.Name == "helseid://claims/client/claims/orgnr_parent")
                ?.Values
                .FirstOrDefault();
            var clientName = statements.FirstOrDefault(s => s.Name == "helseid://claims/client/client_name")
                ?.Values
                .FirstOrDefault();
            var clientId = statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.EhelseClientId)
                ?.Values
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(orgnrParent) || !string.IsNullOrWhiteSpace(clientName) || !string.IsNullOrWhiteSpace(clientId))
            {
                var detail = new List<AuditEvent.DetailComponent>();
                if (!string.IsNullOrWhiteSpace(orgnrParent))
                {
                    detail.Add(new AuditEvent.DetailComponent { Type = "orgnr_parent", Value = new FhirString(orgnrParent) });
                }
                if (!string.IsNullOrWhiteSpace(clientId))
                {
                    detail.Add(new AuditEvent.DetailComponent { Type = "client_id", Value = new FhirString(clientId) });
                }
                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    detail.Add(new AuditEvent.DetailComponent { Type = "client_name", Value = new FhirString(clientName) });
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
                    Detail = detail
                });
            }

            var purposeOfUseValue = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.PurposeOfUse || s.Name == Constants.Saml.Attribute.PurposeOfUse_Helsenorge)?.Values.FirstOrDefault());

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
                if (subjectNameParts != null && subjectNameParts.Count != 0)
                {
                    healthcarePersonHumanName = new HumanName
                    {
                        Family = subjectNameParts.LastOrDefault(),
                        Given = subjectNameParts.Count > 1 ? subjectNameParts.Take(subjectNameParts.Count - 1).ToList() : null
                    };
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

                var pointOfCareStatement = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.ChildOrganization)?.Values.FirstOrDefault());
                var pointOfCareName = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.TrustChildOrgName)?.Values.FirstOrDefault());

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

                var legalEntityStatement = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.OrganizationId)?.Values.FirstOrDefault());
                var legalEntityName = SamlExtensions.GetSamlAttributeValueAsCodedValue(statements.FirstOrDefault(s => s.Name == Constants.Saml.Attribute.Organization)?.Values.FirstOrDefault());

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

                auditEvent.Agent.Add(new AuditEvent.AgentComponent()
                {
                    Who = new ResourceReference($"#{practitionerRole.Id}")
                    {
                        Identifier = practitionerRole.Identifier.FirstOrDefault()
                    },
                    Requestor = true,
                    PurposeOfUse = auditEvent.PurposeOfEvent,
                    Policy = string.IsNullOrWhiteSpace(samlToken.Id) ? null : [samlToken.Id],
                    Network = string.IsNullOrWhiteSpace(_appConfig.IpAddress)
                        ? null
                        : new AuditEvent.NetworkComponent()
                        {
                            Address = _appConfig.IpAddress
                        }
                });
            }
            else
            {
                auditEvent.Agent.Add(new AuditEvent.AgentComponent()
                {
                    Requestor = true,
                    Policy = string.IsNullOrWhiteSpace(samlToken.Id) ? null : [samlToken.Id],
                    Network = string.IsNullOrWhiteSpace(_appConfig.IpAddress)
                        ? null
                        : new AuditEvent.NetworkComponent()
                        {
                            Address = _appConfig.IpAddress
                        }
                });
            }
        }

        auditEvent.Type = GetAuditEventTypeFromSoapEnvelope(requestEnvelope);
        auditEvent.Recorded = DateTimeOffset.Now;
        auditEvent.Outcome = GetEventOutcomeFromSoapRequestResponse(requestEnvelope, responseEnvelope);
        auditEvent.Action = GetActionFromSoapEnvelope(requestEnvelope);

        var docRequest = requestEnvelope?.Body?.ProvideAndRegisterDocumentSetRequest;
        var xdsDoc = docRequest?.Document?.FirstOrDefault();
        var rol = docRequest?.SubmitObjectsRequest?.RegistryObjectList;

        var xdsDocEntry = RegistryMetadataTransformerService.TransformRegistryObjectsToRegistryObjectDtos(rol?.OfType<ExtrinsicObjectType>())?.FirstOrDefault();
        var xdsSubmissionSet = RegistryMetadataTransformerService.TransformRegistryObjectsToRegistryObjectDtos(rol?.OfType<RegistryPackageType>())?.FirstOrDefault();

        if (xdsDoc == null || xdsDocEntry == null)
        {
            var registryContent = _registryWrapper.GetDocumentRegistryContentAsDtos();
            var retrieveDocumentsRequest = requestEnvelope?.Body?.RetrieveDocumentSetRequest?.DocumentRequest.FirstOrDefault();

            xdsDocEntry = registryContent.OfType<DocumentEntryDto>().FirstOrDefault(rc => rc.UniqueId == retrieveDocumentsRequest?.DocumentUniqueId);
        }

        if (xdsDoc != null || xdsDocEntry != null)
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

            var detail = new List<AuditEvent.DetailComponent>();

            AddDetail(detail, "documentUniqueId", docUniqueId);
            AddDetail(detail, "mimeType", mimeType);
            AddDetail(detail, "classCode", classCode);
            AddDetail(detail, "title", title);
            AddDetail(detail, "homeCommunityId", _appConfig.HomeCommunityId);

            var submissionSetId = xdsSubmissionSet?.Id;
            AddDetail(detail, "submissionSetId", submissionSetId);

            auditEvent.Entity.Add(new AuditEvent.EntityComponent
            {
                What = reference == null
                    ? new ResourceReference { Display = title }
                    : new ResourceReference(reference) { Display = title },
                Type = new Coding
                {
                    System = "http://terminology.hl7.org/CodeSystem/audit-entity-type",
                    Code = "2",
                    Display = "System Object"
                },
                Role = new Coding
                {
                    System = "http://terminology.hl7.org/CodeSystem/object-role",
                    Code = "3",
                    Display = "Report"
                },
                Detail = detail.Count == 0 ? null : detail
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

        // Make these readable in logs:
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

        // IMPORTANT: remove this old overwrite entirely:
        // if (auditEvent.Contained.OfType<PractitionerRole>().FirstOrDefault()?.Organization != null)
        // {
        //     auditEvent.Source.Observer = auditEvent.Contained.OfType<PractitionerRole>().First().Organization;
        // }

        return auditEvent;
    }

    void AddDetail(List<AuditEvent.DetailComponent> detail, string type, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        detail.Add(new AuditEvent.DetailComponent { Type = type, Value = new FhirString(value) });
    }


    /// <summary>
    /// Get the patient identifier related to the registry objects being queried or stored
    /// </summary>
    private CX?[] GetRegistryPatientIdentifierForRequest(SoapEnvelope requestEnvelope)
    {
        // ITI-38 or ITI-18 AdhocQuery FindDocuments 
        var requestPatientIdentifier = requestEnvelope.Body.AdhocQueryRequest?.AdhocQuery
            .GetFirstSlot(Constants.Xds.QueryParameters.FindDocuments.PatientId)?.GetFirstValue();

        if (requestPatientIdentifier != null)
        {
            _logger.LogDebug(requestPatientIdentifier, "Found patient identifier in AdhocQueryRequest");
            return [Hl7Object.Parse<CX>(requestPatientIdentifier)];
        }

        // ITI-41 or ITI-42
        // DeleteDocuments_Jank! ITI-86 or ITI-62 DeleteDocumentSet
        var provideAndRegister = requestEnvelope.Body?.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList ?? requestEnvelope.Body?.RegisterDocumentSetRequest?.SubmitObjectsRequest?.RegistryObjectList;

        if (provideAndRegister != null)
        {
            var registryObjects = RegistryMetadataTransformerService.TransformRegistryObjectsToRegistryObjectDtos(provideAndRegister);

            return PatientIdCxFromDocumentEntries(registryObjects);
        }

        // ITI-39 or ITI-43 RetrieveDocumentSet
        var registry = _registryWrapper.GetDocumentRegistryContentAsDtos();
        var retrieveDocumentRequest = requestEnvelope.Body?.RetrieveDocumentSetRequest;

        if (retrieveDocumentRequest != null)
        {
            var docEntryUniqueId = retrieveDocumentRequest.DocumentRequest;

            if (docEntryUniqueId?.Length < 0)
            {
                var ids = new HashSet<string>(docEntryUniqueId.Select(x => x.DocumentUniqueId ?? string.Empty));

                var documentEntries = registry
                    .OfType<DocumentEntryDto>()
                    .Where(de => ids.Contains(de.UniqueId ?? string.Empty));

                return PatientIdCxFromDocumentEntries(documentEntries);
            }
        }

        return [];
    }

    private CX?[] PatientIdCxFromDocumentEntries(IEnumerable<RegistryObjectDto> registryObjects)
    {
        return registryObjects.OfType<DocumentEntryDto>().Select(de => new CX()
        {
            IdNumber = de.SourcePatientInfo?.PatientId?.Id ?? "Unknown",
            AssigningAuthority = new HD()
            {
                UniversalId = de.SourcePatientInfo?.PatientId?.System ?? "Unknown",
                UniversalIdType = Constants.Hl7.UniversalIdType.Iso
            }
        }).ToArray();
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
                return GetCreateOrUpdateFromRequest(requestEnvelope);

            case Constants.Xds.OperationContract.Iti86Action:
            case Constants.Xds.OperationContract.Iti62Action:
                return AuditEvent.AuditEventAction.D;

            default:
                return AuditEvent.AuditEventAction.R;
        }
    }

    private AuditEvent.AuditEventAction? GetCreateOrUpdateFromRequest(SoapEnvelope requestEnvelope)
    {
        var registryObjects = requestEnvelope.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList;

        var isReplaceUpdate = registryObjects?.OfType<AssociationType>()
            .Any(assoc => assoc.AssociationTypeData.IsAnyOf(Replace, Transformation, Addendum, ReplaceWithTransformation)) ?? false;

        return isReplaceUpdate ? AuditEvent.AuditEventAction.U : AuditEvent.AuditEventAction.C;
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
                var associations = requestEnvelope.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList?.OfType<AssociationType>();

                var isAmend = associations?.Any(assoc => assoc.AssociationTypeData.IsAnyOf(HasMember, Addendum)) ?? false;
                var isTransform = associations?.Any(assoc => assoc.AssociationTypeData.IsAnyOf(Transformation, ReplaceWithTransformation)) ?? false;
                var isReplace = associations?.Any(assoc => assoc.AssociationTypeData.IsAnyOf(Replace)) ?? false;

                if (isAmend)
                {
                    return new Coding()
                    {
                        Code = "amend",
                        System = Constants.Hl7.CodeSystems.IsoHealthRecordLifecycleEvent,
                        Display = "Amend (Update) Record Lifecycle Event"
                    };
                }
                if (isTransform)
                {
                    return new Coding()
                    {
                        Code = "transform",
                        System = Constants.Hl7.CodeSystems.IsoHealthRecordLifecycleEvent,
                        Display = "Transform/Translate Record Lifecycle Event"
                    };
                }
                if (isReplace)
                {
                    return new Coding()
                    {
                        Code = "originate",
                        System = Constants.Hl7.CodeSystems.IsoHealthRecordLifecycleEvent,
                        Display = "Originate/Retain Record Lifecycle Event"
                    };
                }

                // Default: Assume Hasmember/addition
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