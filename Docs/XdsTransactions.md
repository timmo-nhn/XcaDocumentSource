# SOAP-transactions In IHE XDS/XCA  
Below are the transactions supported by default by **PJD.XcaDocumentSource**. Each transaction section contains a table defining the properties of the transaction.  
> It is reccomended to read [Xds And Soap](/Docs/XdsAndSoap.md) first, as it gives an introduction to SOAP/ITI messages and the components 

## ITI-Transactions

### ITI-18 - Registry Stored Query 
This transaction is used in a dialogue between the Document Requester and the Document Registry to query documents with specific properties.  
A request with specific search parameters is sent from a Document Requester to the Document Registry, which sends back a list of documents that satisfy the search parameters.

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Get list of metadata for patient or resource |
| Endpoint    | /Registry/Services/RegistryService |
| XML type request | `<AdhocQueryRequest>` |
| SOAP request action | urn:ihe:iti:2007:RegistryStoredQuery |
| XML type response | `<AdhocQueryResponse>`          |
| SOAP response action | urn:ihe:iti:2007:RegistryStoredQueryResponse |

*ITI-18 request*

#### AdhocQuery Request types  
An `<AdhocQuery>` request can feature different queries for different types of items in the Document Registry. 
The example below shows an `<AdhocQueryRequest>` with id `urn:uuid:14d4debf-8f97-4251-9a74-a90016b0af0d` (`FindDocuments`). Each type of `<AdhocQuery>` has different requirements and optionalities for the slots used in the search.
#### AND/OR semantics
An `<AdhocQueryRequest>` contains `<Slot>`s to specify which parameters/metadata to search for. Each `<Slot>` in the `<AdhocQueryRequest>` works as a **AND-clause**. However, for some slots, the `<Value>` elements in the `<ValueList>` works as an **OR-clause**. It can be thought of like this:  
*This slot with this value **AND** that slot with this **OR** that value*

#### Example  
```xml
<AdhocQueryRequest xmlns="urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0">
    <ResponseOption returnType="LeafClass" returnComposedObjects="true" />
    <AdhocQuery id="urn:uuid:14d4debf-8f97-4251-9a74-a90016b0af0d"
        xmlns="urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0">
        <Slot name="$XDSDocumentEntryPatientId">
        <!-- This slot -->
            <ValueList>
                <Value>'13116900216^^^&amp;2.16.578.1.12.4.1.4.1&amp;ISO'</Value>
            </ValueList>
        </Slot>
        <!-- AND that slot -->
        <Slot name="$XDSDocumentEntryStatus">
            <ValueList>
                <Value>('urn:oasis:names:tc:ebxml-regrep:StatusType:Approved')</Value>
                <!-- with this OR that value-->
                <Value>('urn:oasis:names:tc:ebxml-regrep:StatusType:CustomStatus01')</Value>
            </ValueList>
        </Slot>
    </AdhocQuery>
</AdhocQueryRequest>
```

*AdhocQueryRequest with FindDocuments stored query with a slot for patientId and two different statuses*


```c#
[AdhocQueryRequest]
    [ResponseOption]    [1..1]
    [AdhocQuery]        [1..1]
        [SlotType]      [1..*]
```

*Cardinality of AdhocQueryRequest*


> **🔶 Implementation Note x:** <br> While there are many StoredQueries, **PJD.XcaDocumentSource only supports the ones in the table below**, out of the box  
 
| StoredQuery | Guid |
|---|---|
| FindDocuments | urn:uuid:14d4debf-8f97-4251-9a74-a90016b0af0d |
| FindSubmissionSets | urn:uuid:f26abbcb-ac74-4422-8a30-edb644bbc1a9 |
| FindFolders | urn:uuid:958f3006-baad-4929-a4de-ff1114824431 |
| GetFolders | urn:uuid:5737b14c-8a1a-4539-b659-e03a34a5e1e4 |
| GetAssociations | urn:uuid:a7ae438b-4bc2-4642-93e9-be891f7bb155 |
| GetAll | urn:uuid:10b545ea-725c-446d-9b95-8aeb444eddf3 |

*Possible Stored Queries in PJD.XcaDocumentSource*


#### AdhocQuery Response  
In `FindDocuments` stored queries, a **Document Consumer** can choose between two response types:  
1.	`ObjectRef`: Returns only the documents' unique identificators (UUID) 
2.	`LeafClass`: Returns all metadata the system  can return.  

>**🔶 Implementation Note x:** <br> If neither `ObjectRef` or `LeafClass` is specified (such as if `<ResponseOption>` is missing), an **empty `<AdhocQueryResponse>`** is returned

More on [ITI-18 - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html)  
See [3.18.4.1.2.3.7 Parameters for Required Queries - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html#3.18.4.1.2.3.7) for more information on query parameters aswell as AND/OR semantics


### ITI-38 - Cross Gateway Query 
Cross Gateway Query is essentially Exactly the same as an **ITI-18 AdhocQuery request**, apart from the `<Action>`-field in the `<Header>` of the **SOAP-request**.
In practice, The ITI-38 request originates from the NHN XCA gateway, and is used when querying documents across **Affinity domains**.  
Internally, the ITI-38 request is transformed into an **ITI-18** request, and sent via `HTTP` to the Registry-endpoint as a normal **ITI-18-request**.

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Get list of metadata for patient or resource |
| Endpoint | /XCA/Services/RespondingGatewayService |
| XML type request | `<AdhocQueryRequest>` |
| SOAP request action | urn:ihe:iti:2007:CrossGatewayQuery |
| XML type response | `<AdhocQueryResponse>` |
| SOAP response action | urn:ihe:iti:2007:CrossGatewayQueryResponse |

*ITI-38 request*

More on [ITI-38 - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/Volume2/ITI-38.html)  


### ITI-39 - Cross Gateway Retrieve
Cross Gateway Retrieve is essentially Exactly the same as an **ITI-43 Retrieve Document Set request**, apart from the `<Action>`-field in the `<Header>` of the **SOAP-request**.
In practice, The ITI-39 request originates from the NHN XCA gateway, and is used when querying documents across **Affinity domains**.  
Internally, the ITI-39 request is transformed into an **ITI-43** request, and sent via `HTTP` to the Repository-endpoint as a normal **ITI-43-request**.

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Get document or set of documents from Repository |
| Endpoint    | /XCA/Services/RespondingGatewayService |
| XML type request | `<RetrieveDocumentSetRequest>` |
| SOAP request action | urn:ihe:iti:2007:CrossGatewayRetrieve |
| XML type response | `<RetrieveDocumentSetResponse>`          |
| SOAP response action |rn:ihe:iti:2007:CrossGatewayRetrieveResponse |

*ITI-39 request*

More on [ITI-39 - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/Volume2/ITI-39.html)  


### ITI-41 Provide and Register Document Set.b
The ITI-41 transaction is used to upload **metadata** and **documents** to the Document Registry and Repository, respectively. Internally, the **ITI-41 request** is transformed into an **ITI-42 request**, which is sent to the **Registry**. If the **Registry Request** is successful, the document is uploaded to the **Repository**. If an error occurs while uploading the Registry content, the request is aborted (atomicity). 

> The **ITI-41** (and **ITI-42**) transactions can seem intimidating in size.
However, it's merely a culmination of the types and formats explained earlier in the document

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Get document or set of documents from Repository |
| Endpoint    | /XCA/Services/RespondingGatewayService |
| XML type request | `<ProvideAndRegisterDocumentSetRequest>` |
| SOAP request action | urn:ihe:iti:2007:ProvideAndRegisterDocumentSet-b |
| XML type response | `<RegistryResponse>`          |
| SOAP response action | urn:ihe:iti:2007:ProvideAndRegisterDocumentSet-bResponse |

*ITI-41 request*


#### Example  
```xml
<ProvideAndRegisterDocumentSetRequest xmlns="urn:ihe:iti:xds-b:2007">
    <SubmitObjectsRequest xmlns="urn:oasis:names:tc:ebxml-regrep:xsd:lcm:3.0">
        <RegistryObjectList xmlns="urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0">
            <RegistryPackage 
                id="RegistryPackage01" 
                objectType="urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:RegistryPackage">
                [......]
            </RegistryPackage>
            <ExtrinsicObject 
                id="ExtrinsicObject01" 
                objectType="urn:uuid:7edca82f-054d-47f2-a032-9b2a5b5186c1" 
                status="urn:oasis:names:tc:ebxml-regrep:StatusType:Approved" 
                mimeType="application/pdf">
                [......]
            </ExtrinsicObject>
            <Association 
                id="Association01" 
                objectType="urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Association" 
                associationType="urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember" 
                sourceObject="RegistryPackage01" 
                targetObject="ExtrinsicObject01">
                <Slot name="SubmissionSetStatus">
                    <ValueList>
                        <Value>Original</Value>
                    </ValueList>
                </Slot>
            </Association>
        </RegistryObjectList>
    </SubmitObjectsRequest>
    <Document id="ExtrinsicObject01">JVBERi0xLjcNCiW1tb...</Document>
</ProvideAndRegisterDocumentSetRequest>

```

```c#
[ProvideAndRegisterDocumentSetRequest]
    [SubmitObjectsRequest]              [1..*]
        [RegistryObjectList]            [1..1]
            [Association]               [0..*]
            [RegistryPackage]           [0..*]
            [ExtrinsicObject]           [0..*]
    [Document]                          [1..*]
```

*Cardinality of ProvideAndRegisterDocumentSetRequest*

More on [ITI-41 - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/Volume2/ITI-41.html)  


### ITI-42 Register Document Set.b
Register Document Set is used to upload metadata to the **Document Registry**. A `<RegisterDocumentSetRequest>` is provided, containing the items to be added to the Registry. **Similar to ITI-41**, the metadata or associated resources for a patient is provided, **however**, in this case, **no actual document is provided**. This might be because the document already exists elsewhere or because only registering metadata is appropriate.

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Get document or set of documents from Repository |
| Endpoint    | /Registry/Services/RegistryService |
| XML type request | `<RegisterDocumentSetRequest>` |
| SOAP request action | urn:ihe:iti:2007:RegisterDocumentSet-b |
| XML type response | `<RegistryReponse>`          |
| SOAP response action | urn:ihe:iti:2007:RegisterDocumentSet-bResponse |

*ITI-42 request*


#### Example  
```xml
<RegisterDocumentSetRequest xmlns="urn:ihe:iti:xds-b:2007">
    <SubmitObjectsRequest xmlns="urn:oasis:names:tc:ebxml-regrep:xsd:lcm:3.0">
        <RegistryObjectList xmlns="urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0">
            <RegistryPackage 
                id="RegistryPackage01" 
                objectType="urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:RegistryPackage">
                [......]
            </RegistryPackage>
            <ExtrinsicObject 
                id="ExtrinsicObject01" 
                objectType="urn:uuid:7edca82f-054d-47f2-a032-9b2a5b5186c1" 
                status="urn:oasis:names:tc:ebxml-regrep:StatusType:Approved" 
                mimeType="application/pdf">
                [......]
            </ExtrinsicObject>
            <Association 
                id="Association01" 
                objectType="urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Association" 
                associationType="urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember" 
                sourceObject="RegistryPackage01" 
                targetObject="ExtrinsicObject01">
                <Slot name="SubmissionSetStatus">
                    <ValueList>
                        <Value>Original</Value>
                    </ValueList>
                </Slot>
            </Association>
        </RegistryObjectList>
    </SubmitObjectsRequest>
</RegisterDocumentSetRequest>
```

*Example of RegisterDocumentSet request*


```c#
[RegisterDocumentSetRequest]
    [SubmitObjectsRequest]              [1..*]
        [RegistryObjectList]            [1..1]
            [Association]               [0..*]
            [RegistryPackage]           [0..*]
            [ExtrinsicObject]           [0..*]
```

*Cardinality of RegisterDocumentSetRequest*


More on [ITI-42 - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/Volume2/ITI-42.html)  


### ITI-43 - Retrieve Document Set 
ITI-43 is used by Document Consumer to retrieve one or more documents from Document Repository.  
The Document Consumer must use the following attributes received from `<AdhocQueryResponse>` via **ITI-18 Registry Stored Query**:  

| Field  | Name | Aquisition |
|---|---|---|
| Affinity Domain ID | `homeCommunityID` | `ExtrinsicObject` attribute `home`<br> `<ExtrinsicObject id="eo01" home="2.16.578.1.12.4.5.100.1"` |
| DocumentEntry ID | `documentUniqueId` | `ExtrinsicObject` attribute `id`<br> `<ExtrinsicObject id="eo01"` |
| Document Repository ID | `repositoryUniqueId` | `ExtrinsicObject` slot `repositoryUniqueId`<br> ```<Slot name="repositoryUniqueId">[...]<Value>2.16.578.1.12.4.5.100.1.2</Value>[...]</Slot>``` |


| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Get a document or multiple documents |
| Endpoint    | /Repository/Services/RepositoryService |
| XML type request | `<RetrieveDocumentSetRequest>` |
| SOAP request action | urn:ihe:iti:2007:RegistryStoredQuery |
| XML type response | `<RetrieveDocumentSetResponse>`          |
| SOAP response action | urn:ihe:iti:2007:RegistryStoredQueryResponse |

*ITI-43 request*

>**🚩 National Extension**<br> [IHE ITI-TF Vol.3 4.2.3.2.26 - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/Volume3/ch-4.2.html#4.2.3.2.26) specifies constraints for a document unique ID. **PJD.XcaDocumentSource, aswell as other document sources in norway, does not enforce these constraints by default** - this falls onto the producing application


#### Example  
```xml
<ns2:RetrieveDocumentSetRequest
    xmlns:ns2="urn:ihe:iti:xds-b:2007">
    <ns2:DocumentRequest>
        <ns2:HomeCommunityId>2.16.578.1.12.4.5.100.1</ns2:HomeCommunityId>
        <ns2:RepositoryUniqueId>2.16.578.1.12.4.5.100.1.2</ns2:RepositoryUniqueId>
        <ns2:DocumentUniqueId>105085430</ns2:DocumentUniqueId>
    </ns2:DocumentRequest>
</ns2:RetrieveDocumentSetRequest>
```

*RetrieveDocumentSetRequest request for document with Id 105085430*


```c#
[RetrieveDocumentSetRequest]
    [DocumentRequest]           [1..*]
        [HomeCommunityId]       [1..1]
        [RepositoryUniqueId]    [1..1]
        [DocumentUniqueId]      [1..1]
```

*Cardinality of RetrieveDocumentSetRequest*


More on [ITI-43 - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/TF/Volume2/ITI-43.html)  


## ITI-62 Remove Objects Request
Remove objects is used to remove objects from the **Document Registry**. A list of `<ObjectRef>`s are provided, specifying the identifier for each item to be removed from the **Registry**.

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Remove one or more items from the **Document Registry** by specifying the `Id` of the item(s) to remove |
| Endpoint    | /Registry/Services/RegistryService |
| XML type request | `<RemoveObjectsRequest>` |
| SOAP request action | urn:ihe:iti:2010:DeleteDocumentSet |
| XML type response | `<RegistryResponse>`          |
| SOAP response action | urn:ihe:iti:2010:DeleteDocumentSetResponse |

*ITI-62 request*


#### Example  
```xml
<lcm:RemoveObjectsRequest xmlns:lcm="urn:oasis:names:tc:ebxml-regrep:xsd:lcm:3.0">
    <rim:ObjectRefList xmlns:rim="urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0">
        <rim:ObjectRef id="Association01"/>
        <rim:ObjectRef id="RegistryPackage01"/>
        <rim:ObjectRef id="ExtrinsicObject03"/>
    </rim:ObjectRefList>
</lcm:RemoveObjectsRequest>
```

*RemoveObjectsRequest request for the removal of objects Association01, RegistryPackage01 and ExtrinsicObject03*


```c#
[RemoveObjectsRequest]
    [ObjectRefList]       [1..1]
        [ObjectRef]       [1..*]
```

*Cardinality of RemoveObjectsRequest*

### ITI-86 Delete Document Set
Remove objects is used to remove objects from the **Document Registry**. A list of `<ObjectRef>`s are provided, specifying the identifier for each item to be removed from the **Registry**.

| Property  | Description |
|---|---|
| HTTP action | POST |
| Short description | Delete documents |
| Endpoint    | /Repository/Services/RepositoryService |
| XML type request | `<RemoveDocumentsRequest>` |
| SOAP request action | urn:ihe:iti:2017:RemoveDocuments |
| XML type response | `<RegistryResponse>`          |
| SOAP response action | urn:ihe:iti:2017:RemoveDocumentsResponse |

*ITI-86 request*


#### Example  
```xml
<rmd:RemoveDocumentsRequest xmlns:rmd="urn:ihe:iti:rmd:2017">
    <xds:DocumentRequest xmlns:xds="urn:ihe:iti:xds-b:2007">
        <xds:HomeCommunityId>2.16.578.1.12.4.5.100.1</xds:HomeCommunityId>
        <xds:RepositoryUniqueId>2.16.578.1.12.4.5.100.1.2</xds:RepositoryUniqueId>
        <xds:DocumentUniqueId>ExtrinsicObject03</xds:DocumentUniqueId>
    </xds:DocumentRequest>
</rmd:RemoveDocumentsRequest>
```

*RemoveDocumentsRequest request for the removal of ExtrinsicObject03*


```c#
[RemoveDocumentsRequest]
    [DocumentRequest]           [1..*]
        [HomeCommunityId]       [1..1]
        [RepositoryUniqueId]    [1..1]
        [DocumentUniqueId]      [1..1]
```

*Cardinality of RemoveDocumentsRequest*


## Multipart request and response handling
Multipart requests and responses are ways of separating binary or attachment data from the request or response, which makes processing the message easier.  
A multipart request has the `multipart/related` mimetype, and the request is divided into multiple parts, separated by **Mime Boundaries**.

**Mime Boundaries** are markers in the HTTP response body content, signaling to the consumer that another part of the multipart-response is beginning and ending. Each mime part of the multipart response starts with two dashes (`--`) then an **unique identifier** (the id is often prefixed with `MIMEBoundary_`).  

The **unique identifier** is the same for all the parts in the multipart response, and the `Content-ID` mime part header is used to unqiuely identify each mime part.
Below the boundary, some headers for the mime part is defined, like its content type and a `Content-ID`. The end of the multipart content is defines with a multipart boundary, but this time also ending with two dashes (`--`).  

Headers in a multipart message is also important. It contains not only the content type (often `multipart/related`) of the message, but also some metadata about where the consumer can find the first mime-part, and what the boundary looks like. This is included as extra parameters in the `Content-Type` header of the message

### Example multipart message headers
```yaml
Content-Type: multipart/related; boundary="MIMEBoundary_1234567890"; type="application/xop+xml"; start="<contentid001@tempuri.org>"; start-info="application/xop+xml";
```
*Content-Type header with metadata about the mime-boundary, what content type it is, and the content-id of the beginning mime part*
### Example multipart message body
```xml
--MIMEBoundary_1234567890
Content-Type: application/xml; charset=UTF-8
Content-Transfer-Encoding: binary
Content-ID: <contentid001@tempuri.org>

<message>
    <title>Example message</title>
    <Document><xop:Include href="cid:contentid002@tempuri.org" /></Document>
</message>

--MIMEBoundary_1234567890
Content-Type: text/plain; charset=UTF-8
Content-Transfer-Encoding: binary
Content-ID: <contentid002@tempuri.org>

Hello World!
Example multipart content body

--MIMEBoundary_1234567890--
```
*Multipart body with an `<Include>` element which references the other mime-part of the body (`cid:contentid002@tempuri.org`), instead of including the data directly in the XML*


### Example ITI-39 Request Headers
```yaml
Accept: */*
Host: bjarne-sykehus.t-xcads.pjd.nhn.no
User-Agent: Axis2
Accept-Encoding: gzip,deflate
Content-Type: multipart/related; boundary="MIMEBoundary_1637b04b44658d0fc481e4a0ebf90039be1845d39515bf9e"; type="application/xop+xml"; start="<0.0637b04b44658d0fc481e4a0ebf90039be1845d39515bf9e@apache.org>"; start-info="application/soap+xml"; action="urn:ihe:iti:2007:CrossGatewayRetrieve"
Transfer-Encoding: chunked
X-Forwarded-For: 10.204.36.17
X-Forwarded-Proto: https
```

### Example IIT-39 Request Body
```xml
--MIMEBoundary_1637b04b44658d0fc481e4a0ebf90039be1845d39515bf9e
Content-Type: application/xop+xml; charset=UTF-8; type="application/soap+xml"
Content-Transfer-Encoding: binary
Content-ID: <0.0637b04b44658d0fc481e4a0ebf90039be1845d39515bf9e@apache.org>

<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://www.w3.org/2003/05/soap-envelope">
      <soapenv:Header xmlns:wsa="http://www.w3.org/2005/08/addressing">
            <wsa:ReplyTo soapenv:mustUnderstand="1">
                  <wsa:Address>http://www.w3.org/2005/08/addressing/anonymous</wsa:Address>
            </wsa:ReplyTo>
            <wsa:To>https://bjarne-sykehus.t-xcads.pjd.nhn.no/XCA/services/RespondingGatewayService</wsa:To>
            <wsa:MessageID>urn:uuid:7108041a-6cdb-4988-ad62-c40cec6695b1</wsa:MessageID>
            <wsa:Action>urn:ihe:iti:2007:CrossGatewayRetrieve</wsa:Action>
      </soapenv:Header>
      <soapenv:Body>
            <ns3:RetrieveDocumentSetRequest xmlns:ns3="urn:ihe:iti:xds-b:2007">
                  <ns3:DocumentRequest>
                        <ns3:HomeCommunityId>urn:oid:2.16.578.1.12.4.5.100.15</ns3:HomeCommunityId>
                        <ns3:RepositoryUniqueId>2.16.578.1.12.4.5.100.15.25</ns3:RepositoryUniqueId>
                        <ns3:DocumentUniqueId>e424bf1d-eb39-4c82-a0c7-9ed626f06e3f</ns3:DocumentUniqueId>
                  </ns3:DocumentRequest>
            </ns3:RetrieveDocumentSetRequest>
      </soapenv:Body>
</soapenv:Envelope>
--MIMEBoundary_1637b04b44658d0fc481e4a0ebf90039be1845d39515bf9e--
```

### Example ITI-39 Response Headers
```yaml
connection: keep-alive 
content-length: 83663 
content-type: multipart/related; type="application/xop+xml"; boundary="MIMEBoundary_6b64d6d0cb0948c4a3c26ef43f4778aa"; start="<9798654056f642e4b46d7a53081c27df@xcadocumentsource.com>"; start-info="application/soap+xml" 
date: Fri,19 Sep 2025 06:46:23 GMT 
server: Kestrel 
strict-transport-security: max-age=31536000 
```

### Example Response Body

```xml
--MIMEBoundary_6b64d6d0cb0948c4a3c26ef43f4778aa
Content-Type: application/xop+xml; charset=utf-8; type="application/soap+xml"
Content-ID: <9798654056f642e4b46d7a53081c27df@xcadocumentsource.com>
Content-Transfer-Encoding: binary

  <Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="http://www.w3.org/2003/05/soap-envelope">
    <Header>
        <!-- Omitted for brevity -->
    </Header>
    <Body>
      <RetrieveDocumentSetResponse xmlns="urn:ihe:iti:xds-b:2007">
        <RegistryResponse status="urn:oasis:names:tc:ebxml-regrep:ResponseStatusType:Success" xmlns="urn:oasis:names:tc:ebxml-regrep:xsd:rs:3.0" />
        <DocumentResponse>
          <HomeCommunityId>2.16.578.1.12.4.5.100.15</HomeCommunityId>
          <RepositoryUniqueId>2.16.578.1.12.4.5.100.15.25</RepositoryUniqueId>
          <DocumentUniqueId>94887fbf-ae4f-489c-ab26-d2d05b6d2303</DocumentUniqueId>
          <mimeType>application/hl7-v3+xml</mimeType>
          <Document>
            <xop:Include href="cid:9a01c0d58366472aa0242631bf36e49f@xcadocumentsource.com" xmlns:xop="http://www.w3.org/2004/08/xop/include" />
          </Document>
        </DocumentResponse>
      </RetrieveDocumentSetResponse>
    </Body>
  </Envelope>

--MIMEBoundary_6b64d6d0cb0948c4a3c26ef43f4778aa
Content-Type: application/hl7-v3+xml
Content-ID: 
  <9a01c0d58366472aa0242631bf36e49f@xcadocumentsource.com>
    
Content-Transfer-Encoding: binary

<ClinicalDocument xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="urn:hl7-org:v3">
    <!-- Document content omitted for brevity -->
</ClinicalDocument>
    
--MIMEBoundary_6b64d6d0cb0948c4a3c26ef43f4778aa--

```