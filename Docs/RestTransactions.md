# REST-endpoints (CRUD) - RestfulRegistryService

This page describes the RESTful API-endpoints in **PJD.XcaDocumentSource**. They can be used to externally perform CRUD-operations on the document registry and repository implementation, if nescesarry.

### Create Documents and References
Uploads a Document reference and/or document to the registry. The input type is one `DocumentReference` which can hold one `DocumentEntryDto`, `SubmissionSetDto`, `AssociationDto` and `DocumentDto`. A partial document upload can be done by only uploading a `DocumentEntryDto` and a `SubmissionSetDto`. An **Association** will be created **if it is not present in the request**.

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Upload a Document reference and/or associated document to the registry or repository |
| Endpoint URL | /api/rest/upload |
| Request Object | `DocumentReference` |
| Response Object | `RestfulApiResponse` |

#### Example 
##### Request
```
POST https://localhost:7176/api/rest/upload
```
<details>
<summary><big><strong> View example JSON payload</strong></big></summary>

```json
{
    "documentEntry": {
        "author": {
            "organization": {
                "id": "983974880",
                "organizationName": "Finnmarkssykehuset HF",
                "assigningAuthority": "2.16.578.1.12.4.1.4.101"
            },
            "department": {
                "id": "4211607",
                "organizationName": "Laboratoriemedisinsk avdeling - FIN",
                "assigningAuthority": "2.16.578.1.12.4.1.4.102"
            },
            "person": {
                "id": "502116685",
                "firstName": "BENEDIKTE",
                "lastName": "GEIRAAS",
                "assigningAuthority": "urn:oid:2.16.578.1.12.4.1.4.4"
            }
        },
        "availabilityStatus": "urn:oasis:names:tc:ebxml-regrep:StatusType:Approved",
        "classCode": {
            "code": "A00-1",
            "codeSystem": "2.16.578.1.12.4.1.1.9602",
            "displayName": "Epikriser og sammenfatninger"
        },
        "confidentialityCode": {
            "code": "NORN_DUP",
            "codeSystem": "2.16.578.1.12.4.1.1.9603",
            "displayName": "Nektet, duplikat"
        },
        "creationTime": "2025-02-06T14:13:56",
        "formatCode": {
            "code": "urn:ihe:iti:xds:2017:mimeTypeSufficient",
            "codeSystem": "http://profiles.ihe.net/fhir/ihe.formatcode.fhir/CodeSystem-formatcode",
            "displayName": "urn:ihe:iti:xds:2017:mimeTypeSufficient"
        },
        "hash": "35A3E0104791D9ACD5A16352ED075F390508CA10",
        "healthCareFacilityTypeCode": {
            "code": "86.211",
            "codeSystem": "2.16.578.1.12.4.1.1.1303",
            "displayName": "86.211"
        },
        "homeCommunityId": "2.16.578.1.12.4.5.100.1",
        "languageCode": "nb-NO",
        "mimeType": "application/xml",
        "objectType": "urn:uuid:7edca82f-054d-47f2-a032-9b2a5b5186c1",
        "patientId": {
            "code": "13116900216",
            "codeSystem": "2.16.578.1.12.4.1.7.3.2.1"
        },
        "practiceSettingCode": {
            "code": "4",
            "codeSystem": "2.16.578.1.12.4.1.1.8653",
            "displayName": "4"
        },
        "repositoryUniqueId": "2.16.578.1.12.4.5.100.1.2",
        "size": "80574",
        "serviceStartTime": "2025-01-07T15:14:03",
        "serviceStopTime": "2025-01-22T15:14:03",
        "sourcePatientInfo": {
            "patientId": {
                "id": "13116900216",
                "system": "2.16.578.1.12.4.1.7.3.2.1"
            },
            "givenName": "Line",
            "familyName": "Danser",
            "birthTime": "1969-11-13T00:00:00",
            "gender": "F"
        },
        "title": "test 123",
        "typeCode": {
            "code": "A01-2",
            "codeSystem": "2.16.578.1.12.4.1.1.9602",
            "displayName": "Kriseplan"
        },
        "uniqueId": "ExtrinsicObject01",
        "id": "ExtrinsicObject01"
    },
    "submisisonSet": {
        "author": {
            "organization": {
                "id": "994598759",
                "organizationName": "NORSK HELSENETT SF",
                "assigningAuthority": "2.16.578.1.12.4.1.4.101"
            },
            "department": {
                "id": "1345020",
                "organizationName": "Department X",
                "assigningAuthority": "2.16.578.1.12.4.1.4.102"
            },
            "person": {
                "id": "565505933",
                "firstName": "KVART",
                "lastName": "GREVLING",
                "assigningAuthority": "2.16.578.1.12.4.1.4.4"
            }
        },
        "patientId": {
            "code": "13116900216",
            "codeSystem": "2.16.578.1.12.4.1.7.3.2.1"
        },
        "submissionTime": "2025-02-06T14:13:56",
        "title": "SubmissionSet-RegistryPackage01",
        "uniqueId": "2.16.578.1.12.4.5.7.4.87227.6270.1793",
        "sourceId": "1.2.840.4711.815.1",
        "id": "RegistryPackage01"
    },
    "association": {
        "associationType": "urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember",
        "sourceObject": "RegistryPackage01",
        "submissionSetStatus": "Original",
        "targetObject": "ExtrinsicObject01",
        "id": "Association01"
    }
}
```
</details>

### Read Document List
Read all document references for a given patient identifier. Also supports status (Approved or Deprecated, Default: Approved)

| Property  | Description |
|---|---|
| HTTP action | GET |
| Short description | Upload a Document reference and associated document to the registry or reposito
| Endpoint URL | /api/rest/document-list |
| Request Query | **See table below** | 
| Response Object | `DocumentListResponse` |

#### Request parameters
| Query  | Description |
|---|---|
| `id`(R) | patient identifier to search for. If the patient id is not formatted as a **CX**-datatype, a default **CX** type will be created with assigning authority OID for **Norwegian birth numbers**<br> Example: `?id=13116900216` will assume the assigning authority of the identifier is `2.16.578.1.12.4.1.4.1`|
| `status`(O) | Status of the document. Values are confined to `Approved` or `Deprecated`. Will be joined together with `urn:oasis:names:tc:ebxml-regrep:StatusType:`, so `Approved` is appended to get the urn/ebXML name of Approved status type |
| `serviceStartTime`(O) | DateTime for the service Start time, when the patient care started |
| `serviceStopTime`(O) | DateTime for the service Stop time, when the patient care ended|
| `pageNumber`(O) | Pagination - Current page number for the result. Default: 1 |
| `pageSize`(O) | Pagination - Amount of documentreferences to return per page. Default: 10 |

#### Example 
```
GET https://localhost:7176/api/rest/document-list?id=13116900216
```


#### Example using `x-patient-id` header
To avoid leaking patient identifiers in logs, the patient ID for the search can also be added as a header with the `x-patient-id` property. Internally, it will funciton exactly like the URL query parameter.
```
GET https://localhost:7176/api/rest/document-list?pageNumber=1&pageSize=10

-H 'x-patient-id: 13116900216'
```


### Read Document
Gets a document from the document repository. Will be returned as a base64-encoded string in the JSON. Repository ID and HomecommunityID can be specified. The **Repository** and **Homecommunity** OIDs in the **Application config** will be used if not specified.

| Property  | Description |
|---|---|
| HTTP action | GET |
| Short description | Upload a Document reference and associated document to the registry or repository |
| Endpoint URL | /api/rest/document |
| Request Query | `repository`(O), `home`(O), `document`(R) |
| Response Object | `DocumentResponse` |

#### Example
```
GET https://localhost:7176/api/rest/document?document=extrinsicObjectDocument01
```

### Update Document Reference
Updates an entire existing Document Reference. This endpoint has two behaviors which can be set using the `replace` query parameter:
* **Behavior 1 - Replace** `replace=true` Overwrites the existing document entry entirely 
* **Behavior 2 - Deprecate** `replace=false` Acts similar to how the **IHE XDS** profile defines the behavior for replacing document metadata, where the old DocumentEntry's status is set to `Deprecated`, a new DocumentEntry is created and a **Replace Association** is created between the old and new documententry.

| Property  | Description |
|---|---|
| HTTP action | PUT |
| Short description | Upload a Document reference and associated document to the registry or repository |
| Endpoint URL | /api/rest/update |
| Request Query | `replace`(R) `DocumentReferenceDto` |
| Response Object | `DocumentResponse` |


#### URL
```
PUT https://localhost:7176/api/rest/update?replace=false
```

#### JSON payload
<details>
<summary><big><strong>View example JSON payload</strong></big></summary>

```json
{
    "documentEntry": {
        "author": {
            "organization": {
                "id": "983974880",
                "organizationName": "New Finnmarkssykehuset HF",
                "assigningAuthority": "2.16.578.1.12.4.1.4.101"
            },
            "department": {
                "id": "4211607",
                "organizationName": "New Laboratoriemedisinsk avdeling - FIN",
                "assigningAuthority": "2.16.578.1.12.4.1.4.102"
            },
            "person": {
                "id": "502116685",
                "firstName": "MODIG",
                "lastName": "STOL",
                "assigningAuthority": "urn:oid:2.16.578.1.12.4.1.4.4"
            }
        },
        "availabilityStatus": "urn:oasis:names:tc:ebxml-regrep:StatusType:Approved",
        "classCode": {
            "code": "A00-1",
            "codeSystem": "2.16.578.1.12.4.1.1.9602",
            "displayName": "Epikriser og sammenfatninger"
        },
        "confidentialityCode": {
            "code": "NORN_DUP",
            "codeSystem": "2.16.578.1.12.4.1.1.9603",
            "displayName": "Nektet, duplikat"
        },
        "creationTime": "2025-02-06T14:13:56",
        "formatCode": {
            "code": "urn:ihe:iti:xds:2017:mimeTypeSufficient",
            "codeSystem": "http://profiles.ihe.net/fhir/ihe.formatcode.fhir/CodeSystem-formatcode",
            "displayName": "urn:ihe:iti:xds:2017:mimeTypeSufficient"
        },
        "hash": "35A3E0104791D9ACD5A16352ED075F390508CA10",
        "healthCareFacilityTypeCode": {
            "code": "86.211",
            "codeSystem": "2.16.578.1.12.4.1.1.1303",
            "displayName": "86.211"
        },
        "homeCommunityId": "2.16.578.1.12.4.5.100.1",
        "languageCode": "nb-NO",
        "mimeType": "application/xml",
        "objectType": "urn:uuid:7edca82f-054d-47f2-a032-9b2a5b5186c1",
        "patientId": {
            "code": "13116900216",
            "codeSystem": "2.16.578.1.12.4.1.7.3.2.1"
        },
        "practiceSettingCode": {
            "code": "4",
            "codeSystem": "2.16.578.1.12.4.1.1.8653",
            "displayName": "4"
        },
        "repositoryUniqueId": "2.16.578.1.12.4.5.100.1.2",
        "size": "80574",
        "serviceStartTime": "2025-01-07T15:14:03",
        "serviceStopTime": "2025-01-22T15:14:03",
        "sourcePatientInfo": {
            "patientId": {
                "id": "13116900216",
                "system": "2.16.578.1.12.4.1.7.3.2.1"
            },
            "givenName": "Line",
            "familyName": "Danser",
            "birthTime": "1969-11-13T00:00:00",
            "gender": "F"
        },
        "title": "test 123 oppdatert",
        "typeCode": {
            "code": "A07-2",
            "codeSystem": "2.16.578.1.12.4.1.1.9602",
            "displayName": "Psykologsammenfatning"
        },
        "uniqueId": "ExtrinsicObject01",
        "id": "ExtrinsicObject01"
    },
    "submissionSet": {
        "author": {
            "organization": {
                "id": "994598759",
                "organizationName": "NORSK HELSENETT SF",
                "assigningAuthority": "2.16.578.1.12.4.1.4.101"
            },
            "department": {
                "id": "1345020",
                "organizationName": "Department X",
                "assigningAuthority": "2.16.578.1.12.4.1.4.102"
            },
            "person": {
                "id": "565505933",
                "firstName": "KVART",
                "lastName": "GREVLING",
                "assigningAuthority": "2.16.578.1.12.4.1.4.4"
            }
        },
        "patientId": {
            "code": "13116900216",
            "codeSystem": "2.16.578.1.12.4.1.7.3.2.1"
        },
        "submissionTime": "2025-02-06T14:13:56",
        "title": "SubmissionSet-RegistryPackage01",
        "uniqueId": "2.16.578.1.12.4.5.7.4.87227.6270.1793",
        "sourceId": "1.2.840.4711.815.1",
        "id": "RegistryPackage01"
    },
    "association": {
        "associationType": "urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember",
        "sourceObject": "RegistryPackage01",
        "submissionSetStatus": "Original",
        "targetObject": "ExtrinsicObject01",
        "id": "Association01"
    }
}
```
</details>

### Patch Document Reference
Send a partial request which only updates the specified fields. Useful for small fixes to the registry content.

| Property  | Description |
|---|---|
| HTTP action | PATCH |
| Short description | Partially update an existing document reference by only sending the values that are needed for the update |
| Endpoint URL | /api/rest/update |
| Request Query | `DocumentReferenceDto` |
| Response Object | `DocumentResponse` |

#### URL
```
PATCH https://localhost:7176/api/rest/patch
```
#### Example JSON payload
<details>
<summary><big><strong> View example JSON payload</strong></big></summary>

```json
{
    "documentEntry": {
        "id": "FT_DocumentEntry02",
    "author": {
        "department": {
            "id": "456123789",
        "organizationName": "Corrected Department Name",
        "assigningAuthority": "2.16.578.1.12.4.1.4.102"
      }
    }
  }
}
```
</details>


### Patch Document Reference
Send a partial request which only updates the specified fields. Useful for small fixes to the registry content.

| Property  | Description |
|---|---|
| HTTP action | PATCH |
| Short description | Partially update an existing document reference by only sending the values that are needed for the update |
| Endpoint URL | /api/rest/update |
| Request Query | `DocumentReferenceDto` |
| Response Object | `DocumentResponse` |

#### URL
```
PATCH https://localhost:7176/api/rest/patch
```
#### Example JSON payload
<details>
<summary><big><strong> View example JSON payload</strong></big></summary>

```json
{
    "documentEntry": {
        "id": "FT_DocumentEntry02",
    "author": {
        "department": {
            "id": "456123789",
        "organizationName": "Corrected Department Name",
        "assigningAuthority": "2.16.578.1.12.4.1.4.102"
      }
    }
  }
}
```
</details>