# Clincal Document Architecture (CDA) To Registry Metadata
**PJD.XcaDocumentSource** features a full, self contained implementation of the CDA (Clinical Document Architecture) specification, version 2.0.1. Instances of the ClinicalDocument-class and its subtypes are serializable to a valid CDA document using a regular `XmlSerializer` available with .NET.  
[CDA Core v2.0 - build.fhir.org ↗](https://build.fhir.org/ig/HL7/CDA-core-2.0/StructureDefinition-ClinicalDocument.html)

The `CdaTransformerService` contains a method `TransformRegistryObjectsToClinicalDocument`, which converts a `DocumentEntryDto`, `SubmissionSetDto` and `DocumentDto` instances to a CDA Level 1 document, where the `<NonXmlBody>` contains the original document.

## On-The-Fly CDA Document generation from RegistryObjectDtos
**PJD.XcaDocumentSource** supports generating a **CDA document** dynamically from Registry Object DTOs (`DocumentEntryDto` and `SubmissionSetDto`) using the static class `CdaTransformerService`. This is useful when wanting to return more detailed document response when fetching a document from the repository.  

The `appsettings.json`-parameter `WrapRetrievedDocumentInCda` toggles this functionality, making any document request (either via REST or SOAP), which does not contain structured XML, return the document data wrapped in a XML-serialized CDA document generated from the document metadata corresponding to the document being retrieved.
