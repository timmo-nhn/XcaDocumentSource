using System.Globalization;
using Hl7.Fhir.Model;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Services;

/// <summary>
/// XDS on FHIR functionality, supporting the Mobile access to Health Documents (MHD) - integration of the solution
/// See https://profiles.ihe.net/ITI/MHD/ for more information
/// </summary>
public static class XdsOnFhirService
{
    public static AdhocQueryRequest ConvertIti67ToIti18AdhocQuery(MhdDocumentRequest documentRequest)
    {
        var adhocQueryRequest = new AdhocQueryRequest();
        var adhocQuery = new AdhocQueryType();

        if (!string.IsNullOrWhiteSpace(documentRequest.Patient))
        {
            var patientCx = Hl7Object.Parse<CX>(documentRequest.Patient, '|');

            var patientOid = Hl7FhirExtensions.ParseNinToCxWithAssigningAuthority(documentRequest.Patient);

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

            classCodeCx.AssigningAuthority ??= new()
            {
                UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                UniversalId = Constants.Oid.CodeSystems.Volven.DocumentType
            };

            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.ClassCode, [classCodeCx.Serialize()]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Type))
        {
            var classCodeCx = Hl7Object.Parse<CX>(documentRequest.Category, '|');

            classCodeCx.AssigningAuthority ??= new()
            {
                UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                UniversalId = Constants.Oid.CodeSystems.Volven.DocumentType
            };

            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.ClassCode, [classCodeCx.Serialize()]);
        }

        if (!string.IsNullOrWhiteSpace(documentRequest.Event))
        {
            var eventCodeCx = Hl7Object.Parse<CX>(documentRequest.Event, '|');

            eventCodeCx.AssigningAuthority ??= new()
            {
                UniversalIdType = Constants.Hl7.UniversalIdType.Iso,
                UniversalId = Constants.Oid.CodeSystems.Volven.EventCode
            };

            adhocQuery.AddSlot(Constants.Xds.QueryParameters.FindDocuments.EventCodeList, [eventCodeCx.Serialize()]);
        }


        adhocQueryRequest.AdhocQuery = adhocQuery;

        return adhocQueryRequest;
    }

    public static Bundle TransformRegistryObjectsToFhirBundle(IdentifiableType[] registryObjectList)
    {
        if (registryObjectList.Length == 0)
        {
            return null;
        }

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

        var documentReference = GetFhirDocumentReferencesFromRegistryObjects(registryObjectList);

        bundle.Entry.AddRange(documentReference
            .Select(dr => new Bundle.EntryComponent()
            {
                FullUrl = Guid.NewGuid().ToString(),
                Resource = dr
            })
        );

        return bundle;
    }

    private static IEnumerable<DocumentReference> GetFhirDocumentReferencesFromRegistryObjects(IdentifiableType[] registryObjectList)
    {
        // Mapping table used to generate DocumentReference:
        // https://profiles.ihe.net/ITI/MHD/StructureDefinition-IHE.MHD.Minimal.DocumentReference-mappings.html#mappings-for-xds-and-mhd-mapping-xds

        var documentReferenceList = new List<DocumentReference>();

        var extrinsicObjects = registryObjectList.OfType<ExtrinsicObjectType>().ToArray();
        var registryPackages = registryObjectList.OfType<RegistryPackageType>().ToArray();
        var associations = registryObjectList.OfType<AssociationType>().ToArray();

        foreach (var association in associations)
        {
            if (association.AssociationTypeData is not Constants.Xds.AssociationType.HasMember) continue;

            var assocExtrinsicObject = extrinsicObjects.FirstOrDefault(eo => eo.Id.NoUrn() == association.TargetObject.NoUrn());
            var assocRegistryPackage = registryPackages.FirstOrDefault(rp => rp.Id.NoUrn() == association.SourceObject.NoUrn());

            var documentReference = new DocumentReference();

            if (assocRegistryPackage == null || assocExtrinsicObject == null) continue;

            documentReference.MasterIdentifier = GetMasterIdentifierFromExtrinsicObjectUniqueId(assocExtrinsicObject);
            documentReference.Identifier = GetIdentifierFromExtrinsicObjectId(assocExtrinsicObject);
            documentReference.Status = GetDocumentReferenceStatusFromExtrinsicObjectStatus(assocExtrinsicObject);
            documentReference.Type = GetTypeFromExtrinsicObjectTypeCode(assocExtrinsicObject);
            documentReference.Category = GetCategoryFromExtrinsicObjectClassCode(assocExtrinsicObject);

            // Add patient
            var patientResource = GetPatientAsPatientResource(assocExtrinsicObject);
            documentReference.Contained.Add(patientResource);
            documentReference.Subject = GetResourceAsResourceReference(patientResource);

            // Add Author
            var authorResources = GetAuthorRelatedAsResourceList(assocExtrinsicObject);
            documentReference.Contained.AddRange(authorResources);
            documentReference.Author = GetResourceAsResourceReference(authorResources);

            // Authenticator
            var authenticator = GetAuthenticatorFromExtrinsicObjectLegalAuthenticator(assocExtrinsicObject);
            documentReference.Contained.Add(authenticator);
            documentReference.Authenticator = GetResourceAsResourceReference(authenticator);

            // RelatesTo
            /*

             ===== Yet to be implemented =====

             */

            // SecurityLabel
            var securityLabel = GetCodeableConceptFromExtrinsicObjectConfidentialityCode(assocExtrinsicObject);


            documentReferenceList.Add(documentReference);
        }

        return documentReferenceList;
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
                    System = confidentialityCode.CodeSystem,
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


        var authorPerson = Hl7Object.Parse<XCN>(authorClassification.GetSlots(Constants.Xds.SlotNames.AuthorPerson)?.GetValues().FirstOrDefault());
        var practitioner = GetPractitionerFromAuthorPerson(authorPerson);

        if (practitioner != null)
        {
            resourceList.Add(practitioner);
        }


        var authorInstitution = new XON();
        var authorDepartment = new XON();
        var authorInstitutionValues = authorClassification.GetSlots(Constants.Xds.SlotNames.AuthorInstitution)?.GetValues().Select(auth => Hl7Object.Parse<XON>(auth)).ToList();

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


        var authorSpeciality = Hl7Object.Parse<CX>(authorClassification.GetSlots(Constants.Xds.SlotNames.AuthorSpecialty)?.GetValues().FirstOrDefault());
        var authorRole = Hl7Object.Parse<CX>(authorClassification.GetSlots(Constants.Xds.SlotNames.AuthorRole)?.GetValues().FirstOrDefault());

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
                organization.Identifier = [new() { Value = authorInstitution?.OrganizationIdentifier, System = authorInstitution?.AssigningAuthority?.UniversalId }];
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
                department.Identifier = [new() { Value = authorDepartment?.OrganizationIdentifier, System = authorDepartment?.AssigningAuthority?.UniversalId }];
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
                        System = authorRole.AssigningAuthority?.UniversalId
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
                        System = authorSpeciality.AssigningAuthority?.UniversalId
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
                practitioner.Identifier = [new() { Value = authorPerson.PersonIdentifier, System = authorPerson.AssigningAuthority?.UniversalId }];
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

    private static Patient GetPatientAsPatientResource(ExtrinsicObjectType assocExtrinsicObject)
    {
        var patient = new Patient();
        patient.Id = Guid.NewGuid().ToString();

        // Get slots from extrinsic object
        var sourcePatientInfo = assocExtrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.SourcePatientInfo)?.GetValues();
        var sourcePatientId = assocExtrinsicObject.GetFirstSlot(Constants.Xds.SlotNames.SourcePatientId)?.GetFirstValue();


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
            patient.Identifier = [new() { Value = patientIdCx.IdNumber, System = patientIdCx.AssigningAuthority.UniversalId }];
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
            patient.BirthDate = patientBirthDateDtm.ToString(Constants.Hl7.Dtm.DtmFhirIsoDateTimeFormat);
        }

        return patient;
    }

    private static ResourceReference GetSubjectReferenceFromPatientResource(Resource resource)
    {
        return new ResourceReference()
        {
            Reference = $"#{resource.Id}"
        };
    }

    private static List<CodeableConcept> GetCategoryFromExtrinsicObjectClassCode(ExtrinsicObjectType assocExtrinsicObject)
    {
        var typeCode = RegistryMetadataTransformerService.MapClassificationToCodedValue(assocExtrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.ClassCode));

        if (typeCode == null) return null;

        var codeable = new CodeableConcept()
        {
            Coding = [
                new()
                {
                    System = typeCode.CodeSystem,
                    Display = typeCode.DisplayName,
                    Code = typeCode.Code
                }
            ]
        };

        return [codeable];
    }

    private static CodeableConcept GetTypeFromExtrinsicObjectTypeCode(ExtrinsicObjectType assocExtrinsicObject)
    {
        var typeCode = RegistryMetadataTransformerService.MapClassificationToCodedValue(assocExtrinsicObject.GetFirstClassification(Constants.Xds.Uuids.DocumentEntry.TypeCode));
        if (typeCode == null) return null;

        var codeable = new CodeableConcept()
        {
            Coding = [
                new()
                {
                    System = typeCode.CodeSystem,
                    Display = typeCode.DisplayName,
                    Code = typeCode.Code
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

    private static Identifier GetMasterIdentifierFromExtrinsicObjectUniqueId(ExtrinsicObjectType? assocExtrinsicObject)
    {
        var masterIdentifier = Hl7Object.Parse<CX>(assocExtrinsicObject.GetFirstExternalIdentifier(Constants.Xds.Uuids.DocumentEntry.UniqueId).Value);

        if (masterIdentifier == null) return null;

        return new()
        {
            Value = masterIdentifier.IdNumber,
            System = masterIdentifier?.AssigningAuthority?.UniversalId
        };
    }

    // Helpers
    private static ResourceReference GetResourceAsResourceReference(Resource resource)
    {
        return new ResourceReference() { Reference = $"#{resource.Id}" };
    }

    private static List<ResourceReference> GetResourceAsResourceReference(List<Resource> resource)
    {
        return resource.Select(res => GetResourceAsResourceReference(res)).ToList();
    }
}
