using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Shared;
using XcaXds.Shared.Extensions;
using XcaXds.Tests.FakesAndDoubles;
using XcaXds.Tests.Helpers;
using XcaXds.WebService;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests.IntegrationTests;

#pragma warning disable CS8604, CS8602 // Possible null reference argument.
public class IntegrationTests_FhirMobileAccessToHealthDocuments : IntegrationTests_DefaultFixture, IClassFixture<WebApplicationFactory<WebService.Program>>
{
    public IntegrationTests_FhirMobileAccessToHealthDocuments(WebApplicationFactory<Program> factory, ITestOutputHelper output) : base(factory, output)
    {
    }

    [Fact]
    [Trait("Fetch", "Get DocumentReference")]
    public async Task DocumentReference_GetDocumentReference()
    {
        await NukeRegistryRepository();

        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_machine_getdocumentreference",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments,
            action: "ReadDocumentList",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "Jwt"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var registryObjects = RegistryContent.AsRegistryObjectDtos();

        var registryContentCount = registryObjects.Count();

        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken01")));

        var randomDocumentEntry = RegistryContent.PickRandom().DocumentEntry;

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/R4/fhir/DocumentReference/{randomDocumentEntry?.Id}");

        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var content = await firstResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        _policyRepositoryService.DeleteAllPolicies();
        await NukeRegistryRepository();

        _output.WriteLine("DocumentReference: " + content);
    }

    [Fact]
    [Trait("Fetch", "Get DocumentReference")]
    public async Task DocumentReference_GetDocumentReference_Dept()
    {
        await NukeRegistryRepository();

        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_machine_providebundle",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments,
            action: "Create",
            noCode: true);

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_machine_getdocumentreference",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments,
            action: "ReadDocumentList",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "Jwt"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var registryObjects = RegistryContent.AsRegistryObjectDtos();

        var registryContentCount = registryObjects.Count();

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle02_dept_without_reference_in_authors.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken01")));

        var fhirParser = new FhirJsonDeserializer();
        var fhirBundle = fhirParser.DeserializeResource(fhirProvideBundle);

        var provideBundleDocumentUniqueId = fhirBundle is Bundle bundle
            ? bundle.Entry
                .Select(e => e.Resource)
                .OfType<Binary>()
                .FirstOrDefault().Id
            : null;

        var stringContent = new StringContent(fhirProvideBundle, Encoding.UTF8, Constants.MimeTypes.FhirJson);

        var uploadHttpRequest = new HttpRequestMessage(HttpMethod.Post, "/R4/fhir/Bundle")
        {
            Content = stringContent
        };

        uploadHttpRequest.Headers.Add("Authorization", jsonWebToken);
        var expectedCount = RegistryContent.Count + 1;
        var firstResponse = await _client.SendAsync(uploadHttpRequest, TestContext.Current.CancellationToken);
        var responseContent = await firstResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/R4/fhir/DocumentReference/{provideBundleDocumentUniqueId}");
        httpRequest.Headers.Add("Authorization", jsonWebToken);
        var secondResponse = await _client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var content = await secondResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        _policyRepositoryService.DeleteAllPolicies();
        await NukeRegistryRepository();

        _output.WriteLine("DocumentReference: " + content);
    }

    [Fact]
    [Trait("Fetch", "Provide bundle with department then cross gateway query")]
    public async Task ProvideBundle_With_Department_Then_CrossGatewayQuery()
    {
        await ProvideBundle_With_Department_Then_CrossGatewayQuery_Core("ProvideBundle02_dept_with_reference_in_authors.json");
    }

    [Fact]
    [Trait("Fetch", "Provide bundle with department without direct child author reference then cross gateway query")]
    public async Task ProvideBundle_With_Department_Without_Child_Organization_Author_Reference_Then_CrossGatewayQuery()
    {
        await ProvideBundle_With_Department_Then_CrossGatewayQuery_Core("ProvideBundle02_dept_without_reference_in_authors.json");
    }

    private async Task ProvideBundle_With_Department_Then_CrossGatewayQuery_Core(string bundleFileName)
    {
        await NukeRegistryRepository();

        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_machine_providebundle",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments,
            action: "Create",
            noCode: true);

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_machine_getdocumentreference",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments,
            action: "ReadDocumentList",
            noCode: true);

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "Jwt"));

        //RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        //var registryObjects = RegistryContent.AsRegistryObjectDtos();

        //var registryContentCount = registryObjects.Count();

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains(bundleFileName)));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken01")));

        var fhirParser = new FhirJsonDeserializer();
        var fhirBundle = fhirParser.DeserializeResource(fhirProvideBundle);

        var provideBundleDocumentUniqueId = fhirBundle is Bundle bundle
            ? bundle.Entry
                .Select(e => e.Resource)
                .OfType<Binary>()
                .FirstOrDefault().Id
            : null;

        var stringContent = new StringContent(fhirProvideBundle, Encoding.UTF8, Constants.MimeTypes.FhirJson);

        var uploadHttpRequest = new HttpRequestMessage(HttpMethod.Post, "/R4/fhir/Bundle")
        {
            Content = stringContent
        };

        uploadHttpRequest.Headers.Add("Authorization", jsonWebToken);
        var expectedCount = RegistryContent.Count + 1;
        var firstResponse = await _client.SendAsync(uploadHttpRequest);
        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/R4/fhir/DocumentReference/{provideBundleDocumentUniqueId}");
        httpRequest.Headers.Add("Authorization", jsonWebToken);
        var secondResponse = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var content = await secondResponse.Content.ReadAsStringAsync();

        // Cross gateway query

        testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        testDataFiles = Directory.GetFiles(testDataPath);

        integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var iti38SoapEnvelope = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti-38_request.xml")));

        var crossGatewayQuery = GetSoapEnvelopeWithKjernejournalSamlToken(iti38SoapEnvelope);

        var firstGatewayResponse = await _client.PostAsync("/XCA/services/RespondingGatewayService", new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(firstGatewayResponse.Content.ReadAsStream());

        var responseGatewayContent = await firstGatewayResponse.Content.ReadAsStringAsync();
        var count = firstResponseSoap?.Body.AdhocQueryResponse?.RegistryObjectList?.OfType<ExtrinsicObjectType>()?.Count() ?? 0;

        _output.WriteLine("ResponseGatewayContent: " + responseGatewayContent);

        Assert.Equal(HttpStatusCode.OK, firstGatewayResponse.StatusCode);
        Assert.Equal(1, count);
        Assert.Equal(1, CountOccurrences(responseGatewayContent, $"classificationScheme=\"{Constants.Xds.Uuids.DocumentEntry.Author}\""));
        Assert.Equal(1, CountOccurrences(responseGatewayContent, "<Slot name=\"authorPerson\">"));
        Assert.Equal(1, CountOccurrences(responseGatewayContent, "<Slot name=\"authorInstitution\">"));
        Assert.Contains("OSLO KOMMUNE HELSEETATEN LEGEVAKTEN I", responseGatewayContent);
        Assert.Contains("Allmenlegevakten", responseGatewayContent);

        //var expectedRegistryObjects = BusinessLogicFiltersRegistry.FilterByConfidentiality(RegistryContent.AsRegistryObjectList(), [Normal, Restricted, VeryRestricted]).ToArray();

        // Cleanup

        _policyRepositoryService.DeleteAllPolicies();
        await NukeRegistryRepository();

        _output.WriteLine("DocumentReference: " + content);
    }

    private static int CountOccurrences(string value, string substring)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(substring))
        {
            return 0;
        }

        var count = 0;
        var index = 0;

        while ((index = value.IndexOf(substring, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += substring.Length;
        }

        return count;
    }

    private static XmlDocument? GetSoapEnvelopeWithKjernejournalSamlToken(string soapEnvelope)
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var kjSamlTokenString = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_KJ01")));

        var kjSamlToken = TestHelpers.LoadNewXmlDocument(kjSamlTokenString);
        var soapEnvelopeDocument = TestHelpers.LoadNewXmlDocument(soapEnvelope);

        return GetSoapEnvelopeWithSamlToken(soapEnvelopeDocument, kjSamlToken);
    }

    private static XmlDocument? GetSoapEnvelopeWithSamlToken(XmlDocument? soapEnvelopeDocument, XmlDocument? kjSamlToken)
    {
        var nsmgr = new XmlNamespaceManager(soapEnvelopeDocument.NameTable);
        nsmgr.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        nsmgr.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

        var securityNode = soapEnvelopeDocument.SelectSingleNode("//wsse:Security", nsmgr);

        if (securityNode != null)
        {
            var importedKjToken = soapEnvelopeDocument.ImportNode(kjSamlToken.DocumentElement, true);

            securityNode.AppendChild(importedKjToken);
        }

        return soapEnvelopeDocument;
    }

    [Fact]
    [Trait("Delete", "Delete DocumentReference")]
    public async Task DeleteDocumentsAndMetadata_ExportsAtnaLog_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await DeleteDocumentsAndMetadata_ExportsAtnaLog();
    }

    [Fact]
    [Trait("Delete", "Delete DocumentReference")]
    public async Task DeleteDocumentsAndMetadata_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_machine_deletedocuments",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeDeleteDocument,
            action: "Delete",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "Jwt"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var registryObjects = RegistryContent.AsRegistryObjectDtos();

        var registryContentCount = registryObjects.Count();

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken01")));

        var randomDocumentEntry = RegistryContent.PickRandom().DocumentEntry;

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/R4/fhir/DocumentReference/{randomDocumentEntry?.Id}");

        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var currentRegistry = _registry.ReadRegistry();
        var currentCount = await currentRegistry.CountAsync(TestContext.Current.CancellationToken);

        var expectedCount = registryContentCount - 3;

        _policyRepositoryService.DeleteAllPolicies();
        await NukeRegistryRepository();

        Assert.Equal(expectedCount, currentCount);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine("DeleteDocumentsAndMetadata: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString + "\nUser Access Entry: " + MockStatisticsProcessorService.UserAccessEntryJson);
    }

    [Fact]
    [Trait("Delete", "Delete DocumentReference")]
    public async Task DeleteDocumentsAndMetadata_DocumentDoesNotExist_ExportsAtnaLog_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await DeleteDocumentsAndMetadata_DocumentDoesNotExist_ExportsAtnaLog();
    }

    [Fact]
    [Trait("Delete", "Delete DocumentReference")]
    public async Task DeleteDocumentsAndMetadata_DocumentDoesNotExist_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_machine_deletedocuments",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeDeleteDocument,
            action: "Delete",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "Jwt"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var registryObjects = RegistryContent.AsRegistryObjectDtos();

        var registryContentCount = registryObjects.Count();

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken01")));

        var documentEntryThatDoesntExist = Guid.NewGuid().ToString();

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/R4/fhir/DocumentReference/{documentEntryThatDoesntExist}");

        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, firstResponse.StatusCode);

        var currentRegistry = _registry.ReadRegistry();
        var currentCount = await currentRegistry.CountAsync(TestContext.Current.CancellationToken);

        var expectedCount = registryContentCount;

        _policyRepositoryService.DeleteAllPolicies();
        await NukeRegistryRepository();

        Assert.Equal(expectedCount, currentCount);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine("DeleteDocumentsAndMetadata: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString + "\nUser Access Entry: " + MockStatisticsProcessorService.UserAccessEntryJson);
    }

    [Fact]
    [Trait("Patch", "Patch DocumentReference securityLabel  (Isolated Access Control)")]
    public async Task ProvideBundle_PatchDocumentSecurityLabel_ExportsAtnaLog_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await ProvideBundle_PatchDocumentSecurityLabel_ExportsAtnaLog();
    }

    [Fact]
    [Trait("Patch", "Patch DocumentReference securityLabel")]
    public async Task ProvideBundle_PatchDocumentSecurityLabel_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_" +
            "machine_patchdocumentreference",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments,
            action: "Update",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "Jwt"));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken01")));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);
        var randomDocumentEntry = RegistryContent.PickRandom().DocumentEntry;

        var patchBody = """
        {
          "securityLabel": [
            {
              "coding": [
                {
                  "system": "http://example.org/security",
                  "code": "N",
                  "display": "Normal"
                }
              ]
            }
          ]
        }
        """;

        var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/R4/fhir/DocumentReference/{randomDocumentEntry?.Id}")
        {
            Content = new StringContent(patchBody, Encoding.UTF8, Constants.MimeTypes.FhirJson)
        };

        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var response = await _client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _policyRepositoryService.DeleteAllPolicies();
        await NukeRegistryRepository();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await WaitForAtnaLogToBeExported();
        _output.WriteLine("PatchDocumentSecurityLabel_ExportsAtnaLog: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString + "\nUser Access Entry: " + MockStatisticsProcessorService.UserAccessEntryJson);
    }

    [Fact]
    [Trait("Patch", "Patch DocumentReference securityLabel")]
    public async Task ProvideBundle_PatchDocumentSecurityLabel_TooLongFields_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_" +
            "machine_patchdocumentreference",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments,
            action: "Update",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "Jwt"));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken01")));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);
        var randomDocumentEntry = RegistryContent.PickRandom().DocumentEntry;

        var patchBody = """
        {
          "securityLabel": [
            {
              "coding": [
                {
                  "system": "http://example.org/security",
                  "code": "N",
                  "display": "Lang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøåLang tekst sahfjlksahlkasjhflkjhwquihæøåøæøåøæøå"
                }
              ]
            }
          ]
        }
        """;

        var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/R4/fhir/DocumentReference/{randomDocumentEntry?.Id}")
        {
            Content = new StringContent(patchBody, Encoding.UTF8, Constants.MimeTypes.FhirJson)
        };

        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var response = await _client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _policyRepositoryService.DeleteAllPolicies();
        await NukeRegistryRepository();

        await WaitForAtnaLogToBeExported();
        _output.WriteLine("PatchDocumentSecurityLabel_ExportsAtnaLog: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString + "\nUser Access Entry: " + MockStatisticsProcessorService.UserAccessEntryJson);
    }

    [Fact]
    [Trait("Upload", "Provide Bundle (Isolated Access Control)")]
    public async Task ProvideBundle_WrongValues_ExportsAtnaLog_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await ProvideBundle_WrongValues_ExportsAtnaLog();
    }

    [Fact]
    [Trait("Upload", "Provide Bundle")]
    public async Task ProvideBundle_WrongValues_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        //_policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_" +
            "machine_providebundle",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments,
            action: "Create",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "Jwt"));

        await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle01_WrongValues")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken01")));

        var stringContent = new StringContent(fhirProvideBundle, Encoding.UTF8, Constants.MimeTypes.FhirJson);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/R4/fhir/Bundle");
        httpRequest.Content = stringContent;
        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        var responseContent = await firstResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var fhirparser = new FhirJsonDeserializer();

        var operationOutcome = fhirparser.Deserialize<OperationOutcome>(responseContent);

        _policyRepositoryService.DeleteAllPolicies();
        await NukeRegistryRepository();

        Assert.NotEmpty(operationOutcome.Issue);
        await WaitForAtnaLogToBeExported();

        _output.WriteLine("ProvideBundle_RandomAmount: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString + "\nUser Access Entry: " + MockStatisticsProcessorService.UserAccessEntryJson);
    }

    [Fact]
    [Trait("Upload", "Provide Bundle (Isolated Access Control)")]
    public async Task ProvideBundle_ExportsAtnaLog_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await ProvideBundle_ExportsAtnaLog();
    }

    [Fact]
    [Trait("Upload", "Provide Bundle")]
    public async Task ProvideBundle_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_machine_providebundle",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments,
            action: "Create",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "Jwt"));

        RegistryContent = await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle02_dept_without_reference_in_authors.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken01")));

        var fhirParser = new FhirJsonDeserializer();
        var fhirBundle = fhirParser.DeserializeResource(fhirProvideBundle);

        var provideBundleDocumentUniqueId = fhirBundle is Bundle bundle ? bundle.Entry
            .Select(e => e.Resource)
            .OfType<Binary>()
            .FirstOrDefault().Id
            : null;

        var stringContent = new StringContent(fhirProvideBundle, Encoding.UTF8, Constants.MimeTypes.FhirJson);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/R4/fhir/Bundle")
        {
            Content = stringContent
        };

        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var expectedCount = RegistryContent.Count + 1;

        var firstResponse = await _client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        var responseContent = await firstResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var actualCount = await _registry.ReadRegistry().OfType<DocumentEntryDto>().CountAsync(TestContext.Current.CancellationToken);
        var documentFromProvideBundle = _repository.Read(provideBundleDocumentUniqueId);

        _policyRepositoryService.DeleteAllPolicies();
        await NukeRegistryRepository();

        Assert.NotNull(documentFromProvideBundle);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(expectedCount, actualCount);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine("ProvideBundle_RandomAmount: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString + "\nUser Access Entry: " + MockStatisticsProcessorService.UserAccessEntryJson);
    }

    [Fact]
    [Trait("Upload", "Provide Bundle (virus) (Isolated Access Control)")]
    public async Task ProvideBundle_Virus_ExportsAtnaLog_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await ProvideBundle_Virus_ExportsAtnaLog();
    }

    [Fact]
    [Trait("Upload", "Provide Bundle (virus)")]
    public async Task ProvideBundle_Virus_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_" +
            "machine_providebundle",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments,
            action: "Create",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "Jwt"));

        await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle03_virus.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken01")));

        var stringContent = new StringContent(fhirProvideBundle, Encoding.UTF8, Constants.MimeTypes.FhirJson);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/R4/fhir/Bundle");
        httpRequest.Content = stringContent;
        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, firstResponse.StatusCode);

        var responseContent = await firstResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        _policyRepositoryService.DeleteAllPolicies();
        await NukeRegistryRepository();

        var fhirparser = new FhirJsonDeserializer();

        var operationOutcome = fhirparser.Deserialize<OperationOutcome>(responseContent);

        Assert.NotEmpty(operationOutcome.Issue);

        await WaitForAtnaLogToBeExported();

        _output.WriteLine("ProvideBundle_RandomAmount: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString + "\nUser Access Entry: " + MockStatisticsProcessorService.UserAccessEntryJson);
    }

    [Fact]
    [Trait("Upload", "Validate Bundle (Isolated Access Control)")]
    public async Task ProvideBundle_Validate_ExportsAtnaLog_IAC()
    {
        _policyRepositoryService.DeleteAllPolicies();
        await ProvideBundle_Validate_ExportsAtnaLog();
    }

    [Fact]
    [Trait("Upload", "Validate Bundle")]
    public async Task ProvideBundle_Validate_ExportsAtnaLog()
    {
        await NukeRegistryRepository();

        _atnaLogExportedChecker.AtnaLogExported = false;
        _atnaLogExportedChecker.AtnaMessageString = null;

        //_policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_machine_validatebundle",
            attributeId: Constants.Saml.Attribute.EhelseScope,
            codeValue: Constants.Scopes.FhirMobileAccessToHealthDocuments.ScopeCreateDocuments,
            action: "Execute",
            noCode: true);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "Fhir"));
        var jsonWebTokenfiles = Directory.GetFiles(Path.Combine(testDataPath, "Jwt"));

        await EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var fhirProvideBundle = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("ProvideBundle03.json")));
        var jsonWebToken = File.ReadAllText(jsonWebTokenfiles.FirstOrDefault(f => f.Contains("JsonWebToken01")));

        var stringContent = new StringContent(fhirProvideBundle, Encoding.UTF8, Constants.MimeTypes.FhirJson);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/R4/fhir/Bundle/$validate");
        httpRequest.Content = stringContent;
        httpRequest.Headers.Add("Authorization", jsonWebToken);

        var firstResponse = await _client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        var responseContent = await firstResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var fhirparser = new FhirJsonDeserializer();

        var operationOutcome = fhirparser.Deserialize<OperationOutcome>(responseContent);

        _policyRepositoryService.DeleteAllPolicies();
        await NukeRegistryRepository();

        Assert.NotEmpty(operationOutcome.Issue);

        await WaitForAtnaLogToBeExported();
        _output.WriteLine("ProvideBundle_RandomAmount: ATNA log exported: " + _atnaLogExportedChecker.AtnaMessageString + "\nUser Access Entry: " + MockStatisticsProcessorService.UserAccessEntryJson);
    }
}
#pragma warning restore CS8604, CS8602 // Possible null reference argument.
