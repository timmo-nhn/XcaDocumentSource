# XcaDocumentSource – Open Source XCA Responding Gateway Framework

## Preface: The vision of XcaDocumentSource
> ⚠ **Important Note!** <br>**XcaDocumentSource** is provided as an open-source reference for the implementer to extend or customize its interfaces to align with the requirements of their existing Electronic Patient Record (EPR) systems.  
The solution is **not** a substitute for an EPR system nor a full EPR storage solution; it acts as a translating framework between SOAP messages from NHN's XCA and the implementers **existing** EPR-system.<br><br></span>
**Norsk helsenett (NHN) does not assume responsibility for the integrity, availability, or confidentiality of patient data handled through deployments based on XcaDocumentSource. Use of XcaDocumentSource is at the implementer's own risk, and any integration between XcaDocumentSource and live Electronic Patient Record (EPR) systems must be thoroughly tested and validated within the implementer’s own governance and compliance frameworks.**

**XcaDocumentSource** allows healthcare providers to expose their internal, document storage solution as an **XDS-compliant Registry** and **Repository** interface.  

## Concerns covered by XcaDocumentSource
* **Profiles:** Implements IHE XCA profile for cross-community access
* **Storage:** Provides an XDS.b-compatible registry and repository layer backed by customizable storage adapters
* **National integration** Supports integration with Norsk Helsenett’s XCA Initiating Gateway
* **Access control** Implements Policy Enforcement point, transforming SAML and JWT security attributes (Based on Norwegian standards) to Attribute-Based Access Control (ABAC), allowing for fine grained access control
* **Access control** Implements programmable business-logic for partial or full obfuscation of patient data based on security attributes


**XcaDocumentSource provides basic document registry and repository.** However, the recommended option is for implementers to connect their own storage infrastructure to **XcaDocumentSource** - whether proprietary, legacy, or standards-based - by implementing custom translation logic between document storage metadata and the simpler, internal data-structures of **XcaDocumentSource**.
```mermaid
%%{init: {'theme':'dark'}}%%
flowchart

nhnxca[NHN XCA Initiating Gateway]
subgraph "Actor"
  xcads[XcaDocumentSource<br>XCA Responding Gateway]
  epr[Electronic Patient Records]
end

nhnxca--"ITI-38/-ITI39"-->xcads--"Custom Adapter Interface (Written by the implementer)"-->epr
```

```
 [External XCA Initiating Gateway]
             ↓ ITI-38/39
  [XcaDocumentSource Translation Layer, simple data structures]
             ↓ Custom Adapter Interface (Written by the implementer)
     [Existing Document Storage Backend]
```

It aims to accelerate prototyping and integration with NHN, without requiring full knowledge of **IHE profiles** and **ebXML RegRep** specifications up-front; the solution handles the complexities of SOAP and XML, essentially "doing the plumbing work". This allows the end user to focus on their domain-specific implementations needs, like shaping business logic and handling access control.

Architecturally, **XcaDocumentSource** acts as a translation gateway that sits between an external XCA Initiating Gateway (e.g., NHN) and an organization’s internal EPR system, mapping IHE protocols to local API formats.


## Introduction/Getting started
In the healthcare industry, hospitals, clinics, and municipalities use a variety of Electronic Health Record (EHR) systems, often from different vendors. These systems were rarely designed to communicate with each other, leading to:
* Data silos where patient records are confined in local systems
* Manual, error-prone processes for sharing health information
* Delayed treatment due to lack of access to complete medical histories  

This lack of interoperability results in documents having to be shared via manual routines, such as fax-machines, sent as letters via taxi or calling the hospitals a patient has previously visited. This results in fragmented care, increased administrative burden, and risk to patient safety.

**XcaDocumentSource** is a component which acts as a middleware-system between healthcare provider system and Norsk helsenett's XCA-gateway infrastructure. This will allow actors such as hospitals and municipalities to share patient health records across organizational and technical boundaries by handling the SOAP-implementation, and allowing the implementer to easily modify the solution to for integrating between **XcaDocumentSource** and their own systems.  
The implementation is based around the IHE integration profiles based on **XDS** and **XCA** provided in Volumes 1 through 3 of the [IHE IT Infrastructure Technical Framework - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/index.html) in a national context, aswell as **HL7**:

* XDS.b (Cross-Enterprise Document Sharing) – for registering and retrieving clinical documents
* XCA (Cross-Community Access) Responding Gateway – for querying and retrieving documents from NHN's XCA  
* HL7 (Health Level 7) version 2 - for some queries related to patient identity
* PEP (Policy Enforcement Point) - for access control (ABAC-implementation)

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

  subgraph "Document storage solution"
    docstore[(Document<br>store)]
  end
end

nhnxca <--SOAP-Request--> pep


pep <--> resgw
resgw <--> doclist

resgw <--> document

doclist <--> docstore
document <--> docstore
```
*Solution architecture overview*
 

## Solution Documentation

> **⚠️ Note!**  
The **Dockerfile** supports overriding the upstream container registry via `UPSTREAM_REGISTRY` (default: `mcr.microsoft.com`).
In GitLab CI this is set through build args so base images are pulled via `CI_DEPENDENCY_PROXY_GROUP_IMAGE_PREFIX`.

### [🌐 Document Sharing overiew - Actors and Components](/Docs/Overview.md)
Describes the high-level principles of document sharing, and the components involved in the process.

### [📝 Use case scenarios - XcaDocumentSource](/Docs/UseCases.md)
Scenarios of XcaDocumentSource in a source system.

### [⚙️ Solution Overview/Technical implementation details](/Docs/TechnicalImplementation.md)
How **XcaDocumentSource** solution is structured, and how it can be implemented in a source system, taking in account existing document registries/repositories, and PAP/PDP/PR systems.

### [📜 Custom Registry Format](/Docs/RegistryDto.md)
Describes the custom Registry format which is used to store document entries.

### [🧾 (ebRIM) Metadata, XDS and SOAP-message formats and standards](/Docs/XdsAndSoap.md)
Covering the SOAP-message format and the XDS profile and transactions involved in uploading, downloading and sharing documents and document metadata.

## Interacting With the Document Registry/Repository

### [📨 SOAP-endpoints/ITI-messages (SOAP-transactions) and Multipart](/Docs/XdsTransactions.md)
Overviews the ITI-messages supported by **XcaDocumentSource** and their endpoints, as well as examples. Also describes MTOM/multipart request handling in **XcaDocumentSource**

### [👩‍💻 REST-endpoints (CRUD-transactions)](/Docs/RestTransactions.md)
Describes the REST-endpoints of the solution, allowing for quick and easy CRUD-operations on the Document Registry and Repository.

### [🔥 FHIR/MHD-endpoints](/Docs/MhdTransactions.md)
Describes the RESTful FHIR and MHD-endpoints (Mobile access to Health Documents) of the solution, accesing the registry and repository in a standards-based format (XDS on FHIR).

## Governing the solution

### [💠 OIDs (Object Identifiers)](/Docs/Oids.md)
OIDs are important in identifying the different components in the systems involved in the document sharing exchange. Effective governing and managing of OIDs are crucial in efficiently identifying systems.

### [🏛 Authorization and Access Control](/Docs/AccessControl.md)
Describes the methods and interfaces for access control.

## Other Functionality

### [📄 CDA To Registry Metadata](/Docs/CdaRegistryMetadata.md)
Describes the functionality for converting a CDA document to an ITI-41 message/Registry Metadata and vice-versa.

## Coding Conventions

The following coding conventions are used in XcaDocumentSource. They are not enforced onto the implementer, but described here for informational purposes:
- Functions are used extensively to break up code and separate concerns. Functions should rarely be more than 100 lines.
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
Used to describe an implementation which is notable or specific to **XcaDocumentSource**  
**Example:**
>**🔶 Implementation Note x** <br> Example text

### National Extension Quote  
Used to describe something specific to the **Norwegian** implementation of **IHE XDS/XCA**  
**Example:**
>**🚩 National Extension x** <br> Example text

### Question/Answer
Questions that the reader might ask themselves. Usually in a lighthearted tone.  
**Example:**  
> ⁉️ **Why is the S in SOAP a lie?**<br> The answer might shock you!
