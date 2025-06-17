# REST-endpoints (CRUD) - RestfulRegistryService

### Create Documents and References
Uploads a Document reference and/or document to the registry. The input type is one `DocumentReference` which can hold one `DocumentEntryDto`, `SubmissionSetDto`, `AssociationDto` and `DocumentDto`. A partial document upload can be done by only uploading a `DocumentEntryDto` and a `SubmissionSetDto`. An association will be created if it is not in the request.

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Upload a Document reference and/or associated document to the registry or repository |
| Endpoint URL | /api/rest/upload |
| Request Object | `DocumentReference` |
| Response Object | `RestfulApiResponse` |

#### Example 

### Read Document List

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Upload a Document reference and associated document to the registry or reposito
| Endpoint URL | /api/rest/document-list |
| Request Query | `id`, `status` |
| Response Object | `DocumentListResponse` |

### Read Document

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Upload a Document reference and associated document to the registry or repository |
| Endpoint URL | /api/rest/document |
| Request Query | `repository`, `home`, `documentid` |
| Response Object | `DocumentResponse` |