using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.DataManipulators;
using XcaXds.Tests.Helpers;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests;

public partial class IntegrationTests_XcaXdsRegistryRepository_CRUD
{
    [Fact]
    [Trait("Upload", "Modify Registry/Repository")]
    public async Task PNR_UploadDocuments_RandomAmount()
    {
        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Create");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        Assert.Equal(RegistryItemCount, _registry.DocumentRegistry.OfType<DocumentEntryDto>().Count());

        var metadata = TestHelpers.GenerateRegistryMetadata(RegistryItemCount, PatientIdentifier.IdNumber, true).PickRandom(Random.Shared.Next(1, RegistryItemCount)).ToArray();
        var registryObjects = metadata.SelectMany(dedto => RegistryMetadataTransformerService.TransformDocumentReferenceDtoToRegistryObjects(dedto)).ToArray();
        var documents = metadata.Select(dedto => new DocumentType { Id = dedto.Document.DocumentId, Value = dedto.Document.Data }).ToArray();

        var iti41SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti41-request.xml"))));

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. registryObjects];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = documents;

        var itemsToUploadCount = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterPnR = RegistryItemCount + itemsToUploadCount;

        var iti41RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti41SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti41RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(0, firstResponseSoap?.Body?.RegistryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);

        Assert.Equal(expectedCountAfterPnR, _registry.DocumentRegistry.OfType<DocumentEntryDto>().Count());
        Assert.Equal(expectedCountAfterPnR, _repository.DocumentRepository.Count);
       
        Thread.Sleep(500); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Registry count before test run: {RegistryItemCount}\nUploaded: {itemsToUploadCount} entries.\nRegistry count: {_registry.DocumentRegistry.Count}\nRepository count: {_repository.DocumentRepository.Count}");
    }


    [Fact]
    [Trait("Update", "Modify Registry/Repository")]
    public async Task PNR_UpdateRegistryRepository_Deprecate_RandomAmount()
    {
        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Update");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        var registryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var amountOfItemsToReplace = Random.Shared.Next(1, RegistryItemCount);

        var randomDocumentEntriesToDeprecate = registryContent.PickRandom(amountOfItemsToReplace).ToArray();
        var newDocumentEntries = TestHelpers.GenerateRegistryMetadata(amountOfItemsToReplace, PatientIdentifier.IdNumber, true);

        var assocDtos = newDocumentEntries
            .Zip(randomDocumentEntriesToDeprecate, (nuDocEnt, rndDocEntToDprct) => new AssociationDto
            {
                SourceObject = nuDocEnt.DocumentEntry?.Id,
                TargetObject = rndDocEntToDprct.DocumentEntry?.Id,
                AssociationType = Constants.Xds.AssociationType.Replace
            }).ToArray();


        var assocIds = assocDtos.Select(ass => ass.TargetObject).ToArray();
        var docEntIds = randomDocumentEntriesToDeprecate.Select(ass => ass.DocumentEntry?.Id).ToArray();

        var targets = assocDtos.Select(a => a.TargetObject).ToHashSet();

        Assert.All(randomDocumentEntriesToDeprecate, d => Assert.Contains(d.DocumentEntry?.Id, targets));


        var submitObjectsUpdate = RegistryMetadataTransformerService.
            TransformRegistryObjectDtosToRegistryObjects([.. assocDtos, .. newDocumentEntries.Select(dto => dto.DocumentEntry), .. newDocumentEntries.Select(dto => dto.Association), .. newDocumentEntries.Select(dto => dto.SubmissionSet)]);
        var documentUpdate = newDocumentEntries.Select(nde => new DocumentType { Id = nde.Document.DocumentId, Value = nde.Document.Data }).ToArray();

        var iti41SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti41-request.xml"))));

        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. submitObjectsUpdate];
        iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.Document = [.. documentUpdate];

        var deprecateAssociations = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList
            .OfType<AssociationType>()
            .Where(robj => docEntIds.Any(id => id == robj.TargetObject)).ToArray();

        var itemsToUploadCount = iti41SoapRequestObject.Body.ProvideAndRegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterPnrUpdate = RegistryItemCount + itemsToUploadCount;

        var iti41RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti41SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti41RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(responseContent);

        var deprecatedDocuments = _registry.DocumentRegistry.OfType<DocumentEntryDto>().Where(ro => ro.AvailabilityStatus == Constants.Xds.StatusValues.Deprecated).ToArray();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(expectedCountAfterPnrUpdate, _registry.DocumentRegistry.OfType<DocumentEntryDto>().Count());
        Assert.Equal(expectedCountAfterPnrUpdate, _repository.DocumentRepository.Count);
        Assert.Equal(randomDocumentEntriesToDeprecate.Length, deprecatedDocuments.Count());
        
        Thread.Sleep(500); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Registry count before test run: {RegistryItemCount}\nUpdated: {itemsToUploadCount} entries.\nRegistry count: {_registry.DocumentRegistry.Count}\nRepository count: {_repository.DocumentRepository.Count}");
    }

    [Fact]
    [Trait("Upload", "Add to Registry")]
    public async Task RDS_UploadRegistry_AddMetadata()
    {
        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_CrossGatewayQuery",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Create");

        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var testDataFiles = Directory.GetFiles(testDataPath);

        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

        EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var metadata = TestHelpers.GenerateRegistryMetadata(RegistryItemCount, PatientIdentifier.IdNumber, true).PickRandom(Random.Shared.Next(1, RegistryItemCount)).ToArray();
        var registryObjects = metadata.SelectMany(dedto => RegistryMetadataTransformerService.TransformDocumentReferenceDtoToRegistryObjects(dedto)).ToArray();
        var documents = metadata.Select(dedto => new DocumentType { Id = dedto.Document.DocumentId, Value = dedto.Document.Data }).ToArray();


        var iti42SoapRequestObject = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti42-request.xml"))));

        iti42SoapRequestObject.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList = [.. registryObjects];

        var itemsToUploadCount = iti42SoapRequestObject.Body.RegisterDocumentSetRequest?.SubmitObjectsRequest.RegistryObjectList.OfType<ExtrinsicObjectType>().Count();
        var expectedCountAfterRds = RegistryItemCount + itemsToUploadCount;

        var iti42RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(sxmls.SerializeSoapMessageToXmlString(iti42SoapRequestObject).Content);
        var firstResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(iti42RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(firstResponse.Content.ReadAsStream());

        var responseContent = await firstResponse.Content.ReadAsStringAsync();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(expectedCountAfterRds, _registry.DocumentRegistry.OfType<DocumentEntryDto>().Count());
        Assert.Equal(RegistryItemCount, _repository.DocumentRepository.Count);
      
        Thread.Sleep(500); // Wait for the log to be exported, since it's done asynchronously after the response is sent
        Assert.True(_atnaLogExportedChecker.AtnaLogExported);

        _output.WriteLine($"Registry count before test run: {RegistryItemCount}\nUploaded: {itemsToUploadCount} entries.\nRegistry count: {_registry.DocumentRegistry.Count}\nRepository count: {_repository.DocumentRepository.Count}");
    }

    [Fact]
    [Trait("Delete", "Modify Registry")]
    public async Task RMD_RemoveDocumentsAndMetadata_RandomAmount()
    {
        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_RemoveDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Delete");

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_QueryDocumentList",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_QueryDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocuments");

        RegistryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var iti18AdhocQuery = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti18-request.xml"))));
        var iti43RetrieveDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti43-request.xml"))));
        var iti62DeleteObjectsRequest = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti62-request.xml"))));
        var iti86DeleteDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti86-request.xml"))));

        var amountOfItemsToReplace = Random.Shared.Next(1, RegistryItemCount);

        // Step -1: Pick random DocumentEntries to remove
        var documentEntryToRemove = RegistryContent.PickRandom(amountOfItemsToReplace).Select(rc => rc.DocumentEntry).ToArray();

        // Step 0: Check if Registry and Repository content is present
        Assert.Equal(RegistryItemCount, _registry.DocumentRegistry.OfType<DocumentEntryDto>().Count());

        // Step 1: Get the unique id for the DocumentEntry in the Registry...
        var iti18RmdRequest = new SoapEnvelope();
        iti18RmdRequest = iti18AdhocQuery; // Reusing this variable saves 0,000000124805 µg of CO2

        iti18RmdRequest.Body.AdhocQueryRequest?.AdhocQuery.Slot = [new SlotType()
        {
            Name = Constants.Xds.QueryParameters.Associations.Uuid,
            ValueList = new() { Value = [.. documentEntryToRemove.Select(docent => docent?.Id)] }
        }];

        iti18RmdRequest.Body.AdhocQueryRequest?.AdhocQuery.Id = Constants.Xds.StoredQueries.GetAssociations;

        var iti18RmdRequestSoapString = sxmls.SerializeSoapMessageToXmlString(iti18RmdRequest).Content;
        var iti18RmdRequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti18RmdRequestSoapString);

        var iti18RmdRequestResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(iti18RmdRequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));
        Assert.Equal(System.Net.HttpStatusCode.OK, iti18RmdRequestResponse.StatusCode);

        var iti18RmdRequestResponseContent = await iti18RmdRequestResponse.Content.ReadAsStringAsync();

        var iti18RmdResponseSoapObject = sxmls.DeserializeXmlString<SoapEnvelope>(iti18RmdRequestResponseContent);


        Assert.Empty(iti18RmdResponseSoapObject?.Body?.RegistryResponse?.RegistryErrorList?.RegistryError ?? []);

        var documentEntriesToRemove = new HashSet<string>(documentEntryToRemove?.Select(de => de.Id));
        var amountOfEntitiesToRemove = documentEntriesToRemove.Count;
        var rmdAssociation = iti18RmdResponseSoapObject?.Body.AdhocQueryResponse?.RegistryObjectList.OfType<AssociationType>().Where(assoc => documentEntriesToRemove.Contains(assoc.TargetObject)).ToArray();


        // Step 2: Use the identifiers in the Association to remove the metadata from the Registry...
        Assert.NotNull(rmdAssociation);
        iti62DeleteObjectsRequest.Body.RemoveObjectsRequest?.ObjectRefList?.ObjectRef = rmdAssociation.SelectMany(assoc => new IdentifiableType[]
        {
            new ObjectRefType() { Id = assoc.Id },
            new ObjectRefType() { Id = assoc.SourceObject },
            new ObjectRefType() { Id = assoc.TargetObject },
        }).ToArray();

        var iti62RequestString = sxmls.SerializeSoapMessageToXmlString(iti62DeleteObjectsRequest).Content;

        var iti62RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti62RequestString);

        var iti62RequestResponse = await _client.PostAsync("/Registry/services/RegistryService", new StringContent(iti62RequestXmlDoc.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));
        Assert.Equal(System.Net.HttpStatusCode.OK, iti62RequestResponse.StatusCode);

        var iti62ResponseContent = await iti62RequestResponse.Content.ReadAsStringAsync();

        var iti62ResponseSoapObject = sxmls.DeserializeXmlString<SoapEnvelope>(iti62ResponseContent);
        Assert.Equal(Constants.Xds.ResponseStatusTypes.Success, iti62ResponseSoapObject.Body.RegistryResponse?.Status);

        Assert.Equal(RegistryItemCount - documentEntriesToRemove.Count, _registry.DocumentRegistry.OfType<DocumentEntryDto>().Count());


        // Step 3: Use the DocumentUniqueId in the DocumentEntry to remove the Document
        iti86DeleteDocumentSet.Body.RemoveDocumentsRequest?.DocumentRequest = documentEntryToRemove.SelectMany(docEnt => new[]{

            new DocumentRequestType()
            {
                DocumentUniqueId = docEnt?.UniqueId,
                HomeCommunityId = docEnt?.HomeCommunityId,
                RepositoryUniqueId = docEnt?.RepositoryUniqueId
            }
         }).ToArray();

        iti86DeleteDocumentSet.SetAction(Constants.Xds.OperationContract.Iti86Action);

        var iti86RequestString = sxmls.SerializeSoapMessageToXmlString(iti86DeleteDocumentSet).Content;

        var iti86RequestXmlDoc = GetSoapEnvelopeWithKjernejournalSamlToken(iti86RequestString);
        var iti86RequestResponse = await _client.PostAsync("/Repository/services/RepositoryService", new StringContent(iti86RequestXmlDoc?.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

        var iti86RequestResponseContent = await iti86RequestResponse.Content.ReadAsStringAsync();

        var iti86ResponseSoapObject = sxmls.DeserializeXmlString<SoapEnvelope>(iti86RequestResponseContent);

        Assert.Equal(System.Net.HttpStatusCode.OK, iti86RequestResponse.StatusCode);
        Assert.Equal(Constants.Xds.ResponseStatusTypes.Success, iti62ResponseSoapObject.Body.RegistryResponse?.Status);
        Assert.Equal(RegistryItemCount - documentEntriesToRemove.Count, _repository.DocumentRepository.Count);

        _output.WriteLine($"Registry count before test run: {RegistryItemCount}\nRemoved: {documentEntriesToRemove.Count} entries.\nRegistry count: {_registry.DocumentRegistry.Count}\nRepository count: {_repository.DocumentRepository.Count}");
    }


    [Fact]
    [Trait("Read", "Read Registry/Repository")]
    public async Task ALL_PutWrongRequestsForActions()
    {
        _policyRepositoryService.DeleteAllPolicies();
        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_RemoveDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "Delete");

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_QueryDocumentList",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocumentList");

        TestHelpers.AddAccessControlPolicyForIntegrationTest(
            _policyRepositoryService,
            policyName: "IT_QueryDocuments",
            attributeId: Constants.Saml.Attribute.Role,
            codeValue: "LE;SP;PS",
            codeSystemValue: "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060",
            action: "ReadDocuments");

        RegistryContent = EnsureRegistryAndRepositoryHasContent(registryObjectsCount: RegistryItemCount, patientIdentifier: PatientIdentifier.IdNumber);

        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
        var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));
        var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

        var iti18AdhocQuery = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti18-request.xml"))));
        var iti43RetrieveDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti43-request.xml"))));
        var iti62DeleteObjects = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti62-request.xml"))));
        var iti86DeleteDocumentSet = sxmls.DeserializeXmlString<SoapEnvelope>(File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_iti86-request.xml"))));

        iti18AdhocQuery.Body.RetrieveDocumentSetRequest = iti43RetrieveDocumentSet.Body.RetrieveDocumentSetRequest;
        iti43RetrieveDocumentSet.Body.AdhocQueryRequest = iti18AdhocQuery.Body.AdhocQueryRequest;
        iti18AdhocQuery.Body.AdhocQueryRequest = null;
        iti43RetrieveDocumentSet.Body.RetrieveDocumentSetRequest = null;

        iti62DeleteObjects.Body.RemoveDocumentsRequest = iti86DeleteDocumentSet.Body.RemoveDocumentsRequest;
        iti86DeleteDocumentSet.Body.RemoveObjectsRequest = iti62DeleteObjects.Body.RemoveObjectsRequest;
        iti86DeleteDocumentSet.Body.RemoveDocumentsRequest = null;
        iti62DeleteObjects.Body.RemoveObjectsRequest = null;

        var requests = new List<Dude>
        {
            new Dude { Request = iti18AdhocQuery, Endpoint = "/Registry/services/RegistryService"},
            new Dude { Request = iti62DeleteObjects, Endpoint = "/Registry/services/RegistryService"},
            new Dude { Request = iti86DeleteDocumentSet, Endpoint = "/Repository/services/RepositoryService"},
            new Dude { Request = iti43RetrieveDocumentSet, Endpoint = "/Repository/services/RepositoryService"}
        };

        foreach (var request in requests)
        {
            var soapRequestString = sxmls.SerializeSoapMessageToXmlString(request.Request).Content;
            var soapRequestResponse = await _client.PostAsync(request.Endpoint,
                new StringContent(GetSoapEnvelopeWithKjernejournalSamlToken(soapRequestString)?.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

            var responseEnvelope = sxmls.DeserializeXmlString<SoapEnvelope>(await soapRequestResponse.Content.ReadAsStringAsync());
            Assert.NotNull(responseEnvelope.Body.Fault);
        }
    }
}

internal class Dude
{
    public SoapEnvelope Request { get; set; }
    public string Endpoint { get; set; }
}