using Microsoft.Extensions.Logging;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture;
using XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using CE = XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types.CE;
using TS = XcaXds.Commons.Models.ClinicalDocumentArchitecture.Types.TS;

namespace XcaXds.Commons.Services;

public static class CdaTransformerService
{
    /// <summary>
    /// Parse a provide and register request and transform into a CDA document.
    /// Will preserve the document content in the CDA documents NonXmlBody 
    /// </summary>
    public static ClinicalDocument TransformRegistryObjectsToClinicalDocument(DocumentEntryDto documentEntry, SubmissionSetDto submissionSet, DocumentDto document)
    {
        var cdaDocument = new ClinicalDocument();

        cdaDocument.Id = SetClinicalDocumentId(documentEntry);

        cdaDocument.Code = SetClinicalDocumentTypeCode(documentEntry);

        cdaDocument.Title = SetClinicalDocumentTitle(documentEntry);

        cdaDocument.EffectiveTime = SetClinicalDocumentEffectiveTime(documentEntry);

        cdaDocument.ConfidentialityCode = SetClinicalDocumentConfidentialityCode(documentEntry);

        // Patient and organization
        cdaDocument.RecordTarget ??= new();
        cdaDocument.RecordTarget.Add(SetClinicalDocumentRecordTarget(documentEntry));

        // ClinicalDocument.author
        cdaDocument.Author ??= new();
        cdaDocument.Author.Add(SetClinicalDocumentAuthor(submissionSet));

        // ClinicalDocument.custodian
        cdaDocument.Custodian ??= new();
        cdaDocument.Custodian = SetClinicalDocumentCustodian(submissionSet);

        // ClinicalDocument.nonXmlBody
        if (document != null)
        {
            cdaDocument.Component ??= new();
            cdaDocument.Component.NonXmlBody = SetClinicalDocumentNonXmlBody(document, documentEntry);
        }

        return cdaDocument;
    }

    private static NonXmlBody? SetClinicalDocumentNonXmlBody(DocumentDto document, DocumentEntryDto documentEntry)
    {
        var nonXmlBody = new NonXmlBody();
        nonXmlBody.Text ??= new();
        nonXmlBody.Text.MediaType = documentEntry.MimeType;
        nonXmlBody.Text.Text = Convert.ToBase64String(document.Data);

        return nonXmlBody;
    }

    private static Author SetClinicalDocumentAuthor(SubmissionSetDto submissionSet)
    {
        var author = new Author();



        author.Time = SetAuthorTime(submissionSet);

        author.AssignedAuthor = SetAssignedAuthor(submissionSet);

        return author;
    }

    private static AssignedAuthor SetAssignedAuthor(SubmissionSetDto submissionSet)
    {
        var assignedAuthor = new AssignedAuthor();

        if (submissionSet.Author?.Person != null)
        {
            assignedAuthor.Id ??= new();
            assignedAuthor.Id.Add(new()
            {
                Extension = submissionSet.Author.Person.Id ?? string.Empty,
                Root = submissionSet.Author.Person.AssigningAuthority ?? string.Empty
            });

            assignedAuthor.AssignedPerson = SetAssignedPerson(submissionSet.Author.Person);
        }
        return assignedAuthor;
    }

    private static Person? SetAssignedPerson(AuthorPerson authorPerson)
    {
        var assignedPerson = new Person();

        assignedPerson.Name ??= new();

        assignedPerson.Name.Add(new()
        {
            Given = [new() { Value = authorPerson.FirstName }],
            Family = [new() { Value = authorPerson.LastName }]
        });

        return assignedPerson;
    }

    private static TS SetAuthorTime(SubmissionSetDto submissionSet)
    {
        return new()
        {
            EffectiveTime = submissionSet.SubmissionTime.HasValue ? submissionSet.SubmissionTime.Value : DateTime.MinValue,
            Value = submissionSet.SubmissionTime.HasValue ? submissionSet.SubmissionTime.Value.ToString(Constants.Hl7.Dtm.DtmFormat) : null,
        };
    }

    private static Custodian SetClinicalDocumentCustodian(SubmissionSetDto submissionsSet)
    {
        var custodian = new Custodian();

        if (submissionsSet.Author == null) return null;

        custodian.AssignedCustodian ??= new();
        custodian.AssignedCustodian.RepresentedCustodianOrganization = SetRepresentedCustodianOrganization(submissionsSet.Author);

        return custodian;
    }

    private static CustodianOrganization SetRepresentedCustodianOrganization(AuthorInfo author)
    {
        if (author == null) return null;
        var custodianOrganization = new CustodianOrganization();

        custodianOrganization.Id ??= new();
        custodianOrganization.Id.Add(new()
        {
            Extension = author.Organization.Id,
            Root = author.Organization.AssigningAuthority
        });

        custodianOrganization.Name ??= new();
        custodianOrganization.Name.XmlText = author.Organization.OrganizationName;

        return custodianOrganization;
    }

    private static RecordTarget SetClinicalDocumentRecordTarget(DocumentEntryDto documentEntry)
    {
        var recordTarget = new RecordTarget();
        var patientRole = new PatientRole();

        // PatientRole.id (ExtrinsicObject.Slot_sourcePatientId)
        recordTarget.PatientRole = SetPatientRole(documentEntry);

        return recordTarget;
    }

    private static TS? SetPatientBirthTime(DateTime? patientBirthTime)
    {
        if (patientBirthTime.HasValue)
        {
            return new()
            {
                EffectiveTime = patientBirthTime.Value,
            };
        }
        return null;
    }

    private static CE SetPatientAdministrativeGenderCode(string? patientGender)
    {
        if (patientGender != null)
        {
            return new()
            {
                Code = patientGender switch
                {
                    "U" => "0",
                    "M" => "1",
                    "F" => "2",
                    "O" => "9",
                    _ => "0"
                },
                CodeSystem = Constants.Oid.CodeSystems.Volven.Gender,
                DisplayName = patientGender switch
                {
                    "U" => "Not known",
                    "M" => "Male",
                    "F" => "Female",
                    "O" => "Not applicable",
                    _ => "Not known"
                }
            };
        }
        return null;
    }

    private static PN? SetPatientName(SourcePatientInfo? sourcePatientInfo)
    {
        if (sourcePatientInfo == null) return null;

        return new()
        {
            Family = [new() { Value = sourcePatientInfo.LastName }],
            Given = [new() { Value = sourcePatientInfo.FirstName }]
        };
    }

    private static PatientRole SetPatientRole(DocumentEntryDto documentEntry)
    {
        var patientRole = new PatientRole();
        // PatientRole.Id
        patientRole.Id ??= new();
        patientRole.Id.Add(SetPatientRoleId(documentEntry));

        // recordTarget.PatientRole.Patient
        var patient = new Patient();

        patient.Name ??= new();
        patient.Name.Add(SetPatientName(documentEntry.SourcePatientInfo));

        patient.BirthTime = SetPatientBirthTime(documentEntry.SourcePatientInfo?.BirthTime);

        patient.AdministrativeGenderCode = SetPatientAdministrativeGenderCode(documentEntry.SourcePatientInfo?.Gender);

        patientRole.Patient = patient;

        patientRole.ProviderOrganization = SetPatientRoleProviderOrganization(documentEntry);

        return patientRole;
    }

    private static Organization? SetPatientRoleProviderOrganization(DocumentEntryDto documentEntry)
    {
        var providerOrganization = new Organization();

        var documentAuthor = documentEntry.Author;

        if (documentAuthor?.Department != null)
        {
            providerOrganization.Id ??= new();
            providerOrganization.Id.Add(new()
            {
                Extension = documentAuthor.Department.Id,
                Root = documentAuthor.Department.AssigningAuthority
            });
            providerOrganization.Name = [new() { XmlText = documentAuthor.Department.OrganizationName }];

        }

        if (documentAuthor?.Organization != null)
        {
            providerOrganization.AsOrganizationPartOf ??= new()
            {
                WholeOrganization = new()
                {
                    Id = [
                        new()
                        {
                             Extension = documentAuthor.Organization.Id,
                             Root = documentAuthor.Organization.AssigningAuthority
                        }
                    ],
                    Name = [new() { XmlText = documentAuthor.Organization.OrganizationName }]
                }
            };
        }

        return providerOrganization;
    }

    private static II? SetPatientRoleId(DocumentEntryDto documentEntry)
    {
        if (documentEntry.PatientId == null) return null;

        return new()
        {
            Root = documentEntry?.PatientId?.CodeSystem,
            Extension = documentEntry?.PatientId?.Code
        };
    }

    private static string SetClinicalDocumentTitle(DocumentEntryDto documentEntry)
    {
        return documentEntry.Title;
    }

    private static CV SetClinicalDocumentConfidentialityCode(DocumentEntryDto documentEntry)
    {
        if (documentEntry.ConfidentialityCode == null) return null;

        return new()
        {
            Code = documentEntry.ConfidentialityCode.Code,
            DisplayName = documentEntry.ConfidentialityCode.DisplayName,
            CodeSystem = documentEntry.ConfidentialityCode.CodeSystem
        };
    }

    private static TS SetClinicalDocumentEffectiveTime(DocumentEntryDto documentEntry)
    {
        return new()
        {
            EffectiveTime = documentEntry.CreationTime.HasValue ? documentEntry.CreationTime.Value : DateTime.MinValue,
            Value = documentEntry.CreationTime.HasValue ? documentEntry.CreationTime.Value.ToUniversalTime().ToString(Constants.Hl7.Dtm.DtmFormat) : string.Empty
        };
    }

    private static CV SetClinicalDocumentTypeCode(DocumentEntryDto documentEntry)
    {

        if (documentEntry.TypeCode == null) return null;

        return new()
        {
            Code = documentEntry.TypeCode.Code,
            DisplayName = documentEntry.TypeCode.DisplayName,
            CodeSystem = documentEntry.TypeCode.CodeSystem
        };
    }

    private static II SetClinicalDocumentId(DocumentEntryDto documentEntry)
    {
        return new()
        {
            Root = documentEntry.RepositoryUniqueId ?? string.Empty,
            Extension = documentEntry.Id ?? Guid.NewGuid().ToString()
        };
    }
}
