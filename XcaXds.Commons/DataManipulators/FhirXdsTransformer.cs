using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using static XcaXds.Commons.Commons.Constants;

namespace XcaXds.Commons.DataManipulators;

public static class FhirXdsTransformer
{
    public static List<RegistryObjectDto> TransformFhirResourceToRegistryObjectDto(Bundle.EntryComponent bundleEntry)
    {
        var fhirDocumentReference = (DocumentReference?)bundleEntry.Resource;

        var registryObjectList = new List<RegistryObjectDto>();

        var documentEntry = new DocumentEntryDto();

        documentEntry.Id = bundleEntry.Resource?.Id ?? Guid.NewGuid().ToString();
        documentEntry.Author = GetDocumentEntryAuthorsFromFhirDocumentReference(fhirDocumentReference);

        var submissionSet = new SubmissionSetDto();


        return registryObjectList;
    }

    private static List<AuthorInfo>? GetDocumentEntryAuthorsFromFhirDocumentReference(DocumentReference? documentReference)
    {
        if (documentReference == null) return null;

        var author = new List<AuthorInfo>();

        return author;
    }

    public static ServiceResultDto<ProvideAndRegisterDocumentSetbRequest> CreateSoapObjectFromComprehensiveBundle(Bundle bundle, Patient bundlePatient, List<DocumentReference> documentReferences, List submissionSetList, List<Binary> fhirBinaries, Identifier patientIdentifier, string GpiOid, string homeCommunityId)
    {
        var operationOutcome = new OperationOutcome();

        var registryPackageResult = ConvertSubmissionSetListAndDocumentReferenceToRegistryPackage(bundlePatient, submissionSetList, patientIdentifier, GpiOid, homeCommunityId);
        if (!registryPackageResult.Success)
        {
            operationOutcome.AddIssue(registryPackageResult.OperationOutcome?.Issue);
        }

        var registryPackage = registryPackageResult.Value;
        var extrinsicObjects = new List<ExtrinsicObjectType>();
        var documents = new List<DocumentType>();
        var associations = new List<AssociationType>();

        for (var i = 0; i < documentReferences.Count; i++)
        {
            var documentReference = documentReferences[i];

            var fhirBinary = MatchBinaryToDocumentReference(
                bundle: bundle,
                documentReference: documentReference,
                indexFallback: i,
                binaries: fhirBinaries,
                operationOutcome: operationOutcome);

            var extrinsicResult = ConvertDocumentReferenceToExtrinsicObject(bundlePatient, documentReference, patientIdentifier, GpiOid);
            if (!extrinsicResult.Success)
            {
                operationOutcome.AddIssue(extrinsicResult.OperationOutcome?.Issue);
            }
            if (extrinsicResult.Value != null)
            {
                extrinsicObjects.Add(extrinsicResult.Value);
            }

            // Only add Document element if Binary exists (metadata-only should be allowed)
            if (fhirBinary != null)
            {
                var documentResult = ConvertBinaryToDocument(fhirBinary, extrinsicResult.Value);
                if (!documentResult.Success)
                {
                    operationOutcome.AddIssue(documentResult.OperationOutcome?.Issue);
                }
                if (documentResult.Value != null)
                {
                    documents.Add(documentResult.Value);
                }
            }

            var validationOutcome = ValidateDocumentRelations(documentReference);
            if (validationOutcome.Issue.Any(i => i.Severity == OperationOutcome.IssueSeverity.Error))
            {
                operationOutcome.AddIssue(validationOutcome.Issue);
                continue; // or fail fast
            }

            var sourceExtrinsicId = extrinsicResult.Value?.Id;

            var assocResult = CreateAssociationForSubmissionSet(extrinsicResult.Value, registryPackage);
            if (!assocResult.Success)
            {
                operationOutcome.AddIssue(assocResult.OperationOutcome?.Issue);
            }
            if (assocResult.Value != null)
            {
                associations.Add(assocResult.Value);
            }

            // Map relatesTo → XDS associations (0..N)
            var relationAssociations =
                MapRelationsToXdsAssociations(documentReference, sourceExtrinsicId);

            if (relationAssociations != null)
            {
                associations.AddRange(relationAssociations);
            }
        }

        List<IdentifiableType> combinedRegistryObjects = [.. extrinsicObjects, .. associations];
        if (registryPackage != null)
        {
            combinedRegistryObjects.Add(registryPackage);
        }

        var request = new ProvideAndRegisterDocumentSetbRequest
        {
            ProvideAndRegisterDocumentSetRequest = new ProvideAndRegisterDocumentSetRequestType
            {
                SubmitObjectsRequest = new SubmitObjectsRequest
                {
                    RegistryObjectList = combinedRegistryObjects.ToArray()
                },
                // Document list can be empty for metadata-only submissions
                Document = documents.Count > 0 ? [.. documents] : []
            }
        };

        // We're now sending both:
        // HasMember associations(SubmissionSet -> DocumentEntry)
        // Relation associations(New DocumentEntry -> Old DocumentEntry), i.e.RPLC / APND / XFRM / SIGN

        var creationResult = new ServiceResultDto<ProvideAndRegisterDocumentSetbRequest>()
        {
            Value = request,
            OperationOutcome = operationOutcome
        };

        return creationResult;
    }

    private static ServiceResultDto<RegistryPackageType> ConvertSubmissionSetListAndDocumentReferenceToRegistryPackage(Patient bundlePatient, List submissionSetList, Identifier patientId, string GpiOid, string homeCommunityId)
    {
        var operationOutcome = new OperationOutcome();

        var registryPackage = new RegistryPackageType
        {
            Id = submissionSetList.Id,
            Name = new InternationalStringType($"{submissionSetList.Title}"),
            ObjectType = Constants.Xds.ObjectTypes.RegistryPackage,
            Classification = [],
            ExternalIdentifier = [],
        };

        if (submissionSetList != null)
        {
            // note.Text is nullable, so we need to filter out null values before adding to comments slot
            var comment = submissionSetList.Note.Select(note => note.Text).OfType<string>().Where(text => !string.IsNullOrWhiteSpace(text)).ToArray();

            // Comment from submission
            if (comment?.Length > 0)
            {
                registryPackage.AddSlot(new SlotType
                {
                    Name = "comments",
                    ValueList = new ValueListType
                    {
                        Value = comment
                    }
                });
            }

            //SubmissionTime
            if (submissionSetList.Date != null && DateTime.TryParse(submissionSetList.Date, out var submissionTime))
            {
                registryPackage.AddSlot(new SlotType
                {
                    Name = "submissionTime",
                    ValueList = new ValueListType
                    {
                        Value = [submissionTime.ToUniversalTime().ToString(Constants.Hl7.Dtm.DtmFormat)]
                    }
                });
            }
            else
            {
                operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.NotFound,
                    Diagnostics = "submissiontime not found or format is invalid.",
                    Location = ["SubmissionsetList.Date"]
                });
            }

            // Classify the registryPackage as a submissionSet
            if (!string.IsNullOrWhiteSpace(submissionSetList.Id))
            {
                registryPackage.AddClassification(new ClassificationType()
                {
                    Id = Guid.NewGuid().ToString(),
                    ClassificationNode = Constants.Xds.Uuids.SubmissionSet.SubmissionSetClassificationNode,
                    ClassifiedObject = submissionSetList.Id,
                    ObjectType = Constants.Xds.ObjectTypes.ExternalIdentifier,

                });
            }
            else
            {
                operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.NotFound,
                    Diagnostics = "No unique id found.",
                    Location = ["SubmissionsetList.identifier"]
                });
            }

            // XDS SubmAuthor
            var submAuthorOrg = GetAuthorOrganization(submissionSetList);

            if (submAuthorOrg == null)
            {
                operationOutcome.AddIssue(new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Warning,
                    Code = OperationOutcome.IssueType.NotFound,
                    Diagnostics = "No author organizations for submission was found",
                    Location = ["List.identifier"]
                });
            }

            // Department
            var submAuthorDept = GetAuthorDepartment(submissionSetList);

            if (submAuthorDept == null)
            {
                operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Warning,
                    Code = OperationOutcome.IssueType.NotFound,
                    Diagnostics = "No author departments for submission was found.",
                    Location = ["List.identifier"]
                });
            }

            var submAuthorPerson = GetAuthorPerson(submissionSetList);
            if (submAuthorPerson == null)
            {
                operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Warning,
                    Code = OperationOutcome.IssueType.NotFound,
                    Diagnostics = "No author person for submission was found",
                    Location = ["List.identifier"]
                });

            }
            if (submAuthorOrg != null && submAuthorPerson != null)
            {
                var submAuthorOrgNameOnly = new XON()
                {
                    OrganizationName = submAuthorOrg.OrganizationName
                };

                var subAuthorString = submAuthorDept?.Serialize();

                string[] items = [
                            submAuthorOrgNameOnly.Serialize(),
                            submAuthorOrg.Serialize(),
                    ];

                if (!string.IsNullOrWhiteSpace(subAuthorString))
                {
                    items = [.. items, subAuthorString];
                }

                var authorClassification = new ClassificationType()
                {
                    Id = Guid.NewGuid().ToString(),
                    ClassifiedObject = submissionSetList.Id!.Replace("urn:uuid:", ""),
                    ClassificationScheme = Constants.Xds.Uuids.SubmissionSet.Author,
                    ObjectType = Constants.Xds.ObjectTypes.Classification,
                    Name = new InternationalStringType(Constants.Xds.ClassificationNames.SubmissionSetAuthor),
                    NodeRepresentation = string.Empty,
                    Slot = []
                };

                var authorDepartmentSlot = new SlotType()
                {
                    Name = "authorInstitution",
                    ValueList = new ValueListType()
                    {
                        Value =
                        [
                            submAuthorOrgNameOnly.Serialize(),
                            submAuthorOrg.Serialize(),
                        ]
                    }
                };

                var authorDepartmentString = submAuthorDept?.Serialize();
                if (!string.IsNullOrWhiteSpace(authorDepartmentString))
                {
                    authorDepartmentSlot.AddValue(authorDepartmentString);
                }

                var authorPersonSlot = new SlotType()
                {
                    Name = "authorPerson",
                    ValueList = new ValueListType()
                    {
                        Value = [submAuthorPerson.Serialize().Replace("&&", "")]
                    }
                };

                authorClassification.AddSlot(authorDepartmentSlot);
                authorClassification.AddSlot(authorPersonSlot);

                registryPackage.AddClassification(authorClassification);
            }

            // XdsSubmissionset.ContentTypeCode (Document type)
            var submissionConfCode = submissionSetList.GetExtension("https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-designationType");
            if (submissionConfCode.Value is CodeableConcept valueCodeableConcept && valueCodeableConcept.Coding != null)
            {
                var submissionConcept = valueCodeableConcept.Coding.First();

                var submissionConfClassification = new ClassificationType()
                {
                    Id = Guid.NewGuid().ToString(),
                    ClassifiedObject = submissionSetList.Id ?? "Unknown",
                    ObjectType = Constants.Xds.ObjectTypes.Classification,
                    ClassificationScheme = Constants.Xds.Uuids.SubmissionSet.ContentTypeCode,
                    NodeRepresentation = submissionConcept.Code ?? "Unknown",
                    Slot = []
                };

                if (!string.IsNullOrWhiteSpace(submissionConcept.System))
                {
                    submissionConfClassification.AddSlot(new SlotType()
                    {
                        Name = "codingScheme",
                        ValueList = new()
                        {
                            Value = [submissionConcept.System.NoUrn()]
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(submissionConcept.Display))
                {
                    submissionConfClassification.Name = new InternationalStringType(submissionConcept.Display);
                }

                registryPackage.AddClassification(submissionConfClassification);
            }
            else
            {
                operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.NotFound,
                    Diagnostics = "No document type found.\n should be defined in Bundle with extension https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-designationType",
                    Location = ["SubmissionsetList.extension"]
                });
            }

            // XDSSubmissionSet.uniqueId
            if (!string.IsNullOrWhiteSpace(submissionSetList.Id))
            {
                registryPackage.AddExternalIdentifier(new ExternalIdentifierType()
                {
                    Id = submissionSetList.Id,
                    IdentificationScheme = Constants.Xds.Uuids.SubmissionSet.UniqueId,
                    RegistryObject = submissionSetList.Id,
                    ObjectType = Constants.Xds.ObjectTypes.ExternalIdentifier,
                    Value = GenerateRandomOid(),                                                // ?TBD func(UUID -> OID) => 2.25.XXXXX
                    Name = new InternationalStringType(Constants.Xds.ExternalIdentifierNames.SubmissionSetUniqueId)
                });
            }
            else
            {
                operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.NotFound,
                    Diagnostics = "No unique id found.",
                    Location = ["SubmissionsetList.identifier"]
                });
            }

            // XDSSubmissionSet.sourceId
            var documentOrganization = submissionSetList!.Contained!
                .OfType<Organization>()
                .FirstOrDefault(dpt => dpt?.PartOf == null);

            //Get Extension for SourceId
            string? sourceId = null;

            try
            {
                //Get Extension for SourceId
                var extSourceId = submissionSetList.GetExtension("https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-sourceId");
                var extResReference = extSourceId!.Value as Identifier; // Changed from reference to identifier
                sourceId = extResReference!.Value?.NoUrn();
            }
            catch
            {
                operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.NotFound,
                    Diagnostics = "No extension for sourceId was found.",
                    Location = ["List.Source.Extension"]
                });
            }

            if (documentOrganization != null)
            {
                // If extension is not missing this will be default value
                var value = $"{Constants.Oid.Brreg}.{documentOrganization?.Identifier?.FirstOrDefault()?.Value}";

                // Replace default value
                if (!string.IsNullOrEmpty(sourceId))
                {
                    value = sourceId.NoUrn();
                }

                registryPackage.AddExternalIdentifier(new ExternalIdentifierType()
                {
                    Id = Guid.NewGuid().ToString(),
                    IdentificationScheme = Constants.Xds.Uuids.SubmissionSet.SourceId,
                    RegistryObject = submissionSetList.Id ?? "Unknown",
                    ObjectType = Constants.Xds.ObjectTypes.ExternalIdentifier,
                    Value = value,
                    Name = new InternationalStringType(Constants.Xds.ExternalIdentifierNames.SubmissionSetSourceId)
                });
            }
            else
            {
                operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.NotFound,
                    Diagnostics = "No organization found.",
                    Location = ["SubmissionSetList.contained.Organization"]
                });
            }

            // XDSSubmissionSet.patientId
            //var patientIdFromPix = GetPatient(patientId, GpiOid);
            //var patientIdFromPix = GetPatient(patientId, sourceId);
            var patientIdFromPix = GetPatient(patientId, homeCommunityId)?.Serialize();

            if (!string.IsNullOrWhiteSpace(submissionSetList.Id) && !string.IsNullOrWhiteSpace(patientIdFromPix))
            {
                registryPackage.AddExternalIdentifier(new ExternalIdentifierType()
                {
                    Id = Guid.NewGuid().ToString(),
                    IdentificationScheme = Constants.Xds.Uuids.SubmissionSet.PatientId,
                    RegistryObject = submissionSetList.Id ?? "Unknown",
                    ObjectType = Constants.Xds.ObjectTypes.ExternalIdentifier,
                    Name = new InternationalStringType(Constants.Xds.ExternalIdentifierNames.SubmissionSetPatientId),
                    Value = patientIdFromPix
                });
            }
            else
            {
                operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.NotFound,
                    Diagnostics = "No patient id found.",
                    Location = ["SubmissionsetList.identifier"]
                });
            }

            return new ServiceResultDto<RegistryPackageType>()
            {
                OperationOutcome = operationOutcome,
                Value = registryPackage
            };
        }
        else
        {
            return null!;
        }
    }

    private static ServiceResultDto<ExtrinsicObjectType> ConvertDocumentReferenceToExtrinsicObject(Patient bundlePatient, DocumentReference documentReference, Identifier patientId, string GpiOid)
    {
        var operationOutcome = new OperationOutcome();

        var statusType = documentReference.Status switch
        {
            DocumentReferenceStatus.Current => "urn:oasis:names:tc:ebxml-regrep:StatusType:Approved",
            DocumentReferenceStatus.Superseded => "urn:oasis:names:tc:ebxml-regrep:StatusType:Deprecated",
            _ => "urn:oasis:names:tc:ebxml-regrep:StatusType:Deprecated"
        };

        var attachment = documentReference.Content.First().Attachment;
        var documentCreationTime = DateTime.Parse(attachment.Creation ?? DateTime.MinValue.ToString()).ToUniversalTime();

        var patient = new CX()
        {
            IdNumber = patientId.Value ?? "Unknown",
            AssigningAuthority = new HD()
            {
                NamespaceId = $"&{GpiOid}&",
                UniversalIdType = "ISO"
            },
        };

        var extrinsicObject = new ExtrinsicObjectType
        {
            MimeType = attachment.ContentType ?? "Unknown",
            Id = documentReference.Id?.NoUrn(),
            Status = statusType.ToString(),
            Name = new InternationalStringType(attachment.Title ?? "Unknown"),
            ObjectType = Constants.Xds.Uuids.DocumentEntry.StableDocumentEntries,
            Slot =
            [
                /* XDSDocumentEntry.creationTime - mandatory */
                new SlotType
                {
                    Name = "creationTime",
                    ValueList = new ValueListType
                    {
                        Value = [documentCreationTime.ToString(Constants.Hl7.Dtm.DtmFormat)]
                    }
                },
                /* XDSDocumentEntry.languageCode - mandatory */
                new SlotType
                {
                    Name = "languageCode",
                    ValueList = new ValueListType
                    {
                        Value = [attachment.Language ?? "Unknown"]
                    }

                },
                /* XDSDocumentEntry.sourcePatientId - mandatory */
                new SlotType
                {
                    Name = "sourcePatientId",
                    ValueList = new ValueListType
                    {
                        Value = [patient.Serialize().Replace("&&", "")]
                    }
                }
            ]
        };

        // serviceTime elements - optional, but required if known
        /* XDSDocumentEntry.serviceStartTime */
        if (!string.IsNullOrEmpty(documentReference.Context?.Period?.Start))
        {
            var datePeriodFrom = DateTime.Parse(documentReference.Context.Period.Start);
            extrinsicObject.AddSlot(new SlotType
            {
                Name = "serviceStartTime",
                ValueList = new ValueListType
                {
                    Value = [datePeriodFrom.ToString(Constants.Hl7.Dtm.DtmFormat)]
                }
            });
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Warning,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No serviceStartTime found. (optional, but required if known)",
                Location = ["DocumentReference.Context.Period.Start"]
            });
        }

        /* XDSDocumentEntry.serviceStopTime */
        if (!string.IsNullOrEmpty(documentReference.Context?.Period?.End))
        {
            var datePeriodTo = DateTime.Parse(documentReference.Context.Period.End);
            extrinsicObject.AddSlot(new SlotType
            {
                Name = "serviceStopTime",
                ValueList = new ValueListType
                {
                    Value = [datePeriodTo.ToString(Constants.Hl7.Dtm.DtmFormat)]
                }
            });
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Warning,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No serviceStopTime found. (optional, but required if known)",
                Location = ["DocumentReference.Context.Period.Start"]
            });
        }


        /* XDSDocumentEntry.author */

        // Build list of resource references for each category 
        var listOrganization = new List<ResourceReference>();
        var listPractitioner = new List<ResourceReference>();
        var listPractitionerRole = new List<ResourceReference>();
        foreach (var authorReference in documentReference.Author)
        {
            switch (GetAuthorReferenceTarget(documentReference, authorReference))
            {
                case "Organization":
                    listOrganization.Add(authorReference);
                    break;
                case "Practitioner":
                    listPractitioner.Add(authorReference);
                    break;
                case "PractitionerRole":
                    listPractitionerRole.Add(authorReference);
                    break;
                default:
                    operationOutcome.AddIssue(new OperationOutcome.IssueComponent
                    {
                        Severity = OperationOutcome.IssueSeverity.Error,
                        Code = OperationOutcome.IssueType.Unknown,
                        Diagnostics = "Unexpected identifier type found. Not any of Organization, Practitioner or PractitionerRole",
                        Location = ["DocumentReference.Author"]
                    });
                    break;
            }
            ;
        }

        /*- Special case => just 1 practitioner and 1 organization provided in DocumentReference - without any practitinerRole -*/
        if ((listPractitioner.Count == 1) && (listOrganization.Count == 1) && (listPractitionerRole.Count == 0))
        {
            var listAuthorSlots = new List<SlotType>();
            var practitionerReference = listPractitioner.First();
            var orgReference = listOrganization.First();

            var listProcessedOrganization = new List<ResourceReference>();

            // Practitioner
            AddAuthorPersonSlot(documentReference, practitionerReference, ref listAuthorSlots, ref operationOutcome);
            listPractitioner.Remove(practitionerReference);

            // Organization
            AddAuthorInstitutionSlot(documentReference, orgReference, ref listOrganization, ref listProcessedOrganization, ref listAuthorSlots, ref operationOutcome);
            listOrganization.Remove(orgReference);

            extrinsicObject.AddClassification(new ClassificationType()
            {
                Id = Guid.NewGuid().ToString(),
                ClassifiedObject = documentReference.Id?.NoUrn() ?? "Unknown",
                ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.Author,
                ObjectType = Constants.Xds.ObjectTypes.Classification,
                NodeRepresentation = string.Empty,
                Slot = [.. listAuthorSlots]
            });
        }

        // Each practitioner has its own classification
        if (listPractitioner.Count > 0)
        {
            var listProcessedPractitionerRole = new List<ResourceReference>();
            var listProcessedPractitioner = new List<ResourceReference>();
            var listProcessedOrganization = new List<ResourceReference>();
            foreach (var practitionerReference in listPractitioner)
            {
                // Slots for each author
                var listAuthorSlots = new List<SlotType>();

                // Practitioner
                AddAuthorPersonSlot(documentReference, practitionerReference, ref listAuthorSlots, ref operationOutcome);

                foreach (var roleReference in listPractitionerRole)
                {
                    GetAuthorRefsAndRoleAndSpecialty(documentReference, roleReference, practitionerReference,
                        out var orgReference,
                        out var authorRole,
                        out var authorSpecialty);

                    // Process just in case that there is an organization-reference, otherwise just jump over
                    // Neccessary for author.count > 1
                    if (orgReference != null)
                    {
                        // Organization
                        AddAuthorInstitutionSlot(documentReference, orgReference,
                            ref listOrganization,
                            ref listProcessedOrganization,
                            ref listAuthorSlots,
                            ref operationOutcome);
                        listProcessedOrganization.Add(orgReference);

                        // Role
                        AddAuthorRoleSlot(authorRole, ref listAuthorSlots, ref operationOutcome);

                        // Specialty
                        AddAuthorSpecialtySlot(authorSpecialty, ref listAuthorSlots, ref operationOutcome);

                        // Add processed reference of PractitionerRole to processed list
                        listProcessedPractitionerRole.Add(roleReference);

                        extrinsicObject.AddClassification(new ClassificationType()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ClassifiedObject = documentReference.Id?.NoUrn() ?? "Unknown",
                            ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.Author,
                            ObjectType = Constants.Xds.ObjectTypes.Classification,
                            NodeRepresentation = string.Empty,
                            Slot = [.. listAuthorSlots]
                        });
                    }
                }
                // Remove processed PractitionerRole from main list
                foreach (var processedRole in listProcessedPractitionerRole)
                {
                    listPractitionerRole.Remove(processedRole);
                }
                listProcessedPractitionerRole.Clear();

                // Add processed reference of PractitionerRole
                listProcessedPractitioner.Add(practitionerReference);
            }

            // Remove processed Practitioner from main processing list
            foreach (var processedPractitioner in listProcessedPractitioner)
            {
                listPractitioner.Remove(processedPractitioner);
            }
            listProcessedPractitioner.Clear();

            // Remove processed Organization from main processing list
            foreach (var processedOrganization in listProcessedOrganization)
            {
                var processedOrg = listOrganization.Where(x => x.Reference == processedOrganization.Reference).FirstOrDefault();
                if (processedOrg != null)
                {
                    listOrganization.Remove(processedOrg);
                }
            }
            listProcessedOrganization.Clear();

            // Just in case there is no Practitioner present at all
            if (listPractitionerRole.Count > 0)
            {
                // Slots for each author
                var listAuthorSlots = new List<SlotType>();

                foreach (var roleReference in listPractitionerRole)
                {
                    // Build organization slots for PractitionerRole
                    GetAuthorRefsAndRoleAndSpecialty(documentReference, roleReference,
                            out var orgReference,
                            out var authorRole,
                            out var authorSpecialty);

                    // Organization
                    AddAuthorInstitutionSlot(documentReference, orgReference, ref listOrganization, ref listProcessedOrganization, ref listAuthorSlots, ref operationOutcome);
                    listProcessedOrganization.Add(orgReference);

                    // Role
                    AddAuthorRoleSlot(authorRole, ref listAuthorSlots, ref operationOutcome);

                    // Specialty
                    AddAuthorSpecialtySlot(authorSpecialty, ref listAuthorSlots, ref operationOutcome);

                    extrinsicObject.AddClassification(new ClassificationType()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ClassifiedObject = documentReference.Id?.NoUrn() ?? "Unknown",
                        ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.Author,
                        ObjectType = Constants.Xds.ObjectTypes.Classification,
                        NodeRepresentation = string.Empty,
                        Slot = [.. listAuthorSlots]
                    });

                    listProcessedPractitionerRole.Add(roleReference);
                }

                // Remove processed PractitionerRole from main list
                foreach (var processedRole in listProcessedPractitionerRole)
                {
                    listPractitionerRole.Remove(processedRole);
                }
                listProcessedPractitionerRole.Clear();

                // Remove processed Organization from main list
                foreach (var processedOrganization in listProcessedOrganization)
                {
                    var processedOrganizations = listOrganization.Where(x => x.Reference == processedOrganization.Reference).FirstOrDefault();

                    if (processedOrganizations != null)
                    {
                        listOrganization.Remove(processedOrganizations);
                    }
                }
                listProcessedOrganization.Clear();
            }

            // If there are only organization's details left
            if (listOrganization.Count > 0)
            {
                // Slots for each author
                var listAuthorSlots = new List<SlotType>();

                foreach (var orgReference in listOrganization)
                {
                    // Organization
                    AddAuthorInstitutionSlot(documentReference, orgReference, ref listOrganization, ref listProcessedOrganization, ref listAuthorSlots, ref operationOutcome);
                    listProcessedOrganization.Add(orgReference);
                }

                extrinsicObject.AddClassification(new ClassificationType()
                {
                    Id = Guid.NewGuid().ToString(),
                    ClassifiedObject = documentReference.Id?.NoUrn() ?? "Unknown",
                    ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.Author,
                    ObjectType = Constants.Xds.ObjectTypes.Classification,
                    NodeRepresentation = string.Empty,
                    Slot = [.. listAuthorSlots]
                });

                // Remove processed Organization from main list
                foreach (var processedOrganization in listProcessedOrganization)
                {
                    var processedOrganizations = listOrganization.Where(x => x.Reference == processedOrganization.Reference).FirstOrDefault();

                    if (processedOrganizations != null)
                    {
                        listOrganization.Remove(processedOrganizations);
                    }
                }
                listProcessedOrganization.Clear();
            }
        }

        /* XDSDocumentEntry.formatCode */
        var contentType = documentReference.Content.FirstOrDefault()?.Format;
        if (contentType != null || contentType?.Code != null)
        {
            var contentTypeClassification = new ClassificationType()
            {
                Id = Guid.NewGuid().ToString(),
                ClassifiedObject = documentReference.Id?.NoUrn() ?? "Unknown",
                ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.FormatCode,
                ObjectType = Constants.Xds.ObjectTypes.Classification,
                NodeRepresentation = contentType.Code ?? "Unknown",
                Slot = []
            };

            var name = contentType.Display ?? contentType.Code;
            if (!string.IsNullOrEmpty(name))
            {
                contentTypeClassification.Name = new InternationalStringType(name);
            }

            if (!string.IsNullOrWhiteSpace(contentType.System))
            {
                contentTypeClassification.AddSlot(new SlotType()
                {
                    Name = "codingScheme",
                    ValueList = new ValueListType()
                    {
                        Value =
                        [contentType.System]
                    }
                });
            }

            extrinsicObject.AddClassification(contentTypeClassification);
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No content type code found",
                Location = ["DocumentReference.content[].Format"]
            });

        }

        /* XDSDocumentEntry.HealthcareFacilityTypeCode */
        var healthcareFacilityType = documentReference.Context?.FacilityType?.Coding.FirstOrDefault();

        if (healthcareFacilityType != null)
        {
            var healthcareFacilityTypeClassification = new ClassificationType
            {
                Id = Guid.NewGuid().ToString(),
                ClassifiedObject = documentReference.Id ?? "Unknown",
                ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.HealthCareFacilityTypeCode,
                ObjectType = Constants.Xds.ObjectTypes.Classification,
                NodeRepresentation = healthcareFacilityType.Code ?? "Unknown",
                Slot = []
            };

            var name = healthcareFacilityType.Display ?? healthcareFacilityType.Code;

            if (!string.IsNullOrWhiteSpace(name))
            {
                healthcareFacilityTypeClassification.Name = new InternationalStringType(name);
            }

            if (!string.IsNullOrWhiteSpace(healthcareFacilityType.System))
            {
                healthcareFacilityTypeClassification.AddSlot(new SlotType
                {
                    Name = "codingScheme",
                    ValueList = new ValueListType
                    {
                        Value = [healthcareFacilityType.System.NoUrn()]
                    }
                });
            }

            extrinsicObject.AddClassification(healthcareFacilityTypeClassification);

        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No facilityType code found",
                Location = ["DocumentReference.context"]
            });
        }


        /* XDSDocumentEntry.PracticeSettingCode */
        var practiceSetting = documentReference.Context?.PracticeSetting?.Coding.FirstOrDefault();

        if (practiceSetting != null)
        {
            var practiceSettingClassification = new ClassificationType
            {
                Id = Guid.NewGuid().ToString(),
                ClassifiedObject = documentReference.Id ?? "Unknown",
                ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.PracticeSettingCode,
                ObjectType = Constants.Xds.ObjectTypes.Classification,
                NodeRepresentation = practiceSetting.Code ?? "Unknown",
                Slot = []
            };

            var name = practiceSetting.Display ?? practiceSetting.Code;

            if (!string.IsNullOrWhiteSpace(name))
            {
                practiceSettingClassification.Name = new InternationalStringType(name);
            }

            if (!string.IsNullOrWhiteSpace(practiceSetting.System))
            {
                practiceSettingClassification.AddSlot(new SlotType
                {
                    Name = "codingScheme",
                    ValueList = new ValueListType
                    {
                        Value = [practiceSetting.System.NoUrn()]
                    }
                });
            }

            extrinsicObject.AddClassification(practiceSettingClassification);
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No practiceSetting found",
                Location = ["DocumentReference.context"]
            });
        }


        /* XDSDocumentEntry.classCode */
        var classCode = documentReference.Category.First().Coding.First();
        if (classCode != null)
        {
            var classCodeClassification = new ClassificationType
            {
                Id = Guid.NewGuid().ToString(),
                ClassifiedObject = documentReference?.Id?.NoUrn() ?? "Unknown",
                ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.ClassCode,
                ObjectType = Constants.Xds.ObjectTypes.Classification,
                NodeRepresentation = classCode.Code ?? "Unknown",
                Slot = []
            };

            var name = classCode.Display ?? classCode.Code;

            if (!string.IsNullOrWhiteSpace(name))
            {
                classCodeClassification.Name = new InternationalStringType(name);
            }

            if (!string.IsNullOrWhiteSpace(classCode?.System))
            {
                classCodeClassification.AddSlot(new SlotType
                {
                    Name = "codingScheme",
                    ValueList = new ValueListType
                    {
                        Value = [classCode.System.NoUrn()]
                    }
                });
            }

            extrinsicObject.AddClassification(classCodeClassification);
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No class code found",
                Location = ["DocumentReference.category"]
            });

        }

        /* XDSDocumentEntry.typeCode */
        var typeCode = documentReference!.Type?.Coding.FirstOrDefault();
        if (typeCode != null)
        {
            var typeCodeClassification = new ClassificationType
            {
                Id = Guid.NewGuid().ToString(),
                ClassifiedObject = documentReference?.Id?.NoUrn() ?? "Unknown",
                ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.TypeCode,
                ObjectType = Constants.Xds.ObjectTypes.Classification,
                NodeRepresentation = typeCode.Code ?? "Unknown",
                Slot = []
            };

            var name = typeCode.Display ?? typeCode.Code;
            if (!string.IsNullOrWhiteSpace(name))
            {
                typeCodeClassification.Name = new InternationalStringType(name);
            }

            if (!string.IsNullOrWhiteSpace(typeCode.System))
            {
                typeCodeClassification.AddSlot(new SlotType
                {
                    Name = "codingScheme",
                    ValueList = new ValueListType
                    {
                        Value = [typeCode.System.Replace("urn:oid:", "")]
                    }
                });
            }

            extrinsicObject.AddClassification(typeCodeClassification);
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No type code found",
                Location = ["DocumentReference.type.coding"]
            });

        }

        /* XDSDocumentEntry.ConfidentialityCode (1..*) - required */

        if (documentReference!.SecurityLabel.Count != 0 ||
            documentReference!.SecurityLabel.FirstOrDefault()?.Coding.Count != 0)
        {
            foreach (var securityLabelConcept in documentReference.SecurityLabel)
            {
                foreach (var securityLabelConceptCoding in securityLabelConcept.Coding)
                {
                    var securityLabelConceptClassification = new ClassificationType
                    {
                        Id = Guid.NewGuid().ToString(),
                        ClassifiedObject = documentReference.Id ?? "Unknown",
                        ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode,
                        ObjectType = Constants.Xds.ObjectTypes.Classification,
                        NodeRepresentation = securityLabelConceptCoding.Code ?? "Unknown",
                        Slot = []
                    };

                    var name = securityLabelConceptCoding.Display ?? securityLabelConceptCoding.Code;

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        securityLabelConceptClassification.Name = new InternationalStringType(name);
                    }

                    if (!string.IsNullOrWhiteSpace(securityLabelConceptCoding.System))
                    {
                        securityLabelConceptClassification.AddSlot(new SlotType
                        {
                            Name = "codingScheme",
                            ValueList = new ValueListType
                            {
                                Value = [securityLabelConceptCoding.System.NoUrn()]
                            }
                        });
                    }

                    extrinsicObject.AddClassification(securityLabelConceptClassification);
                }
            }
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No ConfidentialityCode found",
                Location = ["DocumentReference.ConfidentialityCode"]
            });

        }

        /* XDSDocumentEntry.uniqueId */
        if (!string.IsNullOrWhiteSpace(documentReference.Id))
        {
            extrinsicObject.AddExternalIdentifier(new ExternalIdentifierType()
            {
                Id = Guid.NewGuid().ToString(),
                ObjectType = Constants.Xds.ObjectTypes.ExternalIdentifier,
                RegistryObject = documentReference.Id,
                IdentificationScheme = Constants.Xds.Uuids.DocumentEntry.UniqueId,
                Name = new InternationalStringType(Constants.Xds.ExternalIdentifierNames.DocumentEntryUniqueId),
                Value = documentReference.Id.Replace("urn:uuid:", ""),
            });
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No unique id found",
                Location = ["DocumentReference"]
            });

        }

        /* XDSDocumentEntry.patientId */
        var patientIdentifierFromDocRef = GetPatient(bundlePatient, documentReference, GpiOid);
        var patientIdentifierFromPix = GetPatient(patientId, GpiOid);
        if (patientIdentifierFromDocRef?.PersonIdentifier != null && patientIdentifierFromPix != null)
        {
            // Add ExternalIdentifier and a new Slot for sourcePatientInfo
            extrinsicObject.AddExternalIdentifier(new ExternalIdentifierType()
            {
                Id = Guid.NewGuid().ToString(),
                ObjectType = Constants.Xds.ObjectTypes.ExternalIdentifier,
                RegistryObject = documentReference.Id ?? "Unknown",
                IdentificationScheme = Constants.Xds.Uuids.DocumentEntry.PatientId,
                Name = new InternationalStringType(Constants.Xds.ExternalIdentifierNames.DocumentEntryPatientId),
                Value = patientIdentifierFromPix.Serialize().Replace("&&", ""),
            });

            var valueList = new ValueListType();

            var patientName = new XPN()
            {
                FamilyName = patientIdentifierFromDocRef.FamilyName,
                GivenName = patientIdentifierFromDocRef.GivenName,
            };

            var patientFromContained = documentReference.Contained.OfType<Patient>().Where(p => p.Identifier.First().Value == patientIdentifierFromDocRef.PersonIdentifier).FirstOrDefault() ?? bundlePatient;

            var patientGender = patientFromContained?.Gender switch
            {
                AdministrativeGender.Female => "F",
                AdministrativeGender.Male => "M",
                AdministrativeGender.Other => "O",
                _ => "U"
            };

            //if (!string.IsNullOrWhiteSpace(patientIdentifier.PersonIdentifier))
            //{
            //    valueList.AddValue($"PID-2|{patientIdentifier.PersonIdentifier}");
            //}
            if (!string.IsNullOrWhiteSpace(patientName.FamilyName) && !string.IsNullOrWhiteSpace(patientName.GivenName))
            {
                valueList.AddValue($"PID-5|{patientName.Serialize().Replace("&&", "")}");
            }


            if (!string.IsNullOrWhiteSpace(patientFromContained?.BirthDate))
            {
                valueList.AddValue($"PID-7|{patientFromContained?.BirthDate.Replace("-", "")}");
            }

            valueList.AddValue($"PID-8|{patientGender}");

            var patientIdSlot = new SlotType()
            {
                Name = "sourcePatientInfo",
                ValueList = valueList
            };
            extrinsicObject.AddSlot(patientIdSlot);

            /*
            <Slot name="sourcePatientInfo">
                <ValueList>
                    <Value>PID-5|Danser^Line^^^^</Value>
                    <Value>PID-7|19691113</Value>
                    <Value>PID-8|F</Value>
                </ValueList>
            </Slot>
            */
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No patientID found",
                Location = ["DocumentReference.Contained"]
            });

        }

        /* XDSDocumentEntry.EventCodes */
        var eventCodeList = documentReference.Context?.Event.ToCodings(); //?

        if (eventCodeList != null)
        {
            foreach (var e in eventCodeList)
            {
                if (string.IsNullOrEmpty(e.Display))
                {
                    e.Display = "Missing display value";
                }

                var classEventCodeList = new ClassificationType
                {
                    Id = Guid.NewGuid().ToString(),
                    ClassifiedObject = documentReference.Id ?? "Unknown",
                    Name = new InternationalStringType(e.Display),
                    ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.EventCodeList,
                    ObjectType = Constants.Xds.ObjectTypes.Classification,
                    NodeRepresentation = e.Code ?? "Unknown",
                    Slot = []
                };

                if (!string.IsNullOrWhiteSpace(e.System))
                {
                    classEventCodeList.AddSlot(new SlotType
                    {
                        Name = "codingScheme",
                        ValueList = new ValueListType
                        {
                            Value = [e.System]
                        }
                    });
                }

                extrinsicObject.AddClassification(classEventCodeList);
            }
        }

        /* XDSDocumentEntry.Comment */
        if (!string.IsNullOrEmpty(documentReference.Description))
        {
            var comment = documentReference.Description.Trim();

            extrinsicObject.Description = new InternationalStringType(comment);
        }

        return new ServiceResultDto<ExtrinsicObjectType>()
        {
            OperationOutcome = operationOutcome,
            Value = extrinsicObject
        };
    }

    private static void AddAuthorPersonSlot(DocumentReference documentReference, ResourceReference practitionerReference, ref List<SlotType> listAuthorSlots, ref OperationOutcome operationOutcome)
    {
        var refAuthorPerson = GetAuthorPerson(documentReference, practitionerReference);
        if (refAuthorPerson == null)
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No author person found.",
                Location = ["DocumentReference.Author.authorPerson"]
            });
        }
        else
        {
            listAuthorSlots.Add(
                new SlotType()
                {
                    Name = "authorPerson",
                    ValueList = new ValueListType()
                    {
                        Value = [refAuthorPerson.Serialize().Replace("&&", "")]
                    }
                });
        }
    }

    private static void AddAuthorInstitutionSlot(DocumentReference documentReference, ResourceReference orgReference,
        ref List<ResourceReference> listOrganization,
        ref List<ResourceReference> listProcessedOrganization,
        ref List<SlotType> listAuthorSlots,
        ref OperationOutcome operationOutcome)
    {

        // Department
        var refAuthorDept = GetAuthorDepartment(documentReference, listOrganization, orgReference, out var deptOrgReference);

        if (deptOrgReference != null)
        {
            listProcessedOrganization.Add(deptOrgReference);
        }
        var refAuthorOrg = GetAuthorOrganization(documentReference, orgReference);

        if (refAuthorOrg == null && refAuthorDept == null)
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No author organizations found",
                Location = ["DocumentReference.Author.Organization"]
            });
        }

        if (refAuthorDept == null)
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Information,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No author departments found.",
                Location = ["DocumentReference.Author.Organization.Department"]
            });
        }

        // Create slot for organization
        if (refAuthorOrg != null)
        {
            var refAuthorOrgNameOnly = new XON()
            {
                OrganizationName = refAuthorOrg!.OrganizationName
            };

            var authorInstitutionSlot = new SlotType()
            {
                Name = "authorInstitution",
                ValueList = new ValueListType()
                {
                    Value =
                    [
                        refAuthorOrgNameOnly.Serialize(),
                        refAuthorOrg.Serialize(),
                    ]
                }
            };

            var refAuthorDeptString = refAuthorDept?.Serialize();

            if (!string.IsNullOrWhiteSpace(refAuthorDeptString))
            {
                authorInstitutionSlot.AddValue(refAuthorDeptString);
            }

            listAuthorSlots.Add(authorInstitutionSlot);
        }
    }

    private static void AddAuthorSpecialtySlot(
        List<string>? authorSpecialty,
        ref List<SlotType> listAuthorSlots,
        ref OperationOutcome operationOutcome)
    {
        if (authorSpecialty == null)
        {
            operationOutcome!.AddIssue(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Warning,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No author specialty found.",
                Location = ["DocumentReference.Author.authorSpecialty"]
            });
        }
        else
        {
            listAuthorSlots.Add(
            new SlotType()
            {
                Name = "authorSpecialty",
                ValueList = new ValueListType()
                {
                    Value = [.. authorSpecialty]
                }
            });
        }
    }

    private static void AddAuthorRoleSlot(List<string>? authorRole, ref List<SlotType> listAuthorSlots, ref OperationOutcome operationOutcome)
    {
        if (authorRole == null)
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Warning,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No author role found.",
                Location = ["DocumentReference.Author.PractitionerRole"]
            });
        }
        else
        {
            listAuthorSlots.Add(
            new SlotType()
            {
                Name = "authorRole",
                ValueList = new ValueListType()
                {
                    Value = [.. authorRole]
                }
            });
        }
    }

    private static string GetAuthorReferenceTarget(DocumentReference documentReference, ResourceReference authorReference)
    {
        var containedRef = documentReference.Contained.Where(x => x.Id == authorReference.Reference?.Trim('#')).First();

        return containedRef.TypeName.ToString();
    }

    private static ServiceResultDto<AssociationType> CreateAssociationForSubmissionSet(ExtrinsicObjectType? extrinsicObject, RegistryPackageType? registryPackage)
    {
        var operationOutcome = new OperationOutcome();
        var association = new AssociationType()
        {
            Id = Guid.NewGuid().ToString(),
            ObjectType = Constants.Xds.ObjectTypes.Association,
            AssociationTypeData = Constants.Xds.AssociationType.HasMember,
            Slot =
            [
                new SlotType()
            {
                Name = Constants.Xds.SlotNames.SubmissionSetStatus,
                ValueList = new()
                {
                    Value = ["Original"]  // <== hardcoded, TBD
                }
            }
            ]
        };

        if (extrinsicObject?.Id != null && registryPackage?.Id != null)
        {
            // Association defines a link between DocumentEntry and SubmissionSet
            association.SourceObject = registryPackage.Id;
            association.TargetObject = extrinsicObject.Id;
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No ID for RegistryPackage or ExtrinsicObject found. Unable to create association.",
                Location = ["Association.Id"]
            });
        }

        return new ServiceResultDto<AssociationType>()
        {
            OperationOutcome = operationOutcome,
            Value = association
        };
    }

    private static ServiceResultDto<DocumentType> ConvertBinaryToDocument(Binary? fhirBinary, ExtrinsicObjectType? extrinsicObject)
    {
        var operationOutcome = new OperationOutcome();
        var document = new DocumentType();

        if (fhirBinary?.Data != null && extrinsicObject?.Id != null)
        {
            document.Value = fhirBinary.Data;
            document.Id = extrinsicObject.Id;
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No Document Data provided.",
                Location = ["Document.Data"]
            });
        }

        return new ServiceResultDto<DocumentType>()
        {
            Value = document,
            OperationOutcome = operationOutcome
        };
    }

    internal static XON? GetAuthorDepartment(DocumentReference documentReference, List<ResourceReference> listOrganization, ResourceReference parentOrgReference, out ResourceReference deptOrgReference)
    {
        deptOrgReference = null!;

        foreach (var orgRef in listOrganization)
        {

            var authorDept = documentReference!.Contained!
                .OfType<Organization>()
                .FirstOrDefault(dpt => dpt?.PartOf?.Reference == parentOrgReference.Reference);


            if (authorDept != null && authorDept.Name != null)
            {
                deptOrgReference = orgRef;
                var authorDepartment = new XON()
                {
                    OrganizationName = authorDept.Name,
                    AssigningAuthority = new HD()
                    {
                        UniversalId = $"{Constants.Oid.ReshId}",
                        UniversalIdType = "ISO"
                    },
                    OrganizationIdentifier = authorDept?.Identifier?.First()?.Value ?? string.Empty
                };
                return authorDepartment;
            }

        }

        return null;
    }

    internal static XON? GetAuthorDepartment(List submissionSet)
    {
        var authorDept = submissionSet!.Contained!
            .OfType<Organization>()
            .FirstOrDefault(dpt => dpt?.PartOf != null);

        if (authorDept == null || authorDept.Name == null)
        {
            return null;
        }

        var authorDepartment = new XON()
        {
            OrganizationName = authorDept.Name,
            AssigningAuthority = new HD()
            {
                UniversalId = $"{Constants.Oid.ReshId}",
                UniversalIdType = "ISO"
            },
            OrganizationIdentifier = authorDept?.Identifier?.First()?.Value ?? string.Empty
        };
        return authorDepartment;
    }

    internal static XON? GetAuthorOrganization(DocumentReference documentReference, ResourceReference authorReference)
    {
        // Opposite to getting department (organization that does not have a "partOf" field)
        var authorOrg = documentReference.Contained.OfType<Organization>().Where(x => x.Id == authorReference.Reference?.Trim('#')).FirstOrDefault(dpt => dpt.PartOf == null);


        if (authorOrg == null)
        {
            // Fallback. If authorRole points to department, try to check if this author reference has any departemnt and get that department
            var possibleDepartment = documentReference.Contained.OfType<Organization>().Where(x => x.Id == authorReference.Reference?.Trim('#')).FirstOrDefault(dpt => dpt.PartOf != null);
            authorOrg = documentReference.Contained.OfType<Organization>().Where(org => org.Id == possibleDepartment?.PartOf?.Reference?.Trim('#')).FirstOrDefault();


            if (authorOrg == null)
            {
                return null;

            }
        }

        var authorOrganization = new XON()
        {
            OrganizationName = authorOrg?.Name ?? "Unknown",
        };

        // Adds org.identifier if known
        if (authorOrg?.Identifier.Count != 0)
        {
            authorOrganization.AssigningAuthority = new HD()
            {
                UniversalId = $"{Constants.Oid.Brreg}",
                UniversalIdType = "ISO"
            };
            authorOrganization.OrganizationIdentifier = authorOrg?.Identifier?.First()?.Value ?? string.Empty;
        }
        return authorOrganization;
    }

    internal static XON? GetAuthorOrganization(List submissionSet)
    {
        // Opposite to getting department (organization that does not have a "partOf" field)
        var authorOrg = submissionSet.Contained.OfType<Organization>().FirstOrDefault(dpt => dpt.PartOf == null);

        if (authorOrg == null)
        {
            return null;
        }

        var authorOrganization = new XON()
        {
            OrganizationName = authorOrg?.Name ?? "Unknown",
        };

        // Adds org.identifier if known
        if (authorOrg?.Identifier.Count != 0)
        {
            authorOrganization.AssigningAuthority = new HD()
            {
                UniversalId = $"{Constants.Oid.Brreg}",
                UniversalIdType = "ISO"
            };
            authorOrganization.OrganizationIdentifier = authorOrg?.Identifier?.First()?.Value ?? string.Empty;
        }
        ;
        return authorOrganization;
    }

    private static XCN? GetAuthorPerson(DocumentReference documentReference, ResourceReference auhtorReference)
    {
        var authorDocRef = documentReference.Contained.OfType<Practitioner>().Where(x => x.Id == auhtorReference.Reference?.Trim('#')).FirstOrDefault();

        if (authorDocRef == null)
        {
            return null;
        }
        var authorName = authorDocRef.Name.First();

        var author = new XCN()
        {
            GivenName = (authorName.Given.Count() == 1 ? authorName.Given.First() : string.Join(" ", authorName.Given)) ?? "Unknown",
            FamilyName = authorName.Family ?? "Unknown",

        };

        if (authorDocRef?.Identifier.Count != 0)
        {
            foreach (var identity in authorDocRef!.Identifier)
            {
                author.PersonIdentifier = identity.Value ?? "Unknown";
                author.AssigningAuthority = new HD()
                {
                    NamespaceId = $"&{identity.System}&",
                    UniversalIdType = "ISO"
                };
            }
        }

        return author;
    }

    private static XCN? GetAuthorPerson(List submissionSet)
    {
        var authorDocRef = submissionSet.Contained.OfType<Practitioner>().FirstOrDefault();

        if (authorDocRef == null)
        {
            return null;
        }
        var authorName = authorDocRef.Name.First();

        var author = new XCN()
        {
            GivenName = (authorName.Given.Count() == 1 ? authorName.Given.First() : string.Join(" ", authorName.Given)) ?? "Unknown",
            FamilyName = authorName.Family ?? "Unknown",

        };

        if (authorDocRef?.Identifier.Count != 0)
        {
            foreach (var identity in authorDocRef!.Identifier)
            {
                author.PersonIdentifier = identity.Value ?? "Unknown";
                author.AssigningAuthority = new HD()
                {
                    NamespaceId = $"&{identity.System}&",
                    UniversalIdType = "ISO"
                };
            }
        }

        return author;
    }

    private static void GetAuthorRefsAndRoleAndSpecialty(
        DocumentReference documentReference,
        ResourceReference roleReference,
        ResourceReference practitionerReference,
        out ResourceReference organizationReference,
        out List<string>? authorRole,
        out List<string>? authorSpecialty)
    {
        organizationReference = null!;
        authorRole = null!;
        authorSpecialty = null!;

        // Find PractitionerRole based on reference and belonging to correct practitioner by reference at the same time
        var authorDocRef = documentReference.Contained.OfType<PractitionerRole>()
            .Where(x => (x.Id == roleReference.Reference?.Trim('#')) && (x.Practitioner?.Reference == practitionerReference.Reference))
            .FirstOrDefault();

        if (authorDocRef != null)
        {
            if (authorDocRef.Practitioner?.Url == practitionerReference!.Url)
            {
                // List of roles if declared
                if (authorDocRef.Code.Count > 0)
                {
                    authorRole = authorDocRef.Code.SelectMany(role => role.Coding.Select(code => code.Display)).OfType<string>().ToList();
                }

                // List of specialties if declared
                if (authorDocRef.Specialty.Count > 0)
                {
                    authorSpecialty = authorDocRef.Specialty.SelectMany(specialty => specialty.Coding.Select(coding => coding.Display)).OfType<string>().ToList();
                }

                // Reference to organization
                if (authorDocRef.Organization != null)
                {
                    organizationReference = authorDocRef.Organization;
                }
            }
        }
    }

    private static void GetAuthorRefsAndRoleAndSpecialty(
        DocumentReference documentReference,
        ResourceReference roleReference,
        out ResourceReference organizationReference,
        out List<string>? authorRole,
        out List<string>? authorSpecialty)
    {
        organizationReference = null!;
        authorRole = null!;
        authorSpecialty = null!;

        var authorDocRef = documentReference.Contained.OfType<PractitionerRole>().Where(x => x.Id == roleReference.Reference?.Trim('#')).FirstOrDefault();

        if (authorDocRef != null)
        {
            // List of roles if declared
            if (authorDocRef.Code.Count > 0)
            {
                authorRole = authorDocRef.Code.SelectMany(role => role.Coding.Select(code => code.Display)).OfType<string>().ToList();
            }

            // List of specialties if declared
            if (authorDocRef.Specialty.Count > 0)
            {
                authorSpecialty = authorDocRef.Specialty.SelectMany(specialty => specialty.Coding.Select(coding => coding.Display)).OfType<string>().ToList();
            }

            // Reference to organization
            if (authorDocRef.Organization != null)
            {
                organizationReference = authorDocRef.Organization;
            }
        }
    }


    internal static XCN? GetPatient(Patient bundlePatient, DocumentReference documentReference, string GpiOid)
    {
        var patientDocRef = documentReference.Contained.OfType<Patient>().FirstOrDefault() ?? bundlePatient;

        if (patientDocRef == null)
        {
            return null;
        }
        var patientName = patientDocRef.Name.First();

        var patient = new XCN()
        {
            GivenName = (patientName.Given.Count() == 1 ? patientName.Given.First() : string.Join(" ", patientName.Given)) ?? "Unknown",
            FamilyName = patientName.Family ?? "Unknown",
            PersonIdentifier = patientDocRef?.Identifier?.FirstOrDefault()?.Value ?? "Unknown",
            AssigningAuthority = new HD()
            {
                NamespaceId = $"&{GpiOid}&",
                UniversalIdType = "ISO"
            }
        };

        return patient;
    }

    internal static XCN? GetPatient(Identifier identifier, string assigningAuthorityId)
    {
        var patientId = identifier.Value;

        if (patientId == null)
        {
            return null;
        }

        var patient = new XCN()
        {
            PersonIdentifier = patientId,
            AssigningAuthority = new HD()
            {
                NamespaceId = $"&{assigningAuthorityId}&",
                UniversalIdType = "ISO"
            }
        };

        return patient;
    }

    public static string GenerateRandomOid()
    {
        // Generate a random suffix for uniqueness
        var random = new Random();
        var randomSuffix = $"{random.Next(1, 99999)}.{random.Next(1, 99999)}.{random.Next(1, 99999)}";

        // Combine base and suffix
        var randomOid = $"{Constants.Oid.Nhn}.7.4.{randomSuffix}";
        return randomOid;
    }

    private static List<AssociationType>? MapRelationsToXdsAssociations(DocumentReference dr, string? sourceExtrinsicId)
    {
        if (sourceExtrinsicId == null) return null;

        var result = new List<AssociationType>();

        if (dr.RelatesTo == null || dr.RelatesTo.Count == 0)
            return result;

        foreach (var rel in dr.RelatesTo)
        {
            if (rel.Code == null || string.IsNullOrWhiteSpace(rel.Target?.Reference))
                continue;

            var targetRef = rel.Target.Reference!;
            var targetExtrinsicId = targetRef
                .Replace("DocumentReference/", "", StringComparison.OrdinalIgnoreCase)
                .Replace("urn:uuid:", "");

            var associationType = rel.Code.Value switch
            {
                DocumentRelationshipType.Replaces =>
                    Xds.AssociationType.Replace,

                DocumentRelationshipType.Transforms =>
                    Xds.AssociationType.Transformation,

                DocumentRelationshipType.Appends =>
                    Xds.AssociationType.Addendum,

                DocumentRelationshipType.Signs =>
                    Xds.AssociationType.DigitalSignature,

                //DocumentRelationshipType.IsSnapshotOf =>
                //	Xds.Constants.Xds.AssociationType.SnapshotOfOnDemandDocumentEntry,

                _ => throw new NotSupportedException(
                    $"Unsupported DocumentRelationshipType '{rel.Code}'")
            };

            result.Add(new AssociationType
            {
                Id = Guid.NewGuid().ToString(),
                ObjectType = Xds.ObjectTypes.Association,
                AssociationTypeData = associationType,
                SourceObject = sourceExtrinsicId,
                TargetObject = targetExtrinsicId
            });
        }

        return result;
    }

    private static Binary? MatchBinaryToDocumentReference(
    Bundle bundle,
    DocumentReference documentReference,
    int indexFallback,
    List<Binary> binaries,
    OperationOutcome operationOutcome)
    {
        // 1) Try attachment.url → Bundle.Entry.fullUrl
        var attachmentUrl = documentReference.Content
            .FirstOrDefault()?.Attachment?.Url;

        if (!string.IsNullOrWhiteSpace(attachmentUrl))
        {
            // Normalize reference (strip resource prefix if present)
            var normalizedRef = attachmentUrl.StartsWith("Binary/", StringComparison.OrdinalIgnoreCase)
                ? attachmentUrl
                : attachmentUrl;

            var matchedEntry = bundle.Entry.FirstOrDefault(e =>
                string.Equals(e.FullUrl, normalizedRef, StringComparison.OrdinalIgnoreCase));

            if (matchedEntry?.Resource is Binary binaryByFullUrl)
            {
                return binaryByFullUrl;
            }

            // 2) attachment.url → Binary.id
            var idCandidate = attachmentUrl
                .Replace("Binary/", "", StringComparison.OrdinalIgnoreCase)
                .Replace("urn:uuid:", "", StringComparison.OrdinalIgnoreCase);

            var binaryById = binaries.FirstOrDefault(b =>
                string.Equals(b.Id, idCandidate, StringComparison.OrdinalIgnoreCase));

            if (binaryById != null)
            {
                return binaryById;
            }
        }

        // 3) Fallback to index-based matching
        var fallbackBinary = binaries.ElementAtOrDefault(indexFallback);

        if (fallbackBinary != null)
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Warning,
                Code = OperationOutcome.IssueType.Informational,
                Diagnostics =
                    $"Binary matched to DocumentReference '{documentReference.Id}' using index fallback. " +
                    $"Consider using DocumentReference.content.attachment.url referencing Bundle.entry.fullUrl.",
                Location = new[]
                {
                "DocumentReference.content.attachment.url"
            }
            });
        }

        return fallbackBinary;
    }

    private static OperationOutcome ValidateDocumentRelations(DocumentReference dr)
    {
        var oo = new OperationOutcome();

        if (dr.RelatesTo == null || dr.RelatesTo.Count == 0)
            return oo;

        void Error(string diagnostics, params string[] location)
        {
            oo.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.Invalid,
                Diagnostics = diagnostics,
                Location = location?.Length > 0 ? location : new[] { "DocumentReference.relatesTo" }
            });
        }

        // --- Helpers ---
        static string StripUrnUuid(string? v) =>
            string.IsNullOrWhiteSpace(v)
                ? ""
                : v.Replace("urn:uuid:", "", StringComparison.OrdinalIgnoreCase);

        static string StripDocRefPrefix(string? v) =>
            string.IsNullOrWhiteSpace(v)
                ? ""
                : v.Replace("DocumentReference/", "", StringComparison.OrdinalIgnoreCase);

        static bool TryParseTargetEntryUuid(string? reference, out string entryUuid)
        {
            entryUuid = "";

            if (string.IsNullOrWhiteSpace(reference))
                return false;

            // Accept:
            // - "DocumentReference/<uuid>"
            // - "urn:uuid:<uuid>"
            // - "<uuid>"
            var candidate = StripUrnUuid(StripDocRefPrefix(reference)).Trim();

            // Guard against contained refs like "#something"
            if (candidate.StartsWith("#", StringComparison.Ordinal))
                return false;

            // Must be a GUID (entryUUID)
            if (!Guid.TryParse(candidate, out _))
                return false;

            entryUuid = candidate;
            return true;
        }

        // --- 1) Basic per-item validation: code + target required ---
        for (var i = 0; i < dr.RelatesTo.Count; i++)
        {
            var rel = dr.RelatesTo[i];

            if (rel.Code == null)
            {
                Error("relatesTo.code is required.",
                    $"DocumentReference.relatesTo[{i}].code");
                continue;
            }

            if (rel.Target == null || string.IsNullOrWhiteSpace(rel.Target.Reference))
            {
                Error("relatesTo.target.reference is required.",
                    $"DocumentReference.relatesTo[{i}].target.reference");
                continue;
            }

            if (!TryParseTargetEntryUuid(rel.Target.Reference, out _))
            {
                Error("relatesTo.target.reference must be an entryUUID (GUID) in the form " +
                      "'DocumentReference/<uuid>' or 'urn:uuid:<uuid>' or '<uuid>'.",
                    $"DocumentReference.relatesTo[{i}].target.reference");
            }
        }

        // If we already have target/code errors, stop early (avoid misleading combo errors)
        if (oo.Issue.Any(i => i.Severity == OperationOutcome.IssueSeverity.Error))
            return oo;

        // --- 2) Prevent "document relates to itself" ---
        var selfId = StripUrnUuid(dr.Id);

        if (!string.IsNullOrWhiteSpace(selfId) && Guid.TryParse(selfId, out _))
        {
            for (var i = 0; i < dr.RelatesTo.Count; i++)
            {
                var rel = dr.RelatesTo[i];
                if (!TryParseTargetEntryUuid(rel.Target?.Reference, out var targetId))
                    continue;

                if (string.Equals(selfId, targetId, StringComparison.OrdinalIgnoreCase))
                {
                    Error("A document cannot relate to itself.",
                        $"DocumentReference.relatesTo[{i}].target.reference");
                    break;
                }
            }
        }

        // --- 3) Cardinality: only one of each relationship type ---
        var codes = dr.RelatesTo
            .Where(r => r.Code != null)
            .Select(r => r.Code!.Value)
            .ToList();

        foreach (var g in codes.GroupBy(c => c))
        {
            if (g.Count() > 1)
                Error($"Multiple '{g.Key}' relationships are not allowed.");
        }

        // --- 4) Semantic combination rules ---
        var hasReplace = codes.Contains(DocumentRelationshipType.Replaces);
        var hasAppend = codes.Contains(DocumentRelationshipType.Appends);
        var hasTransform = codes.Contains(DocumentRelationshipType.Transforms);
        var hasSign = codes.Contains(DocumentRelationshipType.Signs);
        _ = hasSign; // kept for readability / future rules

        // Invalid semantic combinations
        if (hasReplace && hasAppend)
            Error("A document cannot both replace and append to another document.");

        if (hasAppend && hasTransform)
            Error("An addendum (appends) cannot also be a transform.");

        // Replace must have exactly one target (already 1-of-each, but keep explicit rule)
        if (hasReplace && dr.RelatesTo.Count(r => r.Code == DocumentRelationshipType.Replaces) != 1)
            Error("A document that replaces another must reference exactly one target.");

        // --- 5) Target consistency rules ---
        // All non-sign relationships must target the same document (signing may be multi-sign)
        var nonSignTargets = dr.RelatesTo
            .Where(r => r.Code != DocumentRelationshipType.Signs)
            .Select(r => r.Target?.Reference)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => StripUrnUuid(StripDocRefPrefix(r)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (nonSignTargets.Count > 1)
            Error("All non-sign relationships must reference the same target document.");

        return oo;
    }
}

