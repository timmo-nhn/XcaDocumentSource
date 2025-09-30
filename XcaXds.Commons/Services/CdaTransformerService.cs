using System.Security.Cryptography;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.ClinicalDocument;
using XcaXds.Commons.Models.ClinicalDocument.Types;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Serializers;
using CE = XcaXds.Commons.Models.ClinicalDocument.Types.CE;
using TS = XcaXds.Commons.Models.ClinicalDocument.Types.TS;

namespace XcaXds.Commons.Services;

public static partial class CdaTransformerService
{
    public static DocumentReferenceDto TransformClinicalDocumentToRegistryObjects(ClinicalDocument clinicalDocument)
    {
        var documentReference = new DocumentReferenceDto();

        documentReference.DocumentEntry = GetDoumentEntryFromClinicalDocument(clinicalDocument);

        return documentReference;
    }

    private static DocumentEntryDto? GetDoumentEntryFromClinicalDocument(ClinicalDocument clinicalDocument)
    {
        var documentEntry = new DocumentEntryDto();

        documentEntry.Author = GetAuthorInfoFromClinicalDocument(clinicalDocument);
        documentEntry.AvailabilityStatus = Constants.Xds.StatusValues.Approved;
        documentEntry.ClassCode = GetClassCodeFromClinicalDocument(clinicalDocument);
        documentEntry.ConfidentialityCode = GetConfidentialityCodeFromClinicalDocument(clinicalDocument);
        documentEntry.CreationTime = GetCreationTimeFromClinicalDocument(clinicalDocument);
        documentEntry.FormatCode = GetFormatCodeFromClinicalDocument(clinicalDocument);
        documentEntry.Hash = GetHashFromClinicalDocument(clinicalDocument);
        documentEntry.HomeCommunityId = GetHomeCommunityIdFromClinicalDocument(clinicalDocument);

        return documentEntry;
    }

    private static string? GetHomeCommunityIdFromClinicalDocument(ClinicalDocument clinicalDocument)
    {
        throw new NotImplementedException();
    }

    private static string? GetHashFromClinicalDocument(ClinicalDocument clinicalDocument)
    {
        var sxmls = new SoapXmlSerializer();
        var cdaXml = sxmls.SerializeSoapMessageToXmlString(clinicalDocument).Content;
        if (cdaXml == null) return null;

        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(cdaXml);
        var hashBytes = md5.ComputeHash(inputBytes);

        return Convert.ToHexString(hashBytes);
    }

    private static CodedValue? GetFormatCodeFromClinicalDocument(ClinicalDocument clinicalDocument)
    {
        var formatCode = clinicalDocument.Code;
        if (formatCode == null) return null;

        return new()
        {
            Code = formatCode.Code,
            CodeSystem = formatCode.CodeSystem,
            DisplayName = formatCode.DisplayName
        };
    }

    private static DateTime? GetCreationTimeFromClinicalDocument(ClinicalDocument clinicalDocument)
    {
        return clinicalDocument.EffectiveTime.EffectiveTime.UtcDateTime;
    }

    private static List<CodedValue>? GetConfidentialityCodeFromClinicalDocument(ClinicalDocument clinicalDocument)
    {
        var cdaConfidentialityCode = clinicalDocument.ConfidentialityCode;
        if (cdaConfidentialityCode == null) return null;

        return [new() 
        { 
            Code = cdaConfidentialityCode.Code, 
            CodeSystem = cdaConfidentialityCode.CodeSystem, 
            DisplayName = cdaConfidentialityCode.DisplayName 
        }];
    }

    private static CodedValue? GetClassCodeFromClinicalDocument(ClinicalDocument clinicalDocument)
    {
        return new CodedValue()
        { 
            // https://finnkode.helsedirektoratet.no/adm/collections/9602?q=9602
            Code = clinicalDocument.Code?.Code switch
            {
                string code when code.StartsWith("A") => "A00-1",
                string code when code.StartsWith("B") => "B00-1",
                string code when code.StartsWith("C") => "C00-1",
                string code when code.StartsWith("D") => "D00-1",
                string code when code.StartsWith("E") => "E00-1",
                string code when code.StartsWith("F") => "F00-1",
                string code when code.StartsWith("I") => "I00-1",
                string code when code.StartsWith("J") => "J00-1",
                string code when code.StartsWith("S") => "S00-1",
                _ => clinicalDocument.Code?.Code
            },
            CodeSystem = clinicalDocument.Code?.CodeSystem,
            DisplayName = clinicalDocument.Code?.Code switch
            {
                string code when code.StartsWith("A") => "Epikriser og sammenfatninger",
                string code when code.StartsWith("B") => "Kontinuerlig/løpende journal",
                string code when code.StartsWith("C") => "Prøvesvar, vev og væsker",
                string code when code.StartsWith("D") => "Organfunksjon",
                string code when code.StartsWith("E") => "Bildediagnostikk",
                string code when code.StartsWith("F") => "Kurve, observasjon og behandling",
                string code when code.StartsWith("I") => "Korrespondanse",
                string code when code.StartsWith("J") => "Attester, melding og erklæringer",
                string code when code.StartsWith("S") => "Test og scoring",
                _ => clinicalDocument.Code?.DisplayName
            }
        };
    }

    private static List<AuthorInfo>? GetAuthorInfoFromClinicalDocument(ClinicalDocument clinicalDocument)
    {
        if (clinicalDocument.RecordTarget?.FirstOrDefault()?.PatientRole?.ProviderOrganization == null) return null;

        var authorInfo = new AuthorInfo();

        var assignedAuthor = clinicalDocument.Author?.FirstOrDefault()?.AssignedAuthor;
        authorInfo.Person = GetAuthorPersonFromClinicalDocument(assignedAuthor);

        var department = clinicalDocument.Author?.FirstOrDefault()?.AssignedAuthor?.RepresentedOrganization;
        authorInfo.Department = GetAuthorDepartmentFromClinicalDocument(department);

        var organization = clinicalDocument.Author?.FirstOrDefault()?.AssignedAuthor?.RepresentedOrganization?.AsOrganizationPartOf?.WholeOrganization;
        authorInfo.Organization = GetAuthorOrganizationFromClinicalDocument(organization);

        return [authorInfo];
    }


    private static AuthorPerson? GetAuthorPersonFromClinicalDocument(AssignedAuthor? assignedAuthor)
    {
        if (assignedAuthor == null) return null;

        var authorPerson = new AuthorPerson();

        var authorCdaName = assignedAuthor?.AssignedPerson;

        var authorNames = authorCdaName?.Name?
            .SelectMany(nme =>
            {
                var prefix = nme.Prefix?.Select(g => g.Value) ?? Enumerable.Empty<string>();
                var given = nme.Given?.Select(g => g.Value) ?? Enumerable.Empty<string>();
                var family = nme.Family?.Select(f => f.Value) ?? Enumerable.Empty<string>();
                var suffix = nme.Suffix?.Select(f => f.Value) ?? Enumerable.Empty<string>();
                return new[]
                {
                    string.Join(" ", prefix.Concat(given)),
                    string.Join(" ", family.Concat(suffix))
                };
            }).ToList();

        authorPerson = new()
        {
            FirstName = authorNames?.FirstOrDefault(),
            LastName = authorNames?.LastOrDefault()
        };

        var cdaAuthorId = assignedAuthor?.Id?.FirstOrDefault();

        authorPerson.Id = cdaAuthorId?.Extension;
        authorPerson.AssigningAuthority = cdaAuthorId?.Root;

        return authorPerson;
    }

    private static AuthorOrganization? GetAuthorDepartmentFromClinicalDocument(Organization? department)
    {
        if (department == null) return null;

        var authorDepartment = new AuthorOrganization();

        authorDepartment.AssigningAuthority = department.Id?.FirstOrDefault()?.Root;
        authorDepartment.Id = department.Id?.FirstOrDefault()?.Extension;
        authorDepartment.OrganizationName = department.Name?.FirstOrDefault()?.XmlText;

        return authorDepartment;
    }

    private static AuthorOrganization? GetAuthorOrganizationFromClinicalDocument(Organization? organization)
    {
        if (organization == null) return null;

        var authorOrganization = new AuthorOrganization();

        authorOrganization.AssigningAuthority = organization.Id?.FirstOrDefault()?.Root;
        authorOrganization.Id = organization.Id?.FirstOrDefault()?.Extension;
        authorOrganization.OrganizationName = organization.Name?.FirstOrDefault()?.XmlText;

        return authorOrganization;
    }

    private static CodedValue? GetAuthorSpecialityFromClinicalDocument(ClinicalDocument clinicalDocument)
    {
        throw new NotImplementedException();
    }

    private static CodedValue? GetAuthorRoleFromClinicalDocument(ClinicalDocument clinicalDocument)
    {
        throw new NotImplementedException();
    }

}

public static partial class CdaTransformerService
{
    /// <summary>
    /// Parse a provide and register request and transform into a CDA document.<para/>
    /// Will preserve the document content in the CDA documents NonXmlBody <para/>
    /// <a href="https://build.fhir.org/ig/HL7/CDA-core-2.0/" />
    /// </summary>
    public static ClinicalDocument? TransformRegistryObjectsToClinicalDocument(DocumentEntryDto documentEntry, SubmissionSetDto submissionSet, DocumentDto document)
    {
        var cdaDocument = new ClinicalDocument();

        if (documentEntry != null)
        {
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
            cdaDocument.Author.AddRange(SetClinicalDocumentAuthors(submissionSet));

            // ClinicalDocument.custodian
            cdaDocument.Custodian ??= new();
            cdaDocument.Custodian = SetClinicalDocumentCustodian(submissionSet);

        }

        // ClinicalDocument.nonXmlBody
        if (document != null && document.Data != null)
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

        if (!string.IsNullOrWhiteSpace(nonXmlBody.Text.MediaType) && (nonXmlBody.Text.MediaType == Constants.MimeTypes.Text || nonXmlBody.Text.MediaType.Contains("json")))
        {
            nonXmlBody.Text.Text = Encoding.UTF8.GetString(document.Data ?? []);
        }
        else
        {
            nonXmlBody.Text.Representation = "B64";
            nonXmlBody.Text.Text = Convert.ToBase64String(document.Data ?? []);
        }

        return nonXmlBody;
    }

    private static List<Author> SetClinicalDocumentAuthors(SubmissionSetDto submissionSet)
    {
        var authorList = new List<Author>();

        foreach (var author in submissionSet.Author ?? new List<AuthorInfo>())
        {
            var cdaAauthor = new Author();

            cdaAauthor.Time = SetAuthorTime(submissionSet);

            cdaAauthor.AssignedAuthor = SetAssignedAuthor(author);

            authorList.Add(cdaAauthor);
        }

        return authorList;
    }

    private static AssignedAuthor SetAssignedAuthor(AuthorInfo authorInfo)
    {
        var assignedAuthor = new AssignedAuthor();
        if (authorInfo != null && authorInfo.Person != null)
        {
            assignedAuthor.Id ??= new();
            assignedAuthor.Id.Add(new()
            {
                Extension = authorInfo.Person.Id ?? string.Empty,
                Root = authorInfo.Person.AssigningAuthority ?? string.Empty
            });

            assignedAuthor.AssignedPerson = SetAssignedPerson(authorInfo.Person);
            assignedAuthor.Code = new()
            {
                Code = authorInfo.Role?.Code,
                CodeSystem = authorInfo.Role?.CodeSystem,
                DisplayName = authorInfo.Role?.DisplayName,
            };
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
        custodian.AssignedCustodian.RepresentedCustodianOrganization = SetRepresentedCustodianOrganization(submissionsSet.Author.FirstOrDefault());

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

        var documentAuthor = documentEntry.Author?.FirstOrDefault();

        if (documentAuthor?.Department != null)
        {
            providerOrganization.Id ??= new();
            providerOrganization.Id.Add(new()
            {
                Extension = documentAuthor?.Department?.Id,
                Root = documentAuthor?.Department?.AssigningAuthority
            });
            providerOrganization.Name = [new() { XmlText = documentAuthor?.Department?.OrganizationName }];

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
            Code = documentEntry.ConfidentialityCode.FirstOrDefault()?.Code,
            DisplayName = documentEntry.ConfidentialityCode.FirstOrDefault()?.DisplayName,
            CodeSystem = documentEntry.ConfidentialityCode.FirstOrDefault()?.CodeSystem
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

    private static II? SetClinicalDocumentId(DocumentEntryDto? documentEntry)
    {
        if (documentEntry == null) return null;
        return new()
        {
            Root = documentEntry.RepositoryUniqueId ?? string.Empty,
            Extension = documentEntry.Id ?? Guid.NewGuid().ToString()
        };
    }
}
