using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;

using static XcaXds.Commons.Commons.Constants.Oid.CodeSystems.Hl7.PurposeOfUse;

namespace XcaXds.Tests;

public class UnitTests_BusinessLogic
{
    private List<IdentifiableType>? _documentReferences = new();


    [Fact]
    public async Task Citizen_1_ShouldOpenDocumentsOnThemself()
    {
        SetupTests();

        var patientId = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-30).Year.ToString().Substring(2, 2)}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid },
            Subject = new() { Code = patientId, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicFilteringService.GetAgeFromPatientId(patientId),
            Resource = new() { Code = patientId, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicFilteringService.GetAgeFromPatientId(patientId),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        _documentReferences = _documentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic)?.ToList();

        Assert.Equal(3, _documentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_2_12To16_ShouldGetEmptyDocumentList()
    {
        SetupTests();
        var patientId12To16Years = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-13).Year.ToString().Substring(2, 2)}79740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid },
            Subject = new() { Code = patientId12To16Years, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicFilteringService.GetAgeFromPatientId(patientId12To16Years),
            Resource = new() { Code = patientId12To16Years, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicFilteringService.GetAgeFromPatientId(patientId12To16Years),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        _documentReferences = _documentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic)?.ToList();

        Assert.Empty(_documentReferences);
    }

    [Fact]
    public async Task Citizen_3_16To18_ShouldAccessPartsOfDocumentList()
    {
        SetupTests();

        var patientId16To18Years = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-17).Year.ToString().Substring(2, 2)}79740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid },
            Subject = new() { Code = patientId16To18Years, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicFilteringService.GetAgeFromPatientId(patientId16To18Years),
            Resource = new() { Code = patientId16To18Years, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicFilteringService.GetAgeFromPatientId(patientId16To18Years),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        _documentReferences = _documentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic)?.ToList();

        Assert.Equal(2, _documentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_4_ShouldAccessChildrenBelow12DocumentList()
    {
        SetupTests();

        var resourceBelow12Years = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-6).Year.ToString().Substring(2, 2)}79740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-30).Year.ToString().Substring(2, 2)}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.RepresentCitizenUnder12,
            Purpose = new() { Code = FAMRQT, CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicFilteringService.GetAgeFromPatientId(subject),
            Resource = new() { Code = resourceBelow12Years, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicFilteringService.GetAgeFromPatientId(resourceBelow12Years),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        _documentReferences = _documentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic)?.ToList();

        Assert.Equal(3, _documentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_5_ShouldAccessPowerOfAttorneyDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-70).Year.ToString().Substring(2, 2)}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-30).Year.ToString().Substring(2, 2)}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.RepresentAnotherCitizen,
            Purpose = new() { Code = PWATRNY, CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicFilteringService.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicFilteringService.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        _documentReferences = _documentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic)?.ToList();

        Assert.Equal(2, _documentReferences?.Count);
    }

    [Fact]
    public async Task Citizen_6_ShouldNotAccessNonPowerOfAttorneyDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-70).Year.ToString().Substring(2, 2)}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-30).Year.ToString().Substring(2, 2)}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = PATRQT, CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicFilteringService.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicFilteringService.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        _documentReferences = _documentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic)?.ToList();

        Assert.Empty(_documentReferences);
    }

    [Fact]
    public async Task HealthcarePersonell_7_ShouldAccessTheirOwnDocumentList()
    {
        SetupTests();

        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-30).Year.ToString().Substring(2, 2)}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = TREAT, CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicFilteringService.GetAgeFromPatientId(subject),
            Resource = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicFilteringService.GetAgeFromPatientId(subject),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        _documentReferences = _documentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic)?.ToList();

        Assert.Equal(2, _documentReferences?.Count);
    }

    [Fact]
    public async Task HealthcarePersonell_8_ShouldAccessPatientsDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-70).Year.ToString().Substring(2, 2)}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-30).Year.ToString().Substring(2, 2)}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = TREAT, CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicFilteringService.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicFilteringService.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        _documentReferences = _documentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic)?.ToList();

        Assert.Equal(2, _documentReferences?.Count);
    }

    [Fact]
    public async Task HealthcarePersonell_9_EmergencyShouldAccessPatientsDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-70).Year.ToString().Substring(2, 2)}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-30).Year.ToString().Substring(2, 2)}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = ETREAT, CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicFilteringService.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicFilteringService.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        _documentReferences = _documentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic)?.ToList();

        Assert.Equal(3, _documentReferences?.Count);
    }

    [Fact]
    public async Task HealthcarePersonell_10_IfMissingAttributesShouldNotAccessDocumentList()
    {
        SetupTests();

        var resource = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-70).Year.ToString().Substring(2, 2)}39740";
        var subject = $"{DateTime.Now.AddDays(-1).Day}{DateTime.Now.Month}{DateTime.Now.AddYears(-30).Year.ToString().Substring(2, 2)}39740";

        var businessLogic = new BusinessLogicParameters()
        {
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = "FEILVERDI", CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid },
            Subject = new() { Code = subject, CodeSystem = Constants.Oid.Fnr },
            SubjectAge = BusinessLogicFilteringService.GetAgeFromPatientId(subject),
            Resource = new() { Code = resource, CodeSystem = Constants.Oid.Fnr },
            ResourceAge = BusinessLogicFilteringService.GetAgeFromPatientId(resource),
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        _documentReferences = _documentReferences.FilterRegistryObjectListBasedOnBusinessLogic(businessLogic)?.ToList();

        Assert.Empty(_documentReferences);
    }

    private void SetupTests()
    {
        var documentEntry1 = new DocumentEntryDto()
        {
            ConfidentialityCode =
            [
                new()
                {
                    CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid,
                    Code = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Normal
                }
            ],
        };

        var documentEntry2 = new DocumentEntryDto()
        {
            ConfidentialityCode =
            [
                new()
                {
                    CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid,
                    Code = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Normal
                },
                new()
                {
                    CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid,
                    Code = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Restricted
                }
            ],
        };

        var documentEntry3 = new DocumentEntryDto()
        {
            ConfidentialityCode =
            [
                new()
                {
                    CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid,
                    Code = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Normal
                },
                new()
                {
                    CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid,
                    Code = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Restricted
                },
                new()
                {
                    CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid,
                    Code = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.VeryRestricted
                }
            ],
        };

        _documentReferences = RegistryMetadataTransformerService.TransformDocumentReferenceDtoListToRegistryObjects([documentEntry1, documentEntry2, documentEntry3]).ToList();
    }
}