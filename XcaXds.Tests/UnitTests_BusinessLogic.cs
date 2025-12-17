using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using XcaXds.Source.Services;
using XcaXds.Tests.Helpers;

namespace XcaXds.Tests;

public class UnitTests_BusinessLogic
{
    private List<IdentifiableType> _documentReferences = new();


    [Fact]
    public async Task EvaluateBusinessLogic_Citizen()
    {
        SetupTests();

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(File.ReadAllText(testDataFiles.FirstOrDefault(f => f.Contains("iti38-iti40-request-hn.xml"))));
        var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(soapEnvelope.Header.Security.Assertion?.OuterXml);

        var appliesTo = PolicyRequestMapperSamlService.GetIssuerEnumFromSamlTokenIssuer(samlToken.Assertion.Issuer.Value);

        var xacmlRequest = PolicyRequestMapperSamlService.GetXacmlRequestFromSamlToken(soapEnvelope, samlToken, XacmlVersion.Version20, appliesTo);

        _documentReferences.FilterRegistryObjectListBasedOnBusinessLogic(xacmlRequest);

        Assert.Equal(3, _documentReferences.Count);
    }

    [Fact]
    public async Task EvaluateBusinessLogic_Citizen_ACP_1()
    {
        SetupTests();

        var registryObjects = TestHelpers.GenerateRegistryMetadata();
        var extrinsicObjects = RegistryMetadataTransformerService.TransformDocumentReferenceDtoListToRegistryObjects(registryObjects);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(testDataFiles.FirstOrDefault(f => f.Contains("iti38-iti40-request-hn-acp-1.xml")));
        var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(soapEnvelope.Header.Security.Assertion?.OuterXml);

        var appliesTo = PolicyRequestMapperSamlService.GetIssuerEnumFromSamlTokenIssuer(samlToken.Assertion.Issuer.Value);

        var xacmlRequest = PolicyRequestMapperSamlService.GetXacmlRequestFromSamlToken(soapEnvelope, samlToken, XacmlVersion.Version20, appliesTo, registryObjects);

        extrinsicObjects.FilterRegistryObjectListBasedOnBusinessLogic(xacmlRequest);
    }

    [Fact]
    public async Task EvaluateBusinessLogic_Citizen_ACP_2()
    {
        SetupTests();

        var registryObjects = TestHelpers.GenerateRegistryMetadata();
        var extrinsicObjects = RegistryMetadataTransformerService.TransformDocumentReferenceDtoListToRegistryObjects(registryObjects);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(testDataFiles.FirstOrDefault(f => f.Contains("iti38-iti40-request-hn-acp-2.xml")));
        var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(soapEnvelope.Header.Security.Assertion?.OuterXml);

        var appliesTo = PolicyRequestMapperSamlService.GetIssuerEnumFromSamlTokenIssuer(samlToken.Assertion.Issuer.Value);

        var xacmlRequest = PolicyRequestMapperSamlService.GetXacmlRequestFromSamlToken(soapEnvelope, samlToken, XacmlVersion.Version20, appliesTo, registryObjects);

        extrinsicObjects.FilterRegistryObjectListBasedOnBusinessLogic(xacmlRequest);
    }

    [Fact]
    public async Task EvaluateBusinessLogic_Citizen_ACP_3()
    {
        SetupTests();

        var registryObjects = TestHelpers.GenerateRegistryMetadata();
        var extrinsicObjects = RegistryMetadataTransformerService.TransformDocumentReferenceDtoListToRegistryObjects(registryObjects);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var soapEnvelope = new SoapXmlSerializer().DeserializeXmlString<SoapEnvelope>(testDataFiles.FirstOrDefault(f => f.Contains("iti38-iti40-request-hn-acp-3.xml")));
        var samlToken = PolicyRequestMapperSamlService.ReadSamlToken(soapEnvelope.Header.Security.Assertion?.OuterXml);

        var appliesTo = PolicyRequestMapperSamlService.GetIssuerEnumFromSamlTokenIssuer(samlToken.Assertion.Issuer.Value);

        var xacmlRequest = PolicyRequestMapperSamlService.GetXacmlRequestFromSamlToken(soapEnvelope, samlToken, XacmlVersion.Version20, appliesTo, registryObjects);

        extrinsicObjects.FilterRegistryObjectListBasedOnBusinessLogic(xacmlRequest);
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