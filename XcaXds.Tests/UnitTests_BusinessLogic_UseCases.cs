using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators.BusinessLogic;
using XcaXds.Commons.DataManipulators.Tests;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;
using Xunit.Abstractions;
using static XcaXds.Commons.Commons.Constants.CodeSystems.Hl7.PurposeOfUse;
using static XcaXds.Commons.Commons.Constants.CodeSystems.OtherIsoDerived.PurposeOfUse;

namespace XcaXds.Tests;

public class UnitTests_BusinessLogic_UseCases
{
    private List<IdentifiableType> DocumentReferences = new();
    internal readonly ITestOutputHelper _output;

    public UnitTests_BusinessLogic_UseCases(ITestOutputHelper output)
    {
        _output = output;
    }


    [Fact]
    public async Task Citizen_1_ShouldOpenDocumentsOnThemself()
    {
        SetupTests();

        var patientId = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = SubjectOfCare_13, CodeSystem = Constants.CodeSystems.OtherIsoDerived.PurposeOfUse.System },
            Subject = new() { Code = patientId, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(patientId),
            Resource = new() { Code = patientId, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(patientId),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(2, DocumentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_2_12To16_ShouldGetEmptyDocumentList()
    {
        SetupTests();
        var patientId12To16Years = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-13):yy}79740";

        var yearPart = DateTime.Now.AddYears(-13).Year.ToString().Substring(2, 2);

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = Constants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = patientId12To16Years, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(patientId12To16Years),
            Resource = new() { Code = patientId12To16Years, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(patientId12To16Years),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Empty(DocumentReferences ?? []);
    }

    [Fact]
    public async Task Citizen_3_16To18_ShouldAccessPartsOfDocumentList()
    {
        SetupTests();

        var patientId16To18Years = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-17):yy}79740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = Constants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = patientId16To18Years, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(patientId16To18Years),
            Resource = new() { Code = patientId16To18Years, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(patientId16To18Years),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(2, DocumentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_4_ShouldAccessChildrenBelow12DocumentList()
    {
        SetupTests();

        var resourceBelow12Years = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-6):yy}79740";
        var subject = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.RepresentCitizenUnder12,
            Purpose = new() { Code = SubjectOfCare_13, CodeSystem = Constants.CodeSystems.OtherIsoDerived.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(subject),
            Resource = new() { Code = resourceBelow12Years, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(resourceBelow12Years),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(2, DocumentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_5_ShouldAccessPowerOfAttorneyDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.RepresentAnotherCitizen,
            Purpose = new() { Code = PWATRNY, CodeSystem = Constants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(2, DocumentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_6_ShouldNotAccessNonPowerOfAttorneyDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = Constants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Empty(DocumentReferences ?? []);
    }

    [Fact]
    public async Task Citizen_7_ShouldNotAccessDocumentsForPatientOver12()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-13):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-42):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.RepresentCitizenUnder12,
            Purpose = new() { Code = SubjectOfCare_13, CodeSystem = Constants.CodeSystems.OtherIsoDerived.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Empty(DocumentReferences ?? []);
    }

    [Fact]
    public async Task HealthcarePersonell_7_ShouldAccessTheirOwnDocumentList()
    {
        SetupTests();

        var subject = $"{DateTime.Now.AddDays(-1):dd}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = TREAT, CodeSystem = Constants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(subject),
            Resource = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(subject),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(2, DocumentReferences?.Count);
    }

    [Fact]
    public async Task HealthcarePersonell_8_ShouldAccessPatientsDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = TREAT, CodeSystem = Constants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(resource),
            Scope = ["journaldokumenter_helsepersonell"],
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(3, DocumentReferences?.Count);
    }

    [Fact]
    public async Task HealthcarePersonell_9_EmergencyShouldAccessPatientsDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = ETREAT, CodeSystem = Constants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Equal(3, DocumentReferences?.Count);
    }

    [Fact]
    public async Task HealthcarePersonell_10_IfMissingAttributesShouldNotAccessDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            AppliesTo = AppliesTo.HelseId,
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = "FEILVERDI", CodeSystem = Constants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Empty(DocumentReferences ?? []);
    }

    [Fact]
    public async Task HealthcarePersonell_Custom01()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-70):yy}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now:MM}{DateTime.Now.AddYears(-30):yy}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            AppliesTo = AppliesTo.Helsenorge,
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = "FEILVERDI", CodeSystem = Constants.CodeSystems.Hl7.PurposeOfUse.System },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicMapper.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicMapper.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic, out var applied).ToList();

        _output.WriteLine("Rules applied: " + JsonSerializer.Serialize(applied));

        Assert.Empty(DocumentReferences ?? []);
    }

    private void SetupTests()
    {
        var documentEntry1 = new DocumentEntryDto()
        {
            ConfidentialityCode =
            [
                new()
                {
                    CodeSystem = Constants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = Constants.CodeSystems.Hl7.ConfidentialityCode.Normal
                },
                new()
                {
                    CodeSystem = Constants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = "othercodethatshouldntaffectlogic"
                }
            ],
        };

        var documentEntry2 = new DocumentEntryDto()
        {
            ConfidentialityCode =
            [
                new()
                {
                    CodeSystem = Constants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = Constants.CodeSystems.Hl7.ConfidentialityCode.Normal
                },
                new()
                {
                    CodeSystem = Constants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = Constants.CodeSystems.Hl7.ConfidentialityCode.Restricted
                },
                new()
                {
                    CodeSystem = Constants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = "othercodethatshouldntaffectlogic"
                }

            ],
        };

        var documentEntry3 = new DocumentEntryDto()
        {
            ConfidentialityCode =
            [
                new()
                {
                    CodeSystem = Constants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = Constants.CodeSystems.Hl7.ConfidentialityCode.Normal
                },
                new()
                {
                    CodeSystem = Constants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = Constants.CodeSystems.Hl7.ConfidentialityCode.Restricted
                },
                new()
                {
                    CodeSystem = Constants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = Constants.CodeSystems.Hl7.ConfidentialityCode.VeryRestricted
                },
                new()
                {
                    CodeSystem = Constants.CodeSystems.Hl7.ConfidentialityCode.System,
                    Code = "othercodethatshouldntaffectlogic"
                }

            ],
        };

        DocumentReferences = RegistryMetadataTransformer.TransformDocumentReferenceDtoListToRegistryObjects([documentEntry1, documentEntry2, documentEntry3]).ToList();
    }
}