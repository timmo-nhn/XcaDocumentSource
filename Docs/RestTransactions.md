# REST-endpoints (CRUD) - RestfulRegistryService

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
https://localhost:7176/api/rest/upload
```

##### JSON payload
```json
{
    "documentEntry": {
        "Author": {
            "Organization": {
                "Id": "983974880",
                "OrganizationName": "Finnmarkssykehuset HF",
                "AssigningAuthority": "2.16.578.1.12.4.1.4.101"
            },
            "Department": {
                "Id": "4211607",
                "OrganizationName": "Laboratoriemedisinsk avdeling - FIN",
                "AssigningAuthority": "2.16.578.1.12.4.1.4.102"
            },
            "Person": {
                "Id": "502116685",
                "FirstName": "BENEDIKTE",
                "LastName": "GEIRAAS",
                "AssigningAuthority": "urn:oid:2.16.578.1.12.4.1.4.4"
            }
        },
        "AvailabilityStatus": "urn:oasis:names:tc:ebxml-regrep:StatusType:Approved",
        "ClassCode": {
            "Code": "A00-1",
            "CodeSystem": "2.16.578.1.12.4.1.1.9602",
            "DisplayName": "Epikriser og sammenfatninger"
        },
        "ConfidentialityCode": {
            "Code": "NORN_DUP",
            "CodeSystem": "2.16.578.1.12.4.1.1.9603",
            "DisplayName": "Nektet, duplikat"
        },
        "CreationTime": "2025-02-06T14:13:56",
        "FormatCode": {
            "Code": "urn:ihe:iti:xds:2017:mimeTypeSufficient",
            "CodeSystem": "http://profiles.ihe.net/fhir/ihe.formatcode.fhir/CodeSystem-formatcode",
            "DisplayName": "urn:ihe:iti:xds:2017:mimeTypeSufficient"
        },
        "Hash": "35A3E0104791D9ACD5A16352ED075F390508CA10",
        "HealthCareFacilityTypeCode": {
            "Code": "86.211",
            "CodeSystem": "2.16.578.1.12.4.1.1.1303",
            "DisplayName": "86.211"
        },
        "HomeCommunityId": "2.16.578.1.12.4.5.100.1",
        "LanguageCode": "nb-NO",
        "MimeType": "application/xml",
        "ObjectType": "urn:uuid:7edca82f-054d-47f2-a032-9b2a5b5186c1",
        "PatientId": {
            "Code": "13116900216",
            "CodeSystem": "2.16.578.1.12.4.1.7.3.2.1"
        },
        "PracticeSettingCode": {
            "Code": "4",
            "CodeSystem": "2.16.578.1.12.4.1.1.8653",
            "DisplayName": "4"
        },
        "RepositoryUniqueId": "2.16.578.1.12.4.5.100.1.2",
        "Size": "80574",
        "ServiceStartTime": "2025-01-07T15:14:03",
        "ServiceStopTime": "2025-01-22T15:14:03",
        "SourcePatientInfo": {
            "PatientId": {
                "Id": "13116900216",
                "System": "2.16.578.1.12.4.1.7.3.2.1"
            },
            "GivenName": "Line",
            "FamilyName": "Danser",
            "BirthTime": "1969-11-13T00:00:00",
            "Gender": "F"
        },
        "Title": "test 123",
        "TypeCode": {
            "Code": "A01-2",
            "CodeSystem": "2.16.578.1.12.4.1.1.9602",
            "DisplayName": "Kriseplan"
        },
        "UniqueId": "ExtrinsicObject01",
        "Id": "ExtrinsicObject01"
    },
    "submisisonSet": {
        "Author": {
            "Organization": {
                "Id": "994598759",
                "OrganizationName": "NORSK HELSENETT SF",
                "AssigningAuthority": "2.16.578.1.12.4.1.4.101"
            },
            "Department": {
                "Id": "1345020",
                "OrganizationName": "Department X",
                "AssigningAuthority": "2.16.578.1.12.4.1.4.102"
            },
            "Person": {
                "Id": "565505933",
                "FirstName": "KVART",
                "LastName": "GREVLING",
                "AssigningAuthority": "2.16.578.1.12.4.1.4.4"
            }
        },
        "PatientId": {
            "Code": "13116900216",
            "CodeSystem": "2.16.578.1.12.4.1.7.3.2.1"
        },
        "SubmissionTime": "2025-02-06T14:13:56",
        "Title": "SubmissionSet-RegistryPackage01",
        "UniqueId": "2.16.578.1.12.4.5.7.4.87227.6270.1793",
        "SourceId": "1.2.840.4711.815.1",
        "Id": "RegistryPackage01"
    },
    "association": {
        "AssociationType": "urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember",
        "SourceObject": "RegistryPackage01",
        "SubmissionSetStatus": "Original",
        "TargetObject": "ExtrinsicObject01",
        "Id": "Association01"
    }
}
```

### Read Document List
Read all document references for a given patient identifier. Also supports status (Approved or Deprecated, Default: Approved)

| Property  | Description |
|---|---|
| HTTP action | GET |
| Short description | Upload a Document reference and associated document to the registry or reposito
| Endpoint URL | /api/rest/document-list |
| Request Query | `id`, `status` |
| Response Object | `DocumentListResponse` |

#### Example 
```
https://localhost:7176/api/rest/document-list?id=13116900216
```

### Read Document
Gets a document from the document repository. Will be returned as a base64-encoded string in the JSON. Repository ID and HomecommunityID can be specified. The **Repository** and **Homecommunity** OIDs in the **Application config** will be used if not specified.

| Property  | Description |
|---|---|
| HTTP action | GET |
| Short description | Upload a Document reference and associated document to the registry or repository |
| Endpoint URL | /api/rest/document |
| Request Query | `repository`, `home`, `documentid` |
| Response Object | `DocumentResponse` |

#### Example
```
https://localhost:7176/api/rest/document?document=extrinsicObjectDocument01
```

### Update Document Reference
Updates an entire existing Document Reference. This endpoint has two behaviors which can be set using the `replace` query parameter:
* **Behavior 1 - Replace** `replace=true` Overwrites the existing document entry entirely 
* **Behavior 2 - Deprecate** `replace=false` Acts similar to how IHE XDS defines the behavior for replacing document metadata, where the old DocumentEntry's status is set to `Deprecated`, a new DocumentEntry is created and a **Replace Association** is created between the old and new documententry.

| Property  | Description |
|---|---|
| HTTP action | PUT |
| Short description | Upload a Document reference and associated document to the registry or repository |
| Endpoint URL | /api/rest/update |
| Request Query | `repository`, `home`, `documentid` |
| Response Object | `DocumentResponse` |


#### URL
```
https://localhost:7176/api/rest/update?replace=false
```

#### JSON payload
```json
{
    "documentEntry": {
        "Author": {
            "Organization": {
                "Id": "983974880",
                "OrganizationName": "New Finnmarkssykehuset HF",
                "AssigningAuthority": "2.16.578.1.12.4.1.4.101"
            },
            "Department": {
                "Id": "4211607",
                "OrganizationName": "New Laboratoriemedisinsk avdeling - FIN",
                "AssigningAuthority": "2.16.578.1.12.4.1.4.102"
            },
            "Person": {
                "Id": "502116685",
                "FirstName": "MODIG",
                "LastName": "STOL",
                "AssigningAuthority": "urn:oid:2.16.578.1.12.4.1.4.4"
            }
        },
        "AvailabilityStatus": "urn:oasis:names:tc:ebxml-regrep:StatusType:Approved",
        "ClassCode": {
            "Code": "A00-1",
            "CodeSystem": "2.16.578.1.12.4.1.1.9602",
            "DisplayName": "Epikriser og sammenfatninger"
        },
        "ConfidentialityCode": {
            "Code": "NORN_DUP",
            "CodeSystem": "2.16.578.1.12.4.1.1.9603",
            "DisplayName": "Nektet, duplikat"
        },
        "CreationTime": "2025-02-06T14:13:56",
        "FormatCode": {
            "Code": "urn:ihe:iti:xds:2017:mimeTypeSufficient",
            "CodeSystem": "http://profiles.ihe.net/fhir/ihe.formatcode.fhir/CodeSystem-formatcode",
            "DisplayName": "urn:ihe:iti:xds:2017:mimeTypeSufficient"
        },
        "Hash": "35A3E0104791D9ACD5A16352ED075F390508CA10",
        "HealthCareFacilityTypeCode": {
            "Code": "86.211",
            "CodeSystem": "2.16.578.1.12.4.1.1.1303",
            "DisplayName": "86.211"
        },
        "HomeCommunityId": "2.16.578.1.12.4.5.100.1",
        "LanguageCode": "nb-NO",
        "MimeType": "application/xml",
        "ObjectType": "urn:uuid:7edca82f-054d-47f2-a032-9b2a5b5186c1",
        "PatientId": {
            "Code": "13116900216",
            "CodeSystem": "2.16.578.1.12.4.1.7.3.2.1"
        },
        "PracticeSettingCode": {
            "Code": "4",
            "CodeSystem": "2.16.578.1.12.4.1.1.8653",
            "DisplayName": "4"
        },
        "RepositoryUniqueId": "2.16.578.1.12.4.5.100.1.2",
        "Size": "80574",
        "ServiceStartTime": "2025-01-07T15:14:03",
        "ServiceStopTime": "2025-01-22T15:14:03",
        "SourcePatientInfo": {
            "PatientId": {
                "Id": "13116900216",
                "System": "2.16.578.1.12.4.1.7.3.2.1"
            },
            "GivenName": "Line",
            "FamilyName": "Danser",
            "BirthTime": "1969-11-13T00:00:00",
            "Gender": "F"
        },
        "Title": "test 123 oppdatert",
        "TypeCode": {
            "Code": "A07-2",
            "CodeSystem": "2.16.578.1.12.4.1.1.9602",
            "DisplayName": "Psykologsammenfatning"
        },
        "UniqueId": "ExtrinsicObject01",
        "Id": "ExtrinsicObject01"
    },
    "submissionSet": {
        "Author": {
            "Organization": {
                "Id": "994598759",
                "OrganizationName": "NORSK HELSENETT SF",
                "AssigningAuthority": "2.16.578.1.12.4.1.4.101"
            },
            "Department": {
                "Id": "1345020",
                "OrganizationName": "Department X",
                "AssigningAuthority": "2.16.578.1.12.4.1.4.102"
            },
            "Person": {
                "Id": "565505933",
                "FirstName": "KVART",
                "LastName": "GREVLING",
                "AssigningAuthority": "2.16.578.1.12.4.1.4.4"
            }
        },
        "PatientId": {
            "Code": "13116900216",
            "CodeSystem": "2.16.578.1.12.4.1.7.3.2.1"
        },
        "SubmissionTime": "2025-02-06T14:13:56",
        "Title": "SubmissionSet-RegistryPackage01",
        "UniqueId": "2.16.578.1.12.4.5.7.4.87227.6270.1793",
        "SourceId": "1.2.840.4711.815.1",
        "Id": "RegistryPackage01"
    },
    "association": {
        "AssociationType": "urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember",
        "SourceObject": "RegistryPackage01",
        "SubmissionSetStatus": "Original",
        "TargetObject": "ExtrinsicObject01",
        "Id": "Association01"
    }
}
```

### Patch Document Reference
Send a partial request which only updates the specified fields. Useful for small fixes to the registry content.

| Property  | Description |
|---|---|
| HTTP action | PATCH |
| Short description | Update an existing document reference by only sending the values that are needed for the update |
| Endpoint URL | /api/rest/update |
| Request Query | `repository`, `home`, `documentid` |
| Response Object | `DocumentResponse` |

#### URL
```
https://localhost:7176/api/rest/update
```
#### Example JSON payload
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