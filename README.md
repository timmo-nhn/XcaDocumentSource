# PJD.XcaDocumentSource - XCA Responding Gateway and integrated Document Registry and Repository  

## Introduction/Getting started
In the healthcare industry, hospitals, clinics, and municipalities use a variety of Electronic Health Record (EHR) systems, often from different vendors. These systems were rarely designed to communicate with each other, leading to:
* Data silos where patient records are confined in local systems
* Manual, error-prone processes for sharing health information
* Delayed treatment due to lack of access to complete medical histories  

This lack of interoperability results in documents having to be shared via manual routines, such as fax-machines, sending as letters via taxi or calling the hospitals a patient has previously visited. This results in fragmented care, increased administrative burden, and risk to patient safety.

**XcaDocumentSource** is a component which acts as a middleware-system between healthcare provider system and Norsk helsenett's XCA-gateway infrastructure. This will allow actors such as hospitals and municipalities to share patient health records across organizational and technical boundaries by handling the SOAP-implementation, and allowing the implementer to easily modify the solution to for integrating between **XcaDocumentSource** and their own systems.  
The implementation is based around the IHE integration profiles based on **XDS** and **XCA** provided in Volumes 1 through 3 of the [IHE IT Infrastructure Technical Framework - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/index.html) in a national context, aswell as **HL7** and **XACML**:
* XDS.b (Cross-Enterprise Document Sharing) – for registering and retrieving clinical documents
* XCA (Cross-Community Access) Responding Gateway – for querying and retrieving documents from NHN's XCA  
* HL7 (Health Level 7) version 2 - for some queries related to patient identity
* PEP (Policy Enforcement Point) - for access control

### Technical overview
```mermaid
%%{init: {'theme':'dark'}}%%

flowchart LR
subgraph "Norsk Helsenett"
  nhnxca[NHN XCA<br>Initiating Gateway]
end

subgraph "Actor systems"
  subgraph "XcaDocumentSource"
    resgw[XCA<br>Responding Gateway]

    pep[<br><br>PEP<br><br><br>]

    doclist([Get Document list])
    document([Get Document])
  end

  pdp[PDP]

  subgraph "Document storage solution"
    docstore[(Document<br>store)]
  end
end

nhnxca <--SOAP-Request--> resgw

pep<-->pdp

resgw <--> pep
pep <--> doclist

pep <--> document

doclist <--> docstore
document <--> docstore
```
*Solution architecture overview*


## Solution Documentation

### [🌐 Document Sharing overiew - Actors and Components](/Docs/Overview.md)
Describes the high-level principles of document sharing, and the components involved in the process.

### [⚙️ Technical implementation details](/Docs/TechnicalImplementation.md)
How **PJD.XcaDocumentSource** solution is structured, and how it can be implemented in a source system, taking in account existing document registries/repositories, and PAP/PDP/PR systems.

### [🧾 Custom Registry Format](/Docs/RegistryDto.md)
Describes the custom Registry format which is used to store document entries.

### [🧾 (ebRIM) Metadata, XDS and SOAP-message formats and standards](/Docs/XdsAndSoap.md)
Covering the SOAP-message format and the XDS profile and transactions involved in uploading, downloading and sharing documents and document metadata.

### [📨 ITI-messages](/Docs/XdsTransactions.md)
Overviews the ITI-messages supported by **XcaDocumentSource** and their endpoints, as well as examples.

### [🏥 HL7 Messaging and Patient identity](/Docs/Hl7MessagingPatientIds.md)
Describes the lightweight implementation of HL7 messaging, allowing for Patient Demographics and Identity lookups and cross-referencing

### [💠 OIDs (Object Identifiers)](/Docs/Oids.md)
OIDs are important in identifying the different components in the systems involved in the document sharing exchange. Effective governing and managing of OIDs are crucial in efficiently identifying systems.

## Other Functionality

### [🖥️ XDS Admin Front-End](/Docs/XdsAdminFrontEnd.md)
Documentation of the Admin-GUI which also serves as a practical tool for interacting with the document registry and repository

### [📄 CDA To Registry Metadata](/Docs/CdaRegistryMetadata.md)
Describes the functionality for converting a CDA document to an ITI-41 message/Registry Metadata and vice-versa.

## Use case Scenarios for XcaDocumentSource

### 1. Municipality Sharing Test Results with National Health Network

#### Scenario: 
A municipality’s health department wants to publish lab test results (e.g., COVID-19, bloodwork) from its internal system to the national XCA infrastructure, so they’re available to hospitals and general practitioners (GPs).

#### Use Case Flow:
* The municipality pushes documents into a local repository or document registry.
* The municipality adapts XcaDocumentSource to talk to their local registry or repository. 
* GPs can search and retrieve these documents via the national XCA gateway.

#### Benefits:
Eliminates siloed data. Test results are accessible to any authorized healthcare professional in the country.

### 2. Hospital Sharing Discharge Summaries with GPs and Home Care Providers

#### Scenario:
A hospital wants to make discharge notes and care plans available to the patient’s general practitioner and municipal home care team.

#### Use Case Flow:
* After discharge, the hospital’s EPR system sends a CDA discharge summary.
* XcaDocumentSource registers it with the national XCA infrastructure.
* GPs and municipal care teams query and retrieve the document seamlessly.

#### Benefits:
Ensures continuity of care and informed follow-up treatment.

### 3. Private Specialist Making Consult Notes Available to Hospitals

#### Scenario:
A private dermatologist or cardiologist needs to ensure that their consult findings are accessible to the referring hospital or emergency department.

#### Use Case Flow:
* Specialist uploads or sends consult note (CDA, PDF) to the integration system.
* Integration with XcaDocumentSource makes the document available with appropriate access rights.
* Hospitals query using the national gateway and retrieve the documents.

#### Benefit:
Reduces duplicated effort, supports informed emergency care.

## Coding Conventions

The following coding conventions are used in XcaDocumentSource. They are not enforced onto the implementer, but described here for informational purposes:
- Functions are used extensively to break up code and separate concerns. No function should rarely be more than 100 lines.
  - Functions should have a single purpose (SRP)
- Service-classes encapsulate functionality related to a single "purpose" or function (interact with registry/repository, transform objects from X to Y format etc.)
- Naming:
  - `PascalCase` for classes, methods, and public properties.
  - `camelCase` for local variables and private fields.
- File structure: One class per file.
- Indentation: 4 spaces (no tabs).
- Uses `async/await` over `Task.ContinueWith`, no `.Result` or `.Wait()`.
- Avoid abbreviations in names (e.g., `userProfile`, not `usrProf`, can be abbreviated for long variable names).

### Separation of concerns
The code is divided into clear layers (API/controller, Service, Wrapper, Domain(classes))
```
/Controllers
  - RegistryController.cs
/Services
  - RegistryService.cs
/Repositories
  - RegistryWrapper.cs
```

## Semantics Used  
This section defines how different elements are formatted and referenced within the documentation.

### External hyperlinks
External links are suffixed by an arrow pointing up to the right (↗), signifying that the link leads to a website not affiliated or related to Norsk helsenett. Links will follow this format:  `<title> - <domain> ↗`
**Example:**  
[External link - example.com ↗](https://www.example.com) 

### XML-tags  
Used when referencing something thats part of an XML SOAP-message  
**Example:**   
`<xml-tag>`


### Normal Quotes  
Used as an addendum for a section of text  
**Example:**  
>Quote

### Alert Quote
When there's something that should be paid extra attention to, or is important to know  
**Example:**
> **⚠️ Alert x** <br> Example text

### Implementation Quotes  
Used to describe an implementation which is notable or specific to **PJD.XcaDocumentSource**  
**Example:**
>**🔶 Implementation Note x** <br> Example text

### National Extension Quote  
Used to describe something specific to the **Norwegian** implementation of **IHE XDS/XCA**  
**Example:**
>**🚩 National Extension x** <br> Example text

