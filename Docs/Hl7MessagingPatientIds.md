# HL7 Messaging and Patient-Identifiers
The **XcaDocumentSource** solution includes a minimal but scaleable **HL7 V2** implementation with an endpoint for sending and receiving HL7V2 messages according to the [IHE PDQ profile - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/Volume1/ch-8.html). The endpoint is primarily used by **Document Admin** Frontend to find and reference Patient Identifiers from the Document registry. This in order to assist in filling out forms for the frontend, where, for instance, a full **HL7 CX** datatype is required.

## Patient Demographics Query Implementation
The implementation in **XcaDocumentSource** differs somewhat from what IHE has specified. This implementation of PDQ fetches patient identifiers **directly** from the Document Registry, essentially making the Document Registry the Patient Demographics Supplier.  
In practice, this means there is no separation of concerns when it comes to managing patient identities and metadata about those patients.

## Technical implementation
The **Efferent.HL7.V2** .NET library is used to serialize/deserialize the HL7-messages. ([Efferent.HL7.V2 - GitHub ↗](https://github.com/Efferent-Health/HL7-V2)) It was chosen as it is lightweight, and message-type-agnostic, which differs it from other libraries like **NHAPI**. This allows for more bespoke message types like the `QBP^Q22` and `RSP^K22` messages as specified in the [ITI-21 transaction - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/Volume2/ITI-21.html)  
**Efferent.HL7.V2** library is directly included as source code under `<solution>/XcaXds.Commons/Models/Hl7/V2`.

### API-Routing
The HL7 implementation is set up to allow for easy scaling of HL7-related messaging, if needed. The HL7 API-controller has a Dependency Injected instance of the document registry service (`RegistryService`), allowing for easy access to document registry resources.  
The HL7 implementation features its own API-controller (found in `<solution>/XcaXds.WebService/Controllers/Hl7MessagingController.cs`) and endpoints are published under `/hl7` route.  
An **ASP.NET Inputformatter** is used to serialize the HL7-messages before they are sent to the controller endpoints (found in `<solution>/XcaXds.WebService/InputFormatters/Hl7InputFormatter.cs`). 

#### Default API Routes
| Endpoint | Method
|---|---|
| `<baseurl>/hl7/search-patients` | POST