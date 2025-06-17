# REST-endpoints (CRUD) - RestfulRegistryService

### Create Documents and References

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Upload a Document reference and associated document to the registry or repository |
| Endpoint URL | /api/rest/upload |
| Request Object | `DocumentReference` |
| Response Object | `RestfulApiResponse` |

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