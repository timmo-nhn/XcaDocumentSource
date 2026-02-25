using XcaXds.Commons.Commons;
using XcaXds.Commons.DataManipulators;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap.XdsTypes;
using Task = System.Threading.Tasks.Task;
using static XcaXds.Commons.Commons.Constants.Oid.CodeSystems.OtherIsoDerived.PurposeOfUse;

namespace XcaXds.Tests;

public class UnitTests_BusinessLogic_ObfuscateDocuments
{
    public List<IdentifiableType>? DocumentReferences { get; private set; }

    [Fact]
    public async Task HealthcarePersonell_TREAT_ShouldPartiallyObfuscate()
    {
        SetupDocumentReferencesWithConfidentialityCodes();

        var businessLogic = new BusinessLogicParameters()
        {
            Issuer = Issuer.HelseId,
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = "TREAT", CodeSystem = Constants.Oid.CodeSystems.Hl7.PurposeOfUse.Oid },
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.ObfuscateRestrictedDocumentEntries(businessLogic, out var entries);

        Assert.Equal(1, entries);
    }

    [Fact]
    public async Task HealthcarePersonell_ETREAT_ShouldNOTObfuscate()
    {
        SetupDocumentReferencesWithConfidentialityCodes();

        var businessLogic = new BusinessLogicParameters()
        {
            Issuer = Issuer.HelseId,
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = "ETREAT", CodeSystem = Constants.Oid.CodeSystems.Hl7.PurposeOfUse.Oid },
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.ObfuscateRestrictedDocumentEntries(businessLogic, out var entries);

        Assert.Equal(0, entries);
    }

    [Fact]
    public async Task Citizen_NormalQuery_ShouldPartiallyObfuscate()
    {
        SetupDocumentReferencesWithConfidentialityCodes();

        var businessLogic = new BusinessLogicParameters()
        {
            Issuer = Issuer.HelseId,
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = SubjectOfCare_13, CodeSystem = Constants.Oid.CodeSystems.OtherIsoDerived.PurposeOfUse.Oid },
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.ObfuscateRestrictedDocumentEntries(businessLogic, out var entries);

        Assert.Equal(1, entries);
    }


    [Fact]
    public async Task Unknown_NormalQuery_ShouldFullyObfuscate()
    {
        SetupDocumentReferencesWithConfidentialityCodes();

        var businessLogic = new BusinessLogicParameters()
        {
            Issuer = Issuer.Unknown,
            Acp = Constants.Oid.Saml.Acp.NullValue,
            Purpose = new() { Code = "invalid code", CodeSystem = "invalid system" },
            SubjectOrganization = new() { Code = "Norsk Helsenett" }
        };

        DocumentReferences = DocumentReferences.ObfuscateRestrictedDocumentEntries(businessLogic, out var entries);

        Assert.Equal(3, entries);
    }

    private void SetupDocumentReferencesWithConfidentialityCodes()
    {
        var documentEntry1 = new DocumentEntryDto()
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
                    CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid,
                    Code = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Normal
                },
                new()
                {
                    CodeSystem = Constants.Oid.CodeSystems.Volven.ConfidentialityCode.Oid,
                    Code = Constants.Oid.CodeSystems.Volven.ConfidentialityCode.NORS
                },
                new()
                {
                    CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid,
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
                    CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid,
                    Code = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Normal
                },
                new()
                {
                    CodeSystem = Constants.Oid.CodeSystems.Volven.ConfidentialityCode.Oid,
                    Code = Constants.Oid.CodeSystems.Volven.ConfidentialityCode.NORN_FFL
                },
                new()
                {
                    CodeSystem = Constants.Oid.CodeSystems.Hl7.ConfidentialityCode.Oid,
                    Code = "othercodethatshouldntaffectlogic"
                }

            ],
        };

        DocumentReferences = RegistryMetadataTransformer.TransformDocumentReferenceDtoListToRegistryObjects([documentEntry1, documentEntry2, documentEntry3]).ToList();
    }
}
