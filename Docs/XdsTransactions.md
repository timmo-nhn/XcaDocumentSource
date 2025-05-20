## Transactions In IHE XDS  
Below are the transactions supported by default by **PJD.XcaDocumentSource**. Each transaction section contains a table defining the properties of the transaction.  
> It is reccomended to read [Xds And Soap](/Docs/XdsAndSoap.md) first, as it gives an introduction to SOAP/ITI messages and the components 
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

*Table x: ITI-18 request*

#### AdhocQuery Request types  
An `<AdhocQuery>` request can feature different queries for different types of items in the Document Registry. 
The example below shows an `<AdhocQueryRequest>` with id `urn:uuid:14d4debf-8f97-4251-9a74-a90016b0af0d` (`FindDocuments`). Each type of `<AdhocQuery>` has different requirements and optionalities for the slots used in the search.
##### AND/OR semantics
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
| GetAll | urn:uuid:10b545ea-725c-446d-9b95-8aeb444eddf3 |
| GetFolders | urn:uuid:5737b14c-8a1a-4539-b659-e03a34a5e1e4 |
| GetAssociations | urn:uuid:a7ae438b-4bc2-4642-93e9-be891f7bb155 |
| FindSubmissionSets | urn:uuid:f26abbcb-ac74-4422-8a30-edb644bbc1a9 |

*Table x: Possible Stored Queries in PJD.XcaDocumentSource*


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

*Table x: ITI-38 request*

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

*Table x: ITI-39 request*

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

*Table x: ITI-41 request*


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

*Table x: ITI-42 request*



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

*Table x: ITI-43 request*

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


### ITI-62 Remove Objects Request
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

*Table x: ITI-62 request*


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

*Table x: ITI-86 request*


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
