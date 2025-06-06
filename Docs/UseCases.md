## Use case Scenarios for PJD.XcaDocumentSource

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
%%{init: {'theme':'dark'}}
flowchart LR

nhnxca[NHN<br>Initiating Gateway]

subgraph "Municipality 1"
    subgraph "EHR Systems/platform"
        xcads[PJD.XcaDocumentSource]
        regrep[(Document/<br>Document Metadata)]
    end
end

nhnxca<--Queries-->xcads<--Retrieves-->regrep
```
*Municipality sharing documents via XcaDocumentSource*

### 2. Hospital Sharing Discharge Summaries with GPs and Home Care Providers

#### Scenario:
A hospital wants to make discharge notes and care plans available to the patient’s general practitioner and municipal home care team.

#### Use Case Flow:
* After discharge, the hospital’s EPR system sends a CDA discharge summary.
* PJD.XcaDocumentSource registers it with the national XCA infrastructure.
* GPs and municipal care teams query and retrieve the document seamlessly.

#### Benefits:
Ensures continuity of care and informed follow-up treatment.

### 3. Private Specialist Making Consult Notes Available to Hospitals

#### Scenario:
A private dermatologist or cardiologist needs to ensure that their consult findings are accessible to the referring hospital or emergency department.

#### Use Case Flow:
* Specialist uploads or sends consult note (CDA, PDF) to the integration system.
* Integration with PJD.XcaDocumentSource makes the document available with appropriate access rights.
* Hospitals query using the national gateway and retrieve the documents.

#### Benefit:
Reduces duplicated effort, supports informed emergency care.
