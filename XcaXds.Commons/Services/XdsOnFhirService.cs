using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;

namespace XcaXds.Commons.Services;

/// <summary>
/// XDS on FHIR functionality, supporting the Mobile access to Health Documents (MHD) - integration of the solution <para/>
/// <a href="https://profiles.ihe.net/ITI/MHD/"/>
/// </summary>
public class XdsOnFhirService
{
    private readonly IRegistry _registry;
    private readonly ApplicationConfig _appConfig;
    private readonly ILogger<XdsOnFhirService> _logger;

    public XdsOnFhirService(IRegistry registry, ApplicationConfig appConfig, ILogger<XdsOnFhirService> logger)
    {
        _registry = registry;
        _appConfig = appConfig;
        _logger = logger;
    }

    public AdhocQueryRequest ConvertIti67ToIti18AdhocQuery(MhdDocumentRequest documentRequest)
    {
        var adhocQueryRequest = new AdhocQueryRequest();
        var adhocQuery = new AdhocQueryType();

        if (!string.IsNullOrWhiteSpace(documentRequest.Patient))
        {
            var patientCx = Hl7Object.Parse<CX>(documentRequest.Patient, '|');

            var patientOid = Hl7FhirExtensions.ParseNorwegianNinToCxWithAssigningAuthority(documentRequest.Patient);

            if (patientOid != null && patientCx != null)
            {
                patientCx.AssigningAuthority ??= new()
                {
                    UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                    UniversalId = patientOid.AssigningAuthority.UniversalId
                };
            }

            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.PatientId, [patientCx.Serialize()]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Creation))
        {
            var documentCreationTimeRange = Hl7FhirExtensions.GetDateTimeRangeFromDateParameters(documentRequest.Creation);

            if (documentCreationTimeRange.Start.HasValue)
            {
                adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.CreationTimeFrom, [documentCreationTimeRange.Start.Value.ToString("O")]);
            }

            if (documentCreationTimeRange.End.HasValue)
            {
                adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.CreationTimeTo, [documentCreationTimeRange.End.Value.ToString("O")]);
            }
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.AuthorFamily))
        {
            adhocQuery.UpdateSlot(Constants.Xds.QueryParameters.FindDocuments.AuthorPerson, [documentRequest.AuthorFamily]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.AuthorGiven))
        {
            adhocQuery.UpdateSlot(Constants.Xds.QueryParameters.FindDocuments.AuthorPerson, [documentRequest.AuthorGiven]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Status))
        {
            var ebRimStatus = documentRequest.Status switch
            {
                "current" => Constants.Xds.StatusValues.Approved,
                "superseded" => Constants.Xds.StatusValues.Deprecated,
                _ => Constants.Xds.StatusValues.Approved
            };

            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.Status, [ebRimStatus]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Category))
        {
            var classCodeCx = Hl7Object.Parse<CX>(documentRequest.Category, '|');
            if (classCodeCx != null)
            {
                classCodeCx.AssigningAuthority ??= new()
                {
                    UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                    UniversalId = Constants.Oid.CodeSystems.Volven.DocumentType
                };

                adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.ClassCode, [classCodeCx.Serialize()]);
            }
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Type))
        {
            var typeCodeCx = Hl7Object.Parse<CX>(documentRequest.Type, '|');
            if (typeCodeCx != null)
            {
                typeCodeCx.AssigningAuthority ??= new()
                {
                    UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                    UniversalId = Constants.Oid.CodeSystems.Volven.DocumentType
                };

                adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.TypeCode, [typeCodeCx.Serialize()]);
            }
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Event))
        {
            var eventCodeCx = Hl7Object.Parse<CX>(documentRequest.Event, '|');
            if (eventCodeCx != null)
            {
                eventCodeCx.AssigningAuthority ??= new()
                {
                    UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                    UniversalId = Constants.Oid.CodeSystems.Volven.EventCode
                };

                adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.EventCodeList, [eventCodeCx.Serialize()]);
            }
        }


        adhocQueryRequest.AdhocQuery = adhocQuery;

        return adhocQueryRequest;
    }

    public Bundle? TransformRegistryObjectsToFhirBundle(IdentifiableType[]? registryObjectList)
    {
        // Create a Bundle with DocumentReference resources and return it as the response
        // See example here https://profiles.ihe.net/ITI/MHD/Bundle-Bundle-FindDocumentReferences.json
        var bundle = new Bundle
        {
            Id = Guid.NewGuid().ToString(),
            Meta = new Meta()
            {
                VersionId = "1",
                LastUpdatedElement = Instant.Now(),
                Profile = ["https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.FindDocumentReferencesComprehensiveResponseMessage"],
            },
            Type = Bundle.BundleType.Searchset,
            Total = registryObjectList.Length,
            Timestamp = DateTime.UtcNow,
            Entry = new List<Bundle.EntryComponent>()
        };


        // If only ExtrinsicObjects are in the registryObjectList (such as when returning a FindDocuments AdhocQueryRequest)
        // we need to fetch the registry and get the missing registry objects to properly map this to a FHIR resource
        if (registryObjectList.Length != 0 && registryObjectList.Length == registryObjectList.OfType<ExtrinsicObjectType>().ToArray().Length)
        {
            var registryContent = RegistryMetadataTransformerService.TransformRegistryObjectDtosToRegistryObjects(_registry.ReadRegistry());

			var eos = registryObjectList.OfType<ExtrinsicObjectType>().ToArray();
			var eoIds = eos.Select(e => e.Id?.NoUrn()).Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet(StringComparer.OrdinalIgnoreCase);

			// Pull BOTH membership + lifecycle associations
			var relatedAssociations = registryContent
				.OfType<AssociationType>()
				.Where(a =>
					(a.AssociationTypeData == Constants.Xds.AssociationType.HasMember && eoIds.Contains(a.TargetObject.NoUrn())) ||
					(a.AssociationTypeData != Constants.Xds.AssociationType.HasMember && eoIds.Contains(a.SourceObject.NoUrn()))
				)
				.ToArray();

			// Pull submission sets (registry packages) referenced by HasMember.SourceObject
			var relatedRegistryPackages = relatedAssociations
				.Where(a => a.AssociationTypeData == Constants.Xds.AssociationType.HasMember)
				.Select(a => registryContent.GetById(a.SourceObject))
				.OfType<RegistryPackageType>()
				.ToArray();

			registryObjectList = [.. eos, .. relatedAssociations, .. relatedRegistryPackages];
		}


		var documentReference = GetFhirDocumentReferencesFromRegistryObjects(registryObjectList);

        bundle.Entry.AddRange(documentReference
            .Select(dr => new Bundle.EntryComponent()
            {
                FullUrl = $"urn:uuid:{Guid.NewGuid().ToString()}",
                Resource = dr
            })
        );

        return bundle;
    }

    private IEnumerable<DocumentReference> GetFhirDocumentReferencesFromRegistryObjects(IdentifiableType[] registryObjectList)
    {
        // Mapping table used to generate DocumentReference:
        // https://profiles.ihe.net/ITI/MHD/StructureDefinition-IHE.MHD.Minimal.DocumentReference-mappings.html#mappings-for-xds-and-mhd-mapping-xds

        var documentReferenceList = new List<DocumentReference>();

        var extrinsicObjects = registryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var registryPackages = registryObjectList.OfType<RegistryPackageType>().ToArray();
        var associations = registryObjectList.OfType<AssociationType>().ToArray();

        if (extrinsicObjects.Length == 1 && registryPackages.Length == 1 && associations.Length == 0)
        {
            associations = [.. associations.Append(new AssociationType()
            {
                Id = Guid.NewGuid().ToString(),
                AssociationTypeData = Constants.Xds.AssociationType.HasMember,
                SourceObject = registryPackages.First().Id,
                TargetObject = extrinsicObjects.First().Id
            })];
        }

        foreach (var association in associations)
        {
            if (association.AssociationTypeData is not Constants.Xds.AssociationType.HasMember) continue;

            var assocExtrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id?.NoUrn() == association.TargetObject.NoUrn());
            var assocRegistryPackage = registryPackages.FirstOrDefault(rp => rp.Id?.NoUrn() == association.SourceObject.NoUrn());

            var documentReference = new DocumentReference();

			documentReference.Id = assocExtrinsicObject.Id?.NoUrn();

			if (assocRegistryPackage == null || assocExtrinsicObject == null) continue;

            documentReference.MasterIdentifier = GetMasterIdentifierFromRegistryPackageSourceId(assocRegistryPackage, assocExtrinsicObject);
            documentReference.Identifier = GetIdentifierFromExtrinsicObjectId(assocExtrinsicObject);
            documentReference.Status = GetDocumentReferenceStatusFromExtrinsicObjectStatus(assocExtrinsicObject);
            documentReference.Type = GetTypeFromExtrinsicObjectTypeCode(assocExtrinsicObject);
            documentReference.Category = GetCategoryFromExtrinsicObjectClassCode(assocExtrinsicObject);

            // Add patient
            var patientResource = GetPatientAsPatientResource(assocExtrinsicObject);
            if (patientResource != null)
            {
                documentReference.Contained.Add(patientResource);
                documentReference.Subject = Hl7FhirExtensions.GetResourceAsResourceReference(patientResource);
            }

            // Add Author
            var authorResources = GetAuthorRelatedAsResourceList(assocExtrinsicObject);
            if (authorResources != null)
            {
                documentReference.Contained.AddRange(authorResources);
                documentReference.Author = Hl7FhirExtensions.GetResourceAsResourceReference(authorResources);
            }

            // Authenticator
            var authenticator = GetAuthenticatorFromExtrinsicObjectLegalAuthenticator(assocExtrinsicObject);
            if (authenticator != null)
            {
                documentReference.Contained.Add(authenticator);
                documentReference.Authenticator = Hl7FhirExtensions.GetResourceAsResourceReference(authenticator);
            }
			
			// RelatesTo (XDS Associations -> FHIR relatesTo)
			var relatesTo = BuildRelatesTo(associations, assocExtrinsicObject);
			if (relatesTo.Count > 0)
			{
				documentReference.RelatesTo = relatesTo;
			}

			// SecurityLabel
			var securityLabel = GetCodeableConceptFromExtrinsicObjectConfidentialityCode(assocExtrinsicObject);
            if (securityLabel != null)
            {
                documentReference.SecurityLabel.AddRange(securityLabel);
            }


            // Content
            var content = GetDocumentReferenceContentPropertyFromExtrinsicObject(assocExtrinsicObject);
            if (content != null)
            {
                documentReference.Content.Add(content);
            }


            // Context
            var context = GetContextComponentFromExtrinsicObject(assocExtrinsicObject);
            if (context != null)
            {
                documentReference.Context = context;
                documentReference.Context.SourcePatientInfo = documentReference.Subject;
            }


            documentReferenceList.Add(documentReference);
        }

        return documentReferenceList;
    }

	private static List<DocumentReference.RelatesToComponent> BuildRelatesTo(
	AssociationType[] allAssociations,
	ExtrinsicObjectType currentEo)
	{
		var relatesTo = new List<DocumentReference.RelatesToComponent>();

		if (currentEo.Id == null) return relatesTo;

		var sourceId = currentEo.Id.NoUrn();

		// Associations where THIS document is the SourceObject
		var outgoing = allAssociations
			.Where(a =>
				a.AssociationTypeData != Constants.Xds.AssociationType.HasMember &&
				a.SourceObject.NoUrn() == sourceId)
			.ToList();

		foreach (var a in outgoing)
		{
			var code = a.AssociationTypeData switch
			{
				var t when t == Constants.Xds.AssociationType.Replace => DocumentRelationshipType.Replaces,
				var t when t == Constants.Xds.AssociationType.Addendum => DocumentRelationshipType.Appends,
				var t when t == Constants.Xds.AssociationType.Transformation => DocumentRelationshipType.Transforms,
				var t when t == Constants.Xds.AssociationType.DigitalSignature /* or Signs */ => DocumentRelationshipType.Signs,
				_ => (DocumentRelationshipType?)null
			};

			if (code is null) continue;

			var targetUuid = a.TargetObject.NoUrn();
			if (string.IsNullOrWhiteSpace(targetUuid)) continue;

			relatesTo.Add(new DocumentReference.RelatesToComponent
			{
				Code = code.Value,
				Target = new ResourceReference
				{
					// Your ExistingDocumentId is entryUUID, so reference by id
					Reference = $"DocumentReference/{targetUuid}"
				}
			});
		}

		return relatesTo;
	}


	private DocumentReference.ContextComponent? GetContextComponentFromExtrinsicObject(ExtrinsicObjectType? assocExtrinsicObject)
    {
        if (assocExtrinsicObject == null) return null;

        var context = new DocumentReference.ContextComponent();


        var eventCodeList = RegistryMetadataTransformerService.MapClassificationToCodedValue(assocExtrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.EventCodeList));

        if (eventCodeList != null)
        {
            context.Event.Add(new CodeableConcept()
            {
                Coding =
                [
                    new()
                    {
                        Code = eventCodeList.Code,
                        System = AddUrnOidIfOid(eventCodeList.CodeSystem),
                        Display = eventCodeList.DisplayName
                    }
                ]
            });
        }

        var serviceStartTime = assocExtrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.ServiceStartTime)?.GetFirstValue();
        if (serviceStartTime != null)
        {
            var serviceStartTimeDate = DateTime.ParseExact(serviceStartTime, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            context.Period ??= new();
            context.Period.Start = ((DateTimeOffset)serviceStartTimeDate).ToUniversalTime().ToString(Constants.Hl7.Dtm.DtmFhirIsoDateTimeFormat);
        }


        var serviceStopTime = assocExtrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.ServiceStopTime)?.GetFirstValue();
        if (serviceStopTime != null)
        {
            var serviceStopTimeDate = DateTime.ParseExact(serviceStopTime, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            context.Period ??= new();
            context.Period.End = ((DateTimeOffset)serviceStopTimeDate).ToUniversalTime().ToString(Constants.Hl7.Dtm.DtmFhirIsoDateTimeFormat);
        }

        var healthCareFacilityType = RegistryMetadataTransformerService.MapClassificationToCodedValue(assocExtrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.HealthCareFacilityTypeCode));

        if (healthCareFacilityType != null)
        {
            context.FacilityType = new CodeableConcept()
            {
                Coding =
                [
                    new()
                    {
                        Code = healthCareFacilityType.Code,
                        System = AddUrnOidIfOid(healthCareFacilityType.CodeSystem),
                        Display = healthCareFacilityType.DisplayName
                    }
                ]
            };
        }


        var practiceSettingCode = RegistryMetadataTransformerService.MapClassificationToCodedValue(assocExtrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.PracticeSettingCode));

        if (practiceSettingCode != null)
        {
            context.PracticeSetting = new CodeableConcept()
            {
                Coding =
                [
                    new()
                    {
                        Code = practiceSettingCode.Code,
                        System = AddUrnOidIfOid(practiceSettingCode.CodeSystem),
                        Display = practiceSettingCode.DisplayName
                    }
                ]
            };
        }

        return context;
    }

    private static string? AddUrnOidIfOid(string? codeSystem)
    {
        if (string.IsNullOrWhiteSpace(codeSystem)) return null;

        if (Regex.IsMatch(codeSystem, @"^[\d\.]+$") && codeSystem.StartsWith("urn:oid:") == false)
        {
            return $"urn:oid:{codeSystem}";
        }

        return codeSystem;
    }

    private static DocumentReference.ContentComponent? GetDocumentReferenceContentPropertyFromExtrinsicObject(ExtrinsicObjectType assocExtrinsicObject)
    {
        var content = new DocumentReference.ContentComponent();

        var contenType = assocExtrinsicObject.MimeType;
        var language = assocExtrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.LanguageCode)?.GetFirstValue();
        var url = "placeholder";
        var size = long.MinValue;
        if (long.TryParse(assocExtrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.Size)?.GetFirstValue() ?? "0", out var sizeLong))
        {
            size = sizeLong;
        }
        var hash = Encoding.UTF8.GetBytes(assocExtrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.Hash)?.GetFirstValue() ?? "");
        var title = assocExtrinsicObject.Name.GetFirstValue();

        var creationSlot = assocExtrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.CreationTime)?.GetFirstValue();
        var parsedDate = DateTime.ParseExact(creationSlot, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        var creation = ((DateTimeOffset)parsedDate).ToString(Constants.Hl7.Dtm.DtmFhirIsoDateTimeFormat);

        content.Attachment = new Attachment()
        {
            ContentType = contenType,
            Language = language,
            Url = url,
            Size = size,
            Hash = hash,
            Title = title,
            Creation = creation
        };

        var format = RegistryMetadataTransformerService.MapClassificationToCodedValue(assocExtrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.FormatCode));

        if (format != null)
        {
            content.Format = new Coding()
            {
                Code = format.Code,
                System = AddUrnOidIfOid(format.CodeSystem),
                Display = format.DisplayName
            };
        }

        return content;
    }

    private static List<CodeableConcept>? GetCodeableConceptFromExtrinsicObjectConfidentialityCode(ExtrinsicObjectType assocExtrinsicObject)
    {
        var confidentialityCode = RegistryMetadataTransformerService.MapClassificationToCodedValue(assocExtrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode));

        if (confidentialityCode == null) return null;

        var codeableConcept = new CodeableConcept()
        {
            Coding =
            [
                new()
                {
                    Code = confidentialityCode.Code,
                    System = AddUrnOidIfOid(confidentialityCode.CodeSystem),
                    Display = confidentialityCode.DisplayName
                }
            ]
        };

        return [codeableConcept];
    }

    private static Practitioner? GetAuthenticatorFromExtrinsicObjectLegalAuthenticator(ExtrinsicObjectType assocExtrinsicObject)
    {
        var legalAuthenticatorSlot = Hl7Object.Parse<XCN>(assocExtrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.LegalAuthenticator)?.GetFirstValue());
        return GetPractitionerFromAuthorPerson(legalAuthenticatorSlot);
    }

    private static List<Resource> GetAuthorRelatedAsResourceList(ExtrinsicObjectType assocExtrinsicObject)
    {
        var resourceList = new List<Resource>();

        var authorClassification = assocExtrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.Author);


        var authorPerson = Hl7Object.Parse<XCN>(authorClassification?.GetSlots(Constants.Xds.SlotNames.AuthorPerson)?.GetValues().FirstOrDefault());
        var practitioner = GetPractitionerFromAuthorPerson(authorPerson);

        if (practitioner != null)
        {
            resourceList.Add(practitioner);
        }



        var authorInstitution = new XON();
        var authorDepartment = new XON();
        var authorInstitutionValues = authorClassification?.GetSlots(Constants.Xds.SlotNames.AuthorInstitution)?.GetValues().Select(auth => Hl7Object.Parse<XON>(auth)).ToList();

        if (authorInstitutionValues != null && authorInstitutionValues.Count != 0)
        {
            authorInstitution = authorInstitutionValues.FirstOrDefault(authInst => authInst?.AssigningAuthority?.UniversalId == Constants.Oid.Brreg || authInst?.AssigningAuthority?.UniversalId != null);
            authorDepartment = authorInstitutionValues.LastOrDefault(authInst => authInst?.AssigningAuthority?.UniversalId == Constants.Oid.ReshId || authInst?.AssigningAuthority?.UniversalId != null);

            // If department and institution was the same, nullify department to avoid creating duplicates
            if (authorInstitution != null && authorDepartment != null && authorInstitution.OrganizationName == authorDepartment.OrganizationName)
            {
                authorDepartment = null;
            }
        }


        var organization = GetOrganizationFromAuthorInstitution(authorInstitution);

        if (authorInstitution != null && organization != null)
        {
            resourceList.Add(organization);
        }


        var department = GetOrganizationDepartmentFromAuthorInstitution(authorDepartment);

        if (authorDepartment != null && department != null)
        {
            department.PartOf = new ResourceReference()
            {
                Reference = $"#{organization.Id}"
            };
            resourceList.Add(department);
        }


        var authorSpeciality = Hl7Object.Parse<CX>(authorClassification?.GetSlots(Constants.Xds.SlotNames.AuthorSpecialty)?.GetValues().FirstOrDefault());
        var authorRole = Hl7Object.Parse<CX>(authorClassification?.GetSlots(Constants.Xds.SlotNames.AuthorRole)?.GetValues().FirstOrDefault());

        if (authorSpeciality != null && authorRole != null)
        {
            var practitionerRole = GetPractitionerRoleFromAuthorRoleAndAuthorSpeciality(authorSpeciality, authorRole);
            if (practitionerRole != null)
            {
                resourceList.Add(practitionerRole);
            }
        }

        return resourceList;
    }

    private static Organization? GetOrganizationFromAuthorInstitution(XON? authorInstitution)
    {
        var organization = new Organization();
        if (authorInstitution != null)
        {
            organization.Id = Guid.NewGuid().ToString();

            organization.Name = authorInstitution?.OrganizationName;
            if (authorInstitution?.OrganizationIdentifier != null)
            {
                organization.Identifier = [new() { Value = authorInstitution?.OrganizationIdentifier, System = AddUrnOidIfOid(authorInstitution?.AssigningAuthority?.UniversalId) }];
            }
        }

        return organization;
    }

    private static Organization? GetOrganizationDepartmentFromAuthorInstitution(XON? authorDepartment)
    {
        var department = new Organization();
        if (authorDepartment != null)
        {
            department.Id = Guid.NewGuid().ToString();

            department.Name = authorDepartment?.OrganizationName;
            if (authorDepartment?.OrganizationIdentifier != null)
            {
                department.Identifier = [new() { Value = authorDepartment?.OrganizationIdentifier, System = AddUrnOidIfOid(authorDepartment?.AssigningAuthority?.UniversalId) }];
            }
        }

        return department;
    }

    private static PractitionerRole? GetPractitionerRoleFromAuthorRoleAndAuthorSpeciality(CX? authorSpeciality, CX? authorRole)
    {
        if (authorSpeciality == null && authorRole == null) return null;

        var practitionerRole = new PractitionerRole();

        practitionerRole.Id = Guid.NewGuid().ToString();

        if (authorRole != null)
        {
            var codeableConcept = new CodeableConcept()
            {
                Coding =
                [
                    new()
                    {
                        Code = authorRole.IdNumber,
                        System = AddUrnOidIfOid(authorRole.AssigningAuthority?.UniversalId)
                    }
                ]
            };

            practitionerRole.Code = [codeableConcept];
        }

        if (authorSpeciality != null)
        {
            var codeableConcept = new CodeableConcept()
            {
                Coding =
                [
                    new()
                    {
                        Code = authorSpeciality.IdNumber,
                        System = AddUrnOidIfOid(authorSpeciality.AssigningAuthority?.UniversalId)
                    }
                ]
            };

            practitionerRole.Specialty = [codeableConcept];
        }

        return practitionerRole;
    }

    private static Practitioner? GetPractitionerFromAuthorPerson(XCN? authorPerson)
    {
        if (authorPerson == null) return null;

        var practitioner = new Practitioner();
        practitioner.Id = Guid.NewGuid().ToString();


        // XML example of AuthorPerson Slot value:
        // <Value>502116685^GEIRAAS^BENEDIKTE^^^^^^&amp;urn:oid:2.16.578.1.12.4.1.4.4&amp;ISO</Value>
        // 
        // But it can also be simple text:
        // <Value>Gerald Smitty</Value>
        // We need to account for both

        // First, if authorPerson slot value is a properly formatted XCN Datatype
        if (authorPerson != null && authorPerson.GivenName != null && authorPerson.FamilyName != null)
        {
            practitioner.Name.Add(new() { Family = authorPerson.FamilyName, Given = [authorPerson.GivenName] });
            if (authorPerson.PersonIdentifier != null)
            {
                practitioner.Identifier = [new() { Value = authorPerson.PersonIdentifier, System = AddUrnOidIfOid(authorPerson.AssigningAuthority?.UniversalId) }];
            }
        }
        // If its just a plain text name (authorPerson.PersonIdentifier will contain the name)
        else if (authorPerson != null && authorPerson.GivenName == null && authorPerson.FamilyName == null)
        {
            var nameParts = authorPerson.PersonIdentifier?.Split(" ");
            practitioner.Name.Add(new() { Family = nameParts?.FirstOrDefault(), Given = nameParts?.Length > 1 ? nameParts.Skip(1) : null });
        }

        return practitioner;
    }

    private static Patient? GetPatientAsPatientResource(ExtrinsicObjectType assocExtrinsicObject)
    {
        var patient = new Patient();
        patient.Id = Guid.NewGuid().ToString();

        // Get slots from extrinsic object
        var sourcePatientInfo = assocExtrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.SourcePatientInfo)?.GetValues();
        var sourcePatientId = assocExtrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.SourcePatientId)?.GetFirstValue();

        if (sourcePatientInfo == null && sourcePatientId == null)
        {
            return null;
        }

        // Patient Name
        var patientName = sourcePatientInfo?.FirstOrDefault(st => st.Contains("PID-5"))?.Replace("PID-5|", "");
        var patientNameXpn = Hl7Object.Parse<XPN>(patientName);
        if (patientNameXpn != null)
        {
            patient.Name = [new() { Family = patientNameXpn.FamilyName, Given = [patientNameXpn.GivenName] }];
        }


        // Patient Id
        var patientIdCx = Hl7Object.Parse<CX>(sourcePatientId);
        if (patientIdCx != null)
        {
            patient.Identifier = [new() { Value = patientIdCx.IdNumber, System = AddUrnOidIfOid(patientIdCx.AssigningAuthority.UniversalId) }];
        }

        // Patient Gender
        var patientGender = sourcePatientInfo?.FirstOrDefault(st => st.Contains("PID-8"))?.Replace("PID-8|", "");
        patient.Gender = patientGender switch
        {
            "M" => AdministrativeGender.Male,
            "F" => AdministrativeGender.Female,
            "O" => AdministrativeGender.Other,
            "U" => AdministrativeGender.Unknown,
            _ => AdministrativeGender.Unknown
        };

        // Birth Date
        var patientBirthDate = sourcePatientInfo?.FirstOrDefault(st => st.Contains("PID-7"))?.Replace("PID-7|", "");
        if (patientBirthDate != null)
        {
            var patientBirthDateDtm = DateTime.ParseExact(patientBirthDate, Constants.Hl7.Dtm.DtmYmdFormat, CultureInfo.InvariantCulture);
            patient.BirthDate = patientBirthDateDtm.ToString(Constants.Hl7.Dtm.DtmFhirIsoDateFormat);
        }

        return patient;
    }

    private static List<CodeableConcept>? GetCategoryFromExtrinsicObjectClassCode(ExtrinsicObjectType assocExtrinsicObject)
    {
        var classCode = RegistryMetadataTransformerService.MapClassificationToCodedValue(assocExtrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.ClassCode));

        if (classCode == null) return null;

        var codeable = new CodeableConcept()
        {
            Coding = [
                new()
                {
                    Code = classCode.Code,
                    System = AddUrnOidIfOid(classCode.CodeSystem),
                    Display = classCode.DisplayName
                }
            ]
        };

        return [codeable];
    }

    private static CodeableConcept? GetTypeFromExtrinsicObjectTypeCode(ExtrinsicObjectType assocExtrinsicObject)
    {
        var typeCode = RegistryMetadataTransformerService.MapClassificationToCodedValue(assocExtrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.TypeCode));
        if (typeCode == null) return null;

        var codeable = new CodeableConcept()
        {
            Coding = [
                new()
                {
                    Code = typeCode.Code,
                    System = AddUrnOidIfOid(typeCode.CodeSystem),
                    Display = typeCode.DisplayName
                }
            ]
        };

        return codeable;
    }

    private static List<Identifier> GetIdentifierFromExtrinsicObjectId(ExtrinsicObjectType assocExtrinsicObject)
    {
        return [new() { Value = assocExtrinsicObject.Id }];
    }

    private static DocumentReferenceStatus GetDocumentReferenceStatusFromExtrinsicObjectStatus(ExtrinsicObjectType assocExtrinsicObject)
    {
        return assocExtrinsicObject.Status switch
        {
            Constants.Xds.StatusValues.Approved => DocumentReferenceStatus.Current,
            Constants.Xds.StatusValues.Deprecated => DocumentReferenceStatus.Superseded,
            _ => DocumentReferenceStatus.Current
        };
    }

    private static Identifier? GetMasterIdentifierFromRegistryPackageSourceId(RegistryPackageType? assocRegistryPackage, ExtrinsicObjectType assocExtrinsicObject)
    {
        var sourceId = assocRegistryPackage?.GetFirstExternalIdentifier(Constants.Xds.Uuids.SubmissionSet.SourceId)?.Value;
        var uniqueId = assocExtrinsicObject?.GetFirstExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.UniqueId)?.Value;

        if (sourceId == null || uniqueId == null) return null;

        return new()
        {
            Value = AddUrnOidIfOid(uniqueId),
            System = AddUrnOidIfOid(sourceId)
        };
    }
}
