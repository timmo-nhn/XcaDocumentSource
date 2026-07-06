using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using System.Globalization;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared;
using XcaXds.Shared.Extensions;
using XcaXds.Terminology;
using XcaXds.Terminology.Services;

namespace XcaXds.Commons.DataManipulators.Fhir;

/// <summary>
/// Transforms between FHIR resources (specifically DocumentReference and related resources) and 
/// XDS registry objects (ExtrinsicObject, RegistryPackage, Association) 
/// For now it mainly supports only one-directional transformation (FHIR -> XDS)
/// </summary>
public class FhirToXdsTransformerService
{
    private readonly ILogger<FhirToXdsTransformerService> _logger;
    private readonly TerminologyService _terminologyService;
    private readonly ApplicationConfig _applicationConfig;

    private static string? Organization;
    private static string? Department;

    public FhirToXdsTransformerService(
        ILogger<FhirToXdsTransformerService> logger,
        TerminologyService terminologyService,
        ApplicationConfig applicationConfig)
    {
        _logger = logger;
        _terminologyService = terminologyService;
        _applicationConfig = applicationConfig;

        var organizationSystems = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Other.OrganizationAssigningAuthorities);

        Organization = organizationSystems.GetFirstValueByName("Organization");
        Department = organizationSystems.GetFirstValueByName("Department");
    }

    public ServiceResultDto<ProvideAndRegisterDocumentSetRequestType> CreateSoapObjectFromComprehensiveBundle(Bundle bundle, Patient? bundlePatient, List<DocumentReference>? documentReferences, List? submissionSetList, List<Binary>? fhirBinaries, string? homeCommunityId)
    {
        var operationOutcome = new OperationOutcome();

        var registryPackageResult = ConvertSubmissionSetListAndDocumentReferenceToRegistryPackage(bundlePatient, submissionSetList, homeCommunityId);
        if (!registryPackageResult.Success)
        {
            operationOutcome.AddIssue(registryPackageResult.OperationOutcome?.Issue);
        }

        var registryPackage = registryPackageResult.Value;
        var extrinsicObjects = new List<ExtrinsicObjectType>();
        var documents = new List<DocumentType>();
        var associations = new List<AssociationType>();

        for (var i = 0; i < documentReferences?.Count; i++)
        {
            var documentReference = documentReferences[i];

            var fhirBinary = MatchBinaryToDocumentReference(
                bundle: bundle,
                documentReference: documentReference,
                indexFallback: i,
                binaries: fhirBinaries,
                operationOutcome: operationOutcome);

            var extrinsicResult = ConvertDocumentReferenceToExtrinsicObject(bundlePatient, documentReference, fhirBinary);
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
                var documentResult = ConvertBinaryToDocument(fhirBinary);
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
            if (validationOutcome.Issue.Any(issue => issue.Severity == OperationOutcome.IssueSeverity.Error))
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

            // Map relatesTo → XDS associations (0 ... N)
            var relationAssociations = MapRelationsToXdsAssociations(documentReference, sourceExtrinsicId);

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

        var request = new ProvideAndRegisterDocumentSetRequestType()
        {
            SubmitObjectsRequest = new SubmitObjectsRequest()
            {
                RegistryObjectList = combinedRegistryObjects.ToArray()
            },
            // Document list can be empty for metadata-only submissions
            Document = [.. documents]
        };

        // We're now sending both:
        // HasMember associations(SubmissionSet -> DocumentEntry)
        // Relation associations(New DocumentEntry -> Old DocumentEntry), i.e.RPLC / APND / XFRM / SIGN

        var creationResult = new ServiceResultDto<ProvideAndRegisterDocumentSetRequestType>()
        {
            Value = request,
            OperationOutcome = operationOutcome
        };

        return creationResult;
    }

    private ServiceResultDto<RegistryPackageType> ConvertSubmissionSetListAndDocumentReferenceToRegistryPackage(Patient? bundlePatient, List? submissionSetList, string? homeCommunityId)
    {
        var operationOutcome = new OperationOutcome();


        if (submissionSetList == null)
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Fatal,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "Missing SubmissionSet List"
            });

            return new ServiceResultDto<RegistryPackageType>()
            {
                OperationOutcome = operationOutcome
            };
        }

        var registryPackage = new RegistryPackageType()
        {
            Id = submissionSetList.Id,
            Name = string.IsNullOrWhiteSpace(submissionSetList.Title) ? null : new InternationalStringType($"{submissionSetList.Title}"),
            ObjectType = Constants.Xds.ObjectTypes.RegistryPackage,
        };

        HandleSubmissionSetClassification(submissionSetList, operationOutcome, registryPackage);
        HandleSubmissionSetComment(submissionSetList, registryPackage);
        HandleSubmissionSetSubmissionTime(submissionSetList, operationOutcome, registryPackage);
        HandleSubmissionSetAuthor(submissionSetList, operationOutcome, registryPackage);
        HandleSubmissionSetContentTypeCode(submissionSetList, operationOutcome, registryPackage);
        HandleSubmissionSetUniqueId(submissionSetList, operationOutcome, registryPackage);
        HandleSubmissionSetSourceId(submissionSetList, operationOutcome, registryPackage);
        HandleSubmissionSetPatientId(bundlePatient, submissionSetList, homeCommunityId, operationOutcome, registryPackage);

        return new ServiceResultDto<RegistryPackageType>()
        {
            OperationOutcome = operationOutcome,
            Value = registryPackage
        };
    }

    private static void HandleSubmissionSetClassification(List submissionSetList, OperationOutcome operationOutcome, RegistryPackageType registryPackage)
    {
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
    }

    private static void HandleSubmissionSetComment(List submissionSetList, RegistryPackageType registryPackage)
    {
        var comment = submissionSetList.Note.Select(note => note.Text).OfType<string>().Where(text => !string.IsNullOrWhiteSpace(text)).ToArray();

        // Comment from submission
        if (comment?.Length > 0)
        {
            registryPackage.AddSlot(new SlotType(Constants.Xds.SlotNames.Comments, comment));
        }
    }

    private static void HandleSubmissionSetSubmissionTime(List submissionSetList, OperationOutcome operationOutcome, RegistryPackageType registryPackage)
    {
        //SubmissionTime
        if (submissionSetList.Date != null && DateTime.TryParse(submissionSetList.Date, out var submissionTime))
        {
            registryPackage.AddSlot(new SlotType(Constants.Xds.SlotNames.SubmissionTime, submissionTime.ToUniversalTime().ToString(Constants.Hl7.Dtm.DtmFormat)));
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
    }

    private static void HandleSubmissionSetPatientId(Patient? bundlePatient, List submissionSetList, string? homeCommunityId, OperationOutcome operationOutcome, RegistryPackageType registryPackage)
    {
        // XDSSubmissionSet.patientId
        //var patientIdFromPix = GetPatient(patientId, GpiOid);
        //var patientIdFromPix = GetPatient(patientId, sourceId);
        var patientId = bundlePatient?.Identifier.FirstOrDefault();
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
    }

    private static void HandleSubmissionSetSourceId(List submissionSetList, OperationOutcome operationOutcome, RegistryPackageType registryPackage)
    {
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
            var value = $"{Organization}.{documentOrganization?.Identifier?.FirstOrDefault()?.Value}";

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
    }

    private void HandleSubmissionSetUniqueId(List submissionSetList, OperationOutcome operationOutcome, RegistryPackageType registryPackage)
    {
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
    }

    private static void HandleSubmissionSetContentTypeCode(List submissionSetList, OperationOutcome operationOutcome, RegistryPackageType registryPackage)
    {
        // XdsSubmissionset.ContentTypeCode (Document type)
        var submissionConfCode = submissionSetList.GetExtension("https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-designationType");
        if (submissionConfCode.Value is CodeableConcept valueCodeableConcept)
        {
            var submissionConcept = valueCodeableConcept.Coding.FirstOrDefault();

            var submissionConfClassification = new ClassificationType()
            {
                Id = Guid.NewGuid().ToString(),
                ClassifiedObject = submissionSetList.Id ?? "Unknown",
                ObjectType = Constants.Xds.ObjectTypes.Classification,
                ClassificationScheme = Constants.Xds.Uuids.SubmissionSet.ContentTypeCode,
                NodeRepresentation = submissionConcept?.Code ?? "Unknown",
                Slot = []
            };

            if (!string.IsNullOrWhiteSpace(submissionConcept?.System))
            {
                submissionConfClassification.AddSlot(new SlotType(Constants.Xds.SlotNames.CodingScheme, submissionConcept.System.NoUrn()));
            }

            if (!string.IsNullOrWhiteSpace(submissionConcept?.Display))
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
    }

    private void HandleSubmissionSetAuthor(List submissionSetList, OperationOutcome operationOutcome, RegistryPackageType registryPackage)
    {
        // XDS SubmissionSet - Legal entity
        var submAuthorOrg = GetAuthorOrganization(submissionSetList);

        if (submAuthorOrg == null)
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Warning,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No author organizations for submission was found",
                Location = ["List.identifier"]
            });
        }

        // XDS SubmissionSet.Author - Department
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

            var submAuthorOrgNameOnly = new XON()
            {
                OrganizationName = submAuthorOrg.OrganizationName
            };

            var submAuthorOrgNameOnlyString = submAuthorOrgNameOnly.Serialize();
            var submAuthorOrgString = submAuthorOrg.Serialize();

            var authorDepartmentSlot = new SlotType(Constants.Xds.SlotNames.AuthorInstitution, submAuthorOrgNameOnlyString, submAuthorOrgString);

            var submAuthorDepartmentString = submAuthorDept?.Serialize();

            if (!string.IsNullOrWhiteSpace(submAuthorDepartmentString))
            {
                authorDepartmentSlot.AddValue(submAuthorDepartmentString);
            }

            authorClassification.AddSlot(authorDepartmentSlot);

            var submAuthorPersonString = submAuthorPerson.Serialize()?.Replace("&&", "");
            if (submAuthorPersonString != null)
            {
                var authorPersonSlot = new SlotType(Constants.Xds.SlotNames.AuthorPerson, submAuthorPersonString);
                authorClassification.AddSlot(authorPersonSlot);
            }

            registryPackage.AddClassification(authorClassification);
        }
    }

    public ServiceResultDto<ExtrinsicObjectType> ConvertDocumentReferenceToExtrinsicObject(Patient? bundlePatient, DocumentReference documentReference, Binary? fhirBinary)
    {
        var operationOutcome = new OperationOutcome();
        var statusType = GetDocumentReferenceStatus(documentReference);

        var attachment = documentReference.Content.FirstOrDefault()?.Attachment;
        var patientId = bundlePatient?.Identifier.FirstOrDefault();
        var gpiOid = patientId?.System?.NoUrn();

        var extrinsicObject = new ExtrinsicObjectType()
        {
            MimeType = attachment?.ContentType ?? "Unknown",
            Id = documentReference.Id?.NoUrn(),
            Status = statusType.ToString(),
            Name = new InternationalStringType(attachment?.Title ?? "Unknown"),
            ObjectType = Constants.Xds.Uuids.DocumentEntry.StableDocumentEntries,
        };

        HandleDocumentEntryCreationTime(attachment, extrinsicObject);
        HandleDocumentEntryLanguageCode(attachment, extrinsicObject);
        HandleDocumentEntrySourcePatientId(patientId, gpiOid, extrinsicObject);
        HandleDocumentEntryServiceStartTime(documentReference, operationOutcome, extrinsicObject);
        HandleDocumentEntryServiceStopTime(documentReference, operationOutcome, extrinsicObject);
        HandleDocumentEntryAuthors(documentReference, operationOutcome, extrinsicObject);
        HandleDocumentEntryFormatCode(documentReference, operationOutcome, extrinsicObject);
        HandleDocumentEntryHealthcareFacilityTypeCode(documentReference, operationOutcome, extrinsicObject);
        HandleDocumentEntryPracticeSettingCode(documentReference, operationOutcome, extrinsicObject);
        HandleDocumentEntryClassCode(documentReference, operationOutcome, extrinsicObject);
        HandleDocumentEntryTypeCode(documentReference, operationOutcome, extrinsicObject);
        HandleDocumentEntryConfidentialityCode(documentReference, operationOutcome, extrinsicObject);
        HandleDocumentEntryUniqueId(documentReference, fhirBinary, operationOutcome, extrinsicObject);
        HandleDocumentEntryPatientId(bundlePatient, documentReference, operationOutcome, patientId, gpiOid, extrinsicObject);
        HandleDocumentEntryEventCode(documentReference, extrinsicObject);
        HandleDocumentEntryComment(documentReference, extrinsicObject);

        return new ServiceResultDto<ExtrinsicObjectType>()
        {
            OperationOutcome = operationOutcome,
            Value = extrinsicObject
        };
    }

    private static void HandleDocumentEntryComment(DocumentReference documentReference, ExtrinsicObjectType extrinsicObject)
    {
        /* XDSDocumentEntry.Comment */
        if (!string.IsNullOrEmpty(documentReference.Description))
        {
            var comment = documentReference.Description.Trim();

            extrinsicObject.Description = new InternationalStringType(comment);
        }
    }

    private static void HandleDocumentEntryEventCode(DocumentReference documentReference, ExtrinsicObjectType extrinsicObject)
    {
        /* XDSDocumentEntry.EventCodes */
        var eventCodeList = documentReference.Context?.Event.ToCodings();

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
    }

    private static void HandleDocumentEntryPatientId(Patient? bundlePatient, DocumentReference documentReference, OperationOutcome operationOutcome, Identifier? patientId, string? gpiOid, ExtrinsicObjectType extrinsicObject)
    {
        /* XDSDocumentEntry.patientId */
        var patientIdentifierFromDocRef = GetPatient(bundlePatient, documentReference, gpiOid);
        var patientIdentifierFromPix = GetPatient(patientId, gpiOid);
        var pidFromPixString = patientIdentifierFromPix?.Serialize()?.Replace("&&", "");

        if (patientIdentifierFromDocRef?.PersonIdentifier != null && !string.IsNullOrWhiteSpace(pidFromPixString))
        {
            // Add ExternalIdentifier and a new Slot for sourcePatientInfo
            extrinsicObject.AddExternalIdentifier(new ExternalIdentifierType()
            {
                Id = Guid.NewGuid().ToString(),
                ObjectType = Constants.Xds.ObjectTypes.ExternalIdentifier,
                RegistryObject = documentReference.Id ?? "Unknown",
                IdentificationScheme = Constants.Xds.Uuids.DocumentEntry.PatientId,
                Name = new InternationalStringType(Constants.Xds.ExternalIdentifierNames.DocumentEntryPatientId),
                Value = pidFromPixString,
            });

            var valueList = new ValueListType();

            var patientName = new XPN()
            {
                FamilyName = patientIdentifierFromDocRef.FamilyName,
                GivenName = patientIdentifierFromDocRef.GivenName,
            };

            var patientFromContained = documentReference.Contained.OfType<Patient>().Where(p => p.Identifier.FirstOrDefault()?.Value == patientIdentifierFromDocRef.PersonIdentifier).FirstOrDefault() ?? bundlePatient;

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
                valueList.AddValue($"PID-5|{patientName.Serialize()?.Replace("&&", "")}");
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
    }

    private static void HandleDocumentEntryUniqueId(DocumentReference documentReference, Binary? fhirBinary, OperationOutcome operationOutcome, ExtrinsicObjectType extrinsicObject)
    {
        /* XDSDocumentEntry.uniqueId */
        if (!string.IsNullOrWhiteSpace(documentReference.Id) || !string.IsNullOrWhiteSpace(fhirBinary?.Id))
        {
            extrinsicObject.AddExternalIdentifier(new ExternalIdentifierType()
            {
                Id = Guid.NewGuid().ToString(),
                ObjectType = Constants.Xds.ObjectTypes.ExternalIdentifier,
                RegistryObject = documentReference.Id,
                IdentificationScheme = Constants.Xds.Uuids.DocumentEntry.UniqueId,
                Name = new InternationalStringType(Constants.Xds.ExternalIdentifierNames.DocumentEntryUniqueId),
                Value = fhirBinary?.Id?.NoUrn() ?? documentReference.Id,
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
    }

    private static void HandleDocumentEntryConfidentialityCode(DocumentReference documentReference, OperationOutcome operationOutcome, ExtrinsicObjectType extrinsicObject)
    {
        /* XDSDocumentEntry.ConfidentialityCode (1..*) - required */

        if (documentReference!.SecurityLabel.Count > 0 ||
            documentReference!.SecurityLabel.FirstOrDefault()?.Coding.Count > 0)
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
    }

    private static void HandleDocumentEntryTypeCode(DocumentReference documentReference, OperationOutcome operationOutcome, ExtrinsicObjectType extrinsicObject)
    {
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
    }

    private static void HandleDocumentEntryClassCode(DocumentReference documentReference, OperationOutcome operationOutcome, ExtrinsicObjectType extrinsicObject)
    {
        /* XDSDocumentEntry.classCode */
        var classCode = documentReference.Category.FirstOrDefault()?.Coding.FirstOrDefault();
        if (classCode != null)
        {
            var classCodeClassification = new ClassificationType
            {
                Id = Guid.NewGuid().ToString(),
                ClassifiedObject = documentReference.Id?.NoUrn() ?? "Unknown",
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

            if (!string.IsNullOrWhiteSpace(classCode.System))
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
    }

    private static void HandleDocumentEntryCreationTime(Attachment? attachment, ExtrinsicObjectType extrinsicObject)
    {
        if (!string.IsNullOrWhiteSpace(attachment?.Creation))
        {
            var documentCreationTime = DateTime.Parse(attachment.Creation ?? DateTime.MinValue.ToString(CultureInfo.InvariantCulture)).ToUniversalTime();
            /* XDSDocumentEntry.creationTime - mandatory */
            extrinsicObject.AddSlot(new(Constants.Xds.SlotNames.CreationTime, documentCreationTime.ToString(Constants.Hl7.Dtm.DtmFormat)));
        }
        else
        {
            throw new ArgumentNullException(nameof(attachment), "Creation time not set. 'attachment.Creation' (ExtrinsicObject.Slot(creationTime))");
        }
    }

    private static void HandleDocumentEntryLanguageCode(Attachment? attachment, ExtrinsicObjectType extrinsicObject)
    {
        /* XDSDocumentEntry.languageCode - mandatory */
        extrinsicObject.AddSlot(new(Constants.Xds.SlotNames.LanguageCode, attachment?.Language ?? "Unknown"));
    }

    private static void HandleDocumentEntrySourcePatientId(Identifier? patientId, string? gpiOid, ExtrinsicObjectType extrinsicObject)
    {
        /* XDSDocumentEntry.sourcePatientId - mandatory */
        var patientCx = new CX(patientId?.Value ?? "Unknown", gpiOid);
        extrinsicObject.AddSlot(new(Constants.Xds.SlotNames.SourcePatientId, patientCx.Serialize()?.Replace("&&", "") ?? throw new ArgumentNullException("Patient Identifier cannot be null", "ExtrinsicObject.Slot(sourcePatientId)")));
    }

    private static void HandleDocumentEntryPracticeSettingCode(DocumentReference documentReference, OperationOutcome operationOutcome, ExtrinsicObjectType extrinsicObject)
    {
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
    }

    private static void HandleDocumentEntryHealthcareFacilityTypeCode(DocumentReference documentReference, OperationOutcome operationOutcome, ExtrinsicObjectType extrinsicObject)
    {
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
    }

    private static void HandleDocumentEntryFormatCode(DocumentReference documentReference, OperationOutcome operationOutcome, ExtrinsicObjectType extrinsicObject)
    {
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
    }

    private static string GetDocumentReferenceStatus(DocumentReference documentReference)
    {
        return documentReference.Status switch
        {
            DocumentReferenceStatus.Current => "urn:oasis:names:tc:ebxml-regrep:StatusType:Approved",
            DocumentReferenceStatus.Superseded => "urn:oasis:names:tc:ebxml-regrep:StatusType:Deprecated",
            _ => "urn:oasis:names:tc:ebxml-regrep:StatusType:Deprecated"
        };
    }

    private void HandleDocumentEntryAuthors(DocumentReference documentReference, OperationOutcome operationOutcome, ExtrinsicObjectType extrinsicObject)
    {
        /* XDSDocumentEntry.author */
        var authorClassifications = GetDocumentReferenceAuthors(documentReference, out var outcome);
        if (authorClassifications != null)
        {
            extrinsicObject.AddClassificationRange(authorClassifications);
        }

        operationOutcome.Issue.AddRange(outcome.Issue);
    }

    private static void HandleDocumentEntryServiceStopTime(DocumentReference documentReference, OperationOutcome operationOutcome, ExtrinsicObjectType extrinsicObject)
    {
        /* XDSDocumentEntry.serviceStopTime */
        if (!string.IsNullOrEmpty(documentReference.Context?.Period?.End))
        {
            var datePeriodTo = DateTime.Parse(documentReference.Context.Period.End);
            extrinsicObject.AddSlot(new SlotType(Constants.Xds.SlotNames.ServiceStopTime, datePeriodTo.ToString(Constants.Hl7.Dtm.DtmFormat)));
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
    }

    private static void HandleDocumentEntryServiceStartTime(DocumentReference documentReference, OperationOutcome operationOutcome, ExtrinsicObjectType extrinsicObject)
    {
        /* XDSDocumentEntry.serviceStartTime */
        if (!string.IsNullOrEmpty(documentReference.Context?.Period?.Start))
        {
            var datePeriodFrom = DateTime.Parse(documentReference.Context.Period.Start);
            extrinsicObject.AddSlot(new SlotType(Constants.Xds.SlotNames.ServiceStartTime, datePeriodFrom.ToString(Constants.Hl7.Dtm.DtmFormat)));
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
    }

    private ClassificationType[] GetDocumentReferenceAuthors(DocumentReference documentReference, out OperationOutcome operationOutcome)
    {
        operationOutcome = new();
        var classificationList = new List<ClassificationType>();

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
        }

        /*- Special case => just 1 practitioner and 1 organization provided in DocumentReference - without any practitionerRole -*/
        if (listPractitioner.Count == 1 && listOrganization.Count == 1 && listPractitionerRole.Count == 0)
        {
            var listAuthorSlots = new List<SlotType>();
            var practitionerReference = listPractitioner.FirstOrDefault();
            var orgReference = listOrganization.FirstOrDefault();

            var listProcessedOrganization = new List<ResourceReference>();

            // Practitioner
            AddAuthorPersonSlot(documentReference, practitionerReference, ref listAuthorSlots, ref operationOutcome);

            if (practitionerReference != null)
                listPractitioner.Remove(practitionerReference);

            // Organization
            AddAuthorInstitutionSlot(documentReference, orgReference, ref listOrganization, ref listProcessedOrganization, ref listAuthorSlots, ref operationOutcome);

            if (orgReference != null)
                listOrganization.Remove(orgReference);

            classificationList.Add(new ClassificationType()
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

                    // Organization
                    AddAuthorInstitutionSlot(documentReference, orgReference,
                        ref listOrganization,
                        ref listProcessedOrganization,
                        ref listAuthorSlots,
                        ref operationOutcome);
                    if (orgReference != null)
                    {
                        listProcessedOrganization.Add(orgReference);
                    }

                    // Role
                    AddAuthorRoleSlot(authorRole, ref listAuthorSlots, ref operationOutcome);

                    // Specialty
                    AddAuthorSpecialtySlot(authorSpecialty, ref listAuthorSlots, ref operationOutcome);

                    // Add processed reference of PractitionerRole to processed list
                    listProcessedPractitionerRole.Add(roleReference);

                    classificationList.Add(new ClassificationType()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ClassifiedObject = documentReference.Id?.NoUrn() ?? "Unknown",
                        ClassificationScheme = Constants.Xds.Uuids.DocumentEntry.Author,
                        ObjectType = Constants.Xds.ObjectTypes.Classification,
                        NodeRepresentation = string.Empty,
                        Slot = [.. listAuthorSlots]
                    });
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
                if (processedOrganization == null)
                {
                    continue;
                }

                var processedOrg = listOrganization.FirstOrDefault(x => x.Reference == processedOrganization.Reference);
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
                    if (orgReference != null)
                    {
                        listProcessedOrganization.Add(orgReference);
                    }

                    // Role
                    AddAuthorRoleSlot(authorRole, ref listAuthorSlots, ref operationOutcome);

                    // Specialty
                    AddAuthorSpecialtySlot(authorSpecialty, ref listAuthorSlots, ref operationOutcome);

                    classificationList.Add(new ClassificationType()
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
                    if (processedOrganization == null)
                    {
                        continue;
                    }

                    var processedOrganizations = listOrganization.FirstOrDefault(x => x.Reference == processedOrganization.Reference);

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

                classificationList.Add(new ClassificationType()
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
                    if (processedOrganization == null)
                    {
                        continue;
                    }

                    var processedOrganizations = listOrganization.FirstOrDefault(x => x.Reference == processedOrganization.Reference);

                    if (processedOrganizations != null)
                    {
                        listOrganization.Remove(processedOrganizations);
                    }
                }
                listProcessedOrganization.Clear();
            }
        }

        return classificationList.ToArray();
    }
        
    private static void AddAuthorPersonSlot(DocumentReference documentReference, ResourceReference? practitionerReference, ref List<SlotType> listAuthorSlots, ref OperationOutcome operationOutcome)
    {
        var refAuthorPerson = GetAuthorPerson(documentReference, practitionerReference);
        if (refAuthorPerson != null)
        {
            var refAuthorPersonString = refAuthorPerson.Serialize()?.Replace("&&", "");

            if (!string.IsNullOrWhiteSpace(refAuthorPersonString))
            {
                listAuthorSlots.Add(new SlotType(Constants.Xds.SlotNames.AuthorPerson, refAuthorPersonString));
            }
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No author person found.",
                Location = ["DocumentReference.Author.authorPerson"]
            });
        }
    }

    private void AddAuthorInstitutionSlot(DocumentReference documentReference, ResourceReference? orgReference,
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
                OrganizationName = refAuthorOrg.OrganizationName
            };

            var refAuthorOrgNameOnlyString = refAuthorOrgNameOnly.Serialize();
            var refAuthorOrgString = refAuthorOrg.Serialize();

            var authorInstitutionSlot = new SlotType("authorInstitution");

            var refAuthorDeptString = refAuthorDept?.Serialize();

            authorInstitutionSlot.AddValue(refAuthorOrgString);

            authorInstitutionSlot.AddValue(refAuthorOrgNameOnlyString);

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
        if (authorSpecialty != null)
        {
            listAuthorSlots.Add(new SlotType(Constants.Xds.SlotNames.AuthorSpecialty, [.. authorSpecialty]));
        }
        else
        {
            operationOutcome!.AddIssue(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Warning,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No author specialty found.",
                Location = ["DocumentReference.Author.authorSpecialty"]
            });
        }
    }

    private static void AddAuthorRoleSlot(List<string>? authorRole, ref List<SlotType> listAuthorSlots, ref OperationOutcome operationOutcome)
    {
        if (authorRole != null)
        {
            listAuthorSlots.Add(new SlotType(Constants.Xds.SlotNames.AuthorRole, [.. authorRole]));
        }
        else
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Warning,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = "No author role found.",
                Location = ["DocumentReference.Author.PractitionerRole"]
            });
        }
    }

    private static string? GetAuthorReferenceTarget(DocumentReference documentReference, ResourceReference authorReference)
    {
        var containedRef = documentReference.Contained.FirstOrDefault(x => x.Id == authorReference.Reference?.Trim('#'));

        return containedRef?.TypeName;
    }

    private static ServiceResultDto<AssociationType> CreateAssociationForSubmissionSet(ExtrinsicObjectType? extrinsicObject, RegistryPackageType? registryPackage)
    {
        var operationOutcome = new OperationOutcome();
        var association = new AssociationType()
        {
            Id = Guid.NewGuid().ToString(),
            ObjectType = Constants.Xds.ObjectTypes.Association,
            AssociationTypeData = Constants.Xds.AssociationType.HasMember,
            Slot = [new SlotType(Constants.Xds.SlotNames.SubmissionSetStatus, "Original")]
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

    private static ServiceResultDto<DocumentType> ConvertBinaryToDocument(Binary? fhirBinary)
    {
        var operationOutcome = new OperationOutcome();
        var document = new DocumentType();

        if (fhirBinary?.Data != null)
        {
            document.Value = fhirBinary.Data;
            document.Id = fhirBinary.Id;
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

    private XON? GetAuthorDepartment(DocumentReference documentReference, List<ResourceReference> listOrganization, ResourceReference? parentOrgReference, out ResourceReference? deptOrgReference)
    {
        deptOrgReference = null;

        var referencedOrganization = documentReference.Contained?
            .OfType<Organization>()
            .FirstOrDefault(org => org.Id == parentOrgReference?.Reference?.Trim('#'));

        Organization? authorDept = null;

        if (referencedOrganization?.PartOf != null)
        {
            authorDept = referencedOrganization;
        }
        else
        {
            authorDept = documentReference.Contained?
                .OfType<Organization>()
                .FirstOrDefault(dpt => dpt.PartOf?.Reference == parentOrgReference?.Reference);
        }

        if (authorDept?.Id == null || authorDept.Name == null)
        {
            return null;
        }

        deptOrgReference = listOrganization.FirstOrDefault(orgRef =>
            string.Equals(orgRef.Reference, $"#{authorDept.Id}", StringComparison.Ordinal));

        var deptType = authorDept.Type.FirstOrDefault()?.Coding.FirstOrDefault();


        var deptOid = "";
        var deptName = "";

        var department = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Other.OrganizationAssigningAuthorities).GetFirstValueByName("Department");
        var departmentAlternate = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Other.OrganizationAssigningAuthorities).GetFirstValueByName("DepartmentAlternate");

        if (deptType is { System: "http://terminology.hl7.org/CodeSystem/organization-type", Code: "dept" })
        {
            deptOid = departmentAlternate;
            deptName = authorDept.Name;
        }

        var deptIdentifier = authorDept.Identifier.FirstOrDefault();

        if (deptIdentifier?.System.IsAnyOf(department, departmentAlternate) == true)
        {
            deptOid = deptIdentifier.System;
            deptName = deptIdentifier.Value;
        }

        var authorDepartment = new XON()
        {
            OrganizationName = deptName,
            AssigningAuthority = new HD()
            {
                UniversalId = deptOid,
                UniversalIdType = "ISO"
            },
            OrganizationIdentifier = authorDept.Identifier.FirstOrDefault()?.Value ?? $"name-only:{deptName}"
        };

        return authorDepartment;
    }

    private XON? GetAuthorDepartment(List submissionSet)
    {
        var authorDept = submissionSet.Contained
            .OfType<Organization>()
            .FirstOrDefault(dpt => dpt.PartOf != null);

        if (authorDept?.Name == null)
        {
            return null;
        }


        // Define if department is RESH-type (urn:oid:***.102) or evt. defined with nhn-oid for department (urn:oid:***.390)
        // potentionally can be expressed as HL7-code "dept" which should be also accepted

        var deptType = authorDept.Type.FirstOrDefault()?.Coding.FirstOrDefault();

        var deptOid = "";
        var deptName = "";

        var departmentAlternate = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Other.OrganizationAssigningAuthorities).GetFirstValueByName("DepartmentAlternate");
        var department = _terminologyService.GetCodeSystemByKey(CodeSystemNames.Other.OrganizationAssigningAuthorities).GetFirstValueByName("Department");

        if (deptType is { System: "http://terminology.hl7.org/CodeSystem/organization-type", Code: "dept" })
        {
            deptOid = departmentAlternate;
            deptName = authorDept.Name;
        }

        var deptIdentifier = authorDept.Identifier.FirstOrDefault();

        if (deptIdentifier?.System.IsAnyOf(department, departmentAlternate) == true)
        {
            deptOid = deptIdentifier.System;
            deptName = deptIdentifier.Value;
        }

        var authorDepartment = new XON()
        {
            OrganizationName = deptName,
            AssigningAuthority = new HD()
            {
                UniversalId = deptOid,
                UniversalIdType = "ISO"
            },
            OrganizationIdentifier = authorDept.Identifier.FirstOrDefault()?.Value ?? $"name-only:{deptName}"
        };

        return authorDepartment;
    }

    internal static XON? GetAuthorOrganization(DocumentReference documentReference, ResourceReference? authorReference)
    {
        // Opposite to getting department (organization that does not have a "partOf" field)
        var authorOrg = documentReference.Contained.OfType<Organization>().Where(x => x.Id == authorReference?.Reference?.Trim('#')).FirstOrDefault(dpt => dpt.PartOf == null);

        if (authorOrg == null)
        {
            // Fallback. If authorRole points to department, try to check if this author reference has any departemnt and get that department
            var possibleDepartment = documentReference.Contained.OfType<Organization>().Where(x => x.Id == authorReference?.Reference?.Trim('#')).FirstOrDefault(dpt => dpt.PartOf != null);
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
        if (authorOrg?.Identifier.Count > 0)
        {
            authorOrganization.AssigningAuthority = new HD()
            {
                UniversalId = $"{Organization}",
                UniversalIdType = "ISO"
            };
            authorOrganization.OrganizationIdentifier = authorOrg?.Identifier?.FirstOrDefault()?.Value ?? string.Empty;
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
        if (authorOrg?.Identifier.Count > 0)
        {
            authorOrganization.AssigningAuthority = new HD()
            {
                UniversalId = $"{Organization}",
                UniversalIdType = "ISO"
            };
            authorOrganization.OrganizationIdentifier = authorOrg?.Identifier?.FirstOrDefault()?.Value ?? string.Empty;
        }
        ;
        return authorOrganization;
    }

    private static XCN? GetAuthorPerson(DocumentReference documentReference, ResourceReference? authorReference)
    {
        var authorDocRef = documentReference.Contained.OfType<Practitioner>().Where(x => x.Id == authorReference?.Reference?.Trim('#')).FirstOrDefault();

        if (authorDocRef == null)
        {
            return null;
        }
        var authorName = authorDocRef.Name.FirstOrDefault();

        var author = new XCN()
        {
            GivenName = (authorName?.Given.Count() == 1 ? authorName.Given.FirstOrDefault() : string.Join(" ", authorName?.Given ?? [])) ?? "Unknown",
            FamilyName = authorName?.Family ?? "Unknown",

        };

        if (authorDocRef?.Identifier.Count > 0)
        {
            foreach (var identity in authorDocRef!.Identifier)
            {
                author.PersonIdentifier = identity.Value ?? "Unknown";
                author.AssigningAuthority = new HD()
                {
                    NamespaceId = identity.System,
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
        var authorName = authorDocRef.Name.FirstOrDefault();

        var author = new XCN()
        {
            GivenName = (authorName?.Given.Count() == 1 ? authorName.Given.FirstOrDefault() : string.Join(" ", authorName?.Given ?? [])),
            FamilyName = authorName?.Family,
        };

        if (authorDocRef?.Identifier.Count > 0)
        {
            foreach (var identity in authorDocRef!.Identifier)
            {
                author.PersonIdentifier = identity.Value;
                author.AssigningAuthority = new HD()
                {
                    NamespaceId = identity.System,
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
            .FirstOrDefault(x => (x.Id == roleReference.Reference?.Trim('#')) && (x.Practitioner?.Reference == practitionerReference.Reference));

        if (authorDocRef == null || authorDocRef.Practitioner?.Url != practitionerReference!.Url) return;

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


    internal static XCN? GetPatient(Patient? bundlePatient, DocumentReference documentReference, string? GpiOid)
    {
        var patientDocRef = documentReference.Contained.OfType<Patient>().FirstOrDefault() ?? bundlePatient;

        if (patientDocRef == null)
        {
            return null;
        }
        var patientName = patientDocRef.Name.FirstOrDefault();

        var patient = new XCN()
        {
            GivenName = (patientName?.Given.Count() == 1 ? patientName.Given.FirstOrDefault() : string.Join(" ", patientName?.Given ?? [])) ?? "Unknown",
            FamilyName = patientName?.Family ?? "Unknown",
            PersonIdentifier = patientDocRef?.Identifier?.FirstOrDefault()?.Value ?? "Unknown",
            AssigningAuthority = new HD()
            {
                NamespaceId = GpiOid,
                UniversalIdType = "ISO"
            }
        };

        return patient;
    }

    internal static XCN? GetPatient(Identifier? identifier, string? assigningAuthorityId)
    {
        var patientId = identifier?.Value;

        if (patientId == null)
        {
            return null;
        }

        var patient = new XCN()
        {
            PersonIdentifier = patientId,
            AssigningAuthority = new HD()
            {
                NamespaceId = assigningAuthorityId,
                UniversalIdType = "ISO"
            }
        };

        return patient;
    }

    public string GenerateRandomOid()
    {
        // Generate a random suffix for uniqueness
        var random = new Random();
        var randomSuffix = $"{random.Next(1, 99999)}.{random.Next(1, 99999)}.{random.Next(1, 99999)}";

        // Combine base and suffix
        var randomOid = $"{_applicationConfig.HomeCommunityId}.7.4.{randomSuffix}";
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
                    Constants.Xds.AssociationType.Replace,

                DocumentRelationshipType.Transforms =>
                    Constants.Xds.AssociationType.Transformation,

                DocumentRelationshipType.Appends =>
                    Constants.Xds.AssociationType.Addendum,

                DocumentRelationshipType.Signs =>
                    Constants.Xds.AssociationType.Signs,

                //DocumentRelationshipType.IsSnapshotOf =>
                //	Xds.Constants.Xds.AssociationType.SnapshotOfOnDemandDocumentEntry,

                _ => throw new NotSupportedException(
                    $"Unsupported DocumentRelationshipType '{rel.Code}'")
            };

            result.Add(new AssociationType
            {
                Id = Guid.NewGuid().ToString(),
                ObjectType = Constants.Xds.ObjectTypes.Association,
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
        List<Binary>? binaries,
        OperationOutcome operationOutcome)
    {
        if (binaries?.Count == 0) return null;

        // 1) Try attachment.url → Bundle.Entry.fullUrl
        var attachmentUrl = documentReference.Content
            .FirstOrDefault()?.Attachment?.Url;

        if (!string.IsNullOrWhiteSpace(attachmentUrl))
        {
            //// Normalize reference (strip resource prefix if present)
            //var normalizedRef = attachmentUrl.StartsWith("Binary/", StringComparison.OrdinalIgnoreCase) ? attachmentUrl : attachmentUrl;
            var normalizedRef = attachmentUrl;

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

            var binaryById = binaries?.FirstOrDefault(b =>
                string.Equals(b.Id, idCandidate, StringComparison.OrdinalIgnoreCase));

            if (binaryById != null)
            {
                return binaryById;
            }
        }

        // 3) Fallback to index-based matching
        var fallbackBinary = binaries?.ElementAtOrDefault(indexFallback);

        if (fallbackBinary != null)
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Warning,
                Code = OperationOutcome.IssueType.Informational,
                Diagnostics = $"Binary matched to DocumentReference '{documentReference.Id}' using index fallback. " +
                $"Consider using DocumentReference.content.attachment.url referencing Bundle.entry.fullUrl.",
                Location = ["DocumentReference.content.attachment.url"]
            });
        }

        return fallbackBinary;
    }

    private static OperationOutcome ValidateDocumentRelations(DocumentReference dr)
    {
        var operationOutcome = new OperationOutcome();

        if (dr.RelatesTo == null || dr.RelatesTo.Count == 0)
            return operationOutcome;

        void Error(string diagnostics, params string[] location)
        {
            operationOutcome.AddIssue(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.Invalid,
                Diagnostics = diagnostics,
                Location = location?.Length > 0 ? location : new[] { "DocumentReference.relatesTo" }
            });
        }

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
            var candidate = StripDocRefPrefix(reference).Trim().NoUrn();

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
        if (operationOutcome.Issue.Any(i => i.Severity == OperationOutcome.IssueSeverity.Error))
            return operationOutcome;

        // --- 2) Prevent "document relates to itself" ---
        var selfId = dr.Id?.NoUrn();

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
            .Select(r => StripDocRefPrefix(r).NoUrn())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (nonSignTargets.Count > 1)
            Error("All non-sign relationships must reference the same target document.");

        return operationOutcome;
    }
}