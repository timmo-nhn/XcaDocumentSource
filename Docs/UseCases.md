# Use case Scenarios for PJD.XcaDocumentSource

## Use cases

### 1. Municipality Sharing Test Results with National Health Network

#### Scenario: 
A municipality’s health department wants to publish lab test results (e.g., COVID-19, bloodwork) from its internal system to the national XCA infrastructure, so they’re available to hospitals and general practitioners (GPs).

#### Use Case Flow:
* The municipality pushes documents into a local repository or document registry.
* The municipality adapts PJD.XcaDocumentSource to talk to their local registry or repository. 
* GPs can search and retrieve these documents via the national XCA gateway.

#### Benefits:
Eliminates siloed data. Test results are accessible to any authorized healthcare professional in the country.

```mermaid
%%{init: {'theme':'dark'}}%%
flowchart LR

nhnxca[NHN<br>Initiating Gateway]

subgraph "System 1"
    subgraph "EPR Systems/platform"
        xcads[PJD.XcaDocumentSource]
        regrep[(Document/<br>Document Metadata)]
    end
end

nhnxca<--Queries-->xcads<--Retrieves-->regrep
```
*Municipality sharing documents via XcaDocumentSource*

### 2. Hospital Sharing copy of all documents and metadata to XcaDocumentSource

#### Scenario:
A hospital wants to share documents using XcaDocumentSource, but their storage solution does not allow for easy transformation and fetching of data.  
**Solution:** All documents uploaded to the EPR's storage solution is also uploaded to XcaDocumentSource's storage solution

#### Use Case Flow:
* After discharge, the hospital’s EPR system sends a CDA discharge summary.
* The EPR system is connected both to its existing storage solution, but also **PJD.XcaDocumentSource**
* This allows their existing document storage solution to stay unchanged, while a copy of all the data in the "internal" repository is forwarded to **PJD.XcaDocumentSource**

#### Benefits:
Ensures continuity of care and interoperability without migrating storage solutions.

```mermaid
%%{init: {'theme':'dark'}}%%
flowchart LR

nhnxca[NHN<br>Initiating Gateway]

subgraph "System 1"
    subgraph "XcaDocumentSource"
        xcads[API Endpoints]
        regrep1[(Copy of<br>Document/<br>Document Metadata)]
    end
        epr[EPR-system]
        regrep2[(Document/<br>Document Metadata)]
end

regrep1<--Stores documents-->epr
regrep2<--Stores documents-->epr

nhnxca<--Queries-->xcads<--Retrieves-->regrep1
```
*Hospital sharing copy of stored documents via XcaDocumentSource*