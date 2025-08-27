# Authorization and Access Control
To effectively control who gets access to resources, **PJD.XcaDocumentSource** implements the P*P pattern for access control and authorization (PEP, PDP, PR), leveraging standards like **XACML Version 2.0** - part of the broader **Identity and Access Management (IAM)** domain.  

See [Xacml 2.0 Core Spec (PDF) - docs.oasis-open.org ↗](https://docs.oasis-open.org/xacml/2.0/access_control-xacml-2.0-core-spec-os.pdf) for more info.

## Authorization flow

```mermaid
%%{init: {'theme':'dark'}}%%

flowchart LR


in[Incoming Request]

subgraph "PJD.XcaDocumentsource"
    pep["PEP<br>(2.) | (6.)"]
    pdp{PDP}
    pr[(&nbsp;&nbsp;PR&nbsp;&nbsp;)]
    ep((API-endpoint))
end

in  --1\.--> pep
pep --3\.--> pdp
pdp --4\.--> pr
pr  --> pdp
pdp --5\.--> pep
pep --6.1\.----> ep
pep --6.2\.--> in
```

### Explanation
&emsp;1.&nbsp;*A request is sent to one of **PJD.XcaDocumentSource's** endpoints which uses **Policy Enforcement Point (PEP)*** 

&emsp;2.&nbsp;*The **PEP** Extracts a Xacml V2.0 request from the requests authorization details (ie. SAML-token in a SOAP-envelope or JWT in HTTP headers)*

&emsp;3.&nbsp;*The **PEP** sends the request to the **Policy Decision Point (PDP)***

&emsp;4.&nbsp;*The **PDP** queries - or has cached - The **Policy Repository (PR)***

&emsp;5.&nbsp;*The **PDP** has evaluated the request against the policies in the Repository and sends the decision result back to the Policy Enforcement Point*

&emsp;6.&nbsp;*The **PEP** receives the decision response.*

&emsp;6.1.&nbsp;*The **PEP** sends the request on to the API-endpoint*  

&emsp;6.2.&nbsp;*The **PEP** denies the request*

## Policy Enforcement Point
**The Policy Enforcement Point** (PEP) sits in front of an API-endpoint (such as the SOAP-endpoints) and intercepts (enforces a policy upon) the request by parsing the authentication details from the request and sending it to the Policy Decision Point (PDP), to authorize the request.

### The Policy Enforcement Point and API-Endpoints
The Policy Enforcement Point is registered as a **middleware-component** in the **ASP.NET Core Middleware Pipeline** and intercepts the requests before they enter the controllers endpoint.
An extension method is also used to define it in the applications `Program.cs`-file, similar to other components.
```c#
app.UsePolicyEnforcementPointMiddleware();
```
*Excerpt from **XcaXds.WebService**'s `Program.cs`-file*  

#### The `[UsePolicyEnforcementPoint]`-Attribute

A custom attribute is used on each API controller which needs access control.Classes decorated with `[UsePolicyEnforcementPoint]` will go through the Policy Enforcement Point middleware (`PolicyEnforcementPointMiddlware.cs`).


```mermaid
%%{init: {'theme':'dark'}}%%

flowchart LR

incomingrequest[Incoming Request]


subgraph "XcaDocumentSource"
    subgraph "PEP"
        xtract[Extract XACML-request from SAML token or JWT]
        sendpeprequest[Send XACML Request to PDP]
        permitdeny{Permit/Deny}
    end
    
    subgraph "Endpoints"
        subgraph "Other Endpoints"
            epx[Endpoint X]
        end
        subgraph "[UsePolicyEnforcementPoint]"
            regep[Registry Endpoint<br><pre>/RegistryService]
            repep[Repository Endpoint<br><pre>/RepositoryService]
            othr["Other endpoints with  [Usepolicyenforcementpoint]"]

        end
    end
    subgraph "PDP/PAP Service"
        pdp[PDP] 
        pr[(&emsp;PR&emsp;)]
    end

end

incomingrequest--> xtract -->

sendpeprequest <--Deny/Permit--> pdp

pdp <--> pr

sendpeprequest-->permitdeny
permitdeny --Deny--> incomingrequest
permitdeny --Permit-->regep
permitdeny --Permit-->repep
```

*Flow-diagram of Policy Enforcement Point*


## Policy Decision Point
**PJD.XcaDocumentSource** uses the `Abc.Xacml`-library to generate, serialize and evaluate policy requests.  
[Abc.Xacml - github.com ↗](https://github.com/abc-software/abc.xacml)


## Policy Repository
The default implementation of the policy repository is of a simple file-system storage. Policies are found in `<Solution>\XcaXds.Source\PolicyRepository\` and each policy is stored as a separate JSON-file.  
Upon initalization of the `FileBasedPolicyRepository`, all the files in the `PolicyRepository`-folder is read and parsed as **PolicyDto** types, which are added to a **PolicySetDto** which is maintained through Dependency Injection as a **Singleton**-instance.


## DTOs for policy and Policy Set
**PJD.XcaDocumentSource** features a DTO for Policies and Policy Sets. This allows for easier creation and storage of policies. XACML-classes and concepts are abstracted behind a simpler JSON structure.  
By default, the `PolicySetDto` generated from the policies in the Policy Repository defaults to the `deny-overrides` (`urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:deny-overrides`)-combining algorithm.  
If any child (Policy/Rule) evaluates to Deny, the result is Deny.  
Otherwise, the next strongest decision applies (e.g. Permit if any Permit, otherwise NotApplicable).

### AND/OR Semantics
**XACML 2.0** features functions that can perform certain operations on attributes or collections of attributes.
In DTO-semantics, this has been simplified for easier configuration and setup.

For the **DTO**s, every item in the `Rules` property are treated with **AND**-semantics.  
Multi-value fields are also supported; any `value` property separated by semicolon are treated with **OR**-semantics.

#### Example: JSON DTO-policy snippet with AND/OR semantics 
Below is a snippet showing how multiple values and attributed can be combined.
```json
//....
"Rules":[
  // If this attribute has...
  {
    "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:role:code",
    // ... a value of either "LE" OR "SP"...
    "value": "LE;SP" 
  },
  //...AND this attribute has this value...
  {
      "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem",
      "value": "urn:oid:2.16.578.1.12.4.1.1.9060"
  }
]
"effect": "Permit"
//...
```
*Policy DTO with AND/OR semantics*

This how the XACML XML will be mapped from the DTO. 
```xml
<xacml:Rule RuleId="urn:rule:fd5473b8530640faa53b0449d3c05a80" Effect="Deny">
    <xacml:Condition>
        <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:and">
            <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:or">
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                    <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">LE</xacml:AttributeValue>
                    <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                        <xacml:SubjectAttributeDesignator 
                          AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:code" 
                          DataType="http://www.w3.org/2001/XMLSchema#string"/>
                    </xacml:Apply>
                </xacml:Apply>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                    <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">SP</xacml:AttributeValue>
                    <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                        <xacml:SubjectAttributeDesignator 
                          AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:code" 
                          DataType="http://www.w3.org/2001/XMLSchema#string"/>
                    </xacml:Apply>
                </xacml:Apply>
            </xacml:Apply>
            <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:or">
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                    <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">urn:oid:2.16.578.1.12.4.1.1.9060</xacml:AttributeValue>
                    <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                        <xacml:SubjectAttributeDesignator 
                          AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem" 
                          DataType="http://www.w3.org/2001/XMLSchema#string"/>
                    </xacml:Apply>
                </xacml:Apply>
            </xacml:Apply>
        </xacml:Apply>
    </xacml:Condition>
</xacml:Rule>
```
*The XACML-rule created from the Policy DTO above*

### Action-mapping
The XACML 2.0 specification does not enforce specific values for the `<Action>`-segment of a `<Policy>` or `<Request>`.  
For SOAP-requests, **PJD.XcaDocumentSource** maps from the `<Action>` in the `<Header>` to specific values for the appropriate action.

|XACML-action|SOAP-action
|--|--|
|`ReadDocumentList`|`urn:ihe:iti:2007:RegistryStoredQuery`<br>`urn:ihe:iti:2007:CrossGatewayQuery`|
|`ReadDocuments`|`urn:ihe:iti:2007:RetrieveDocumentSet`<br>`urn:ihe:iti:2007:CrossGatewayRetrieve`|
|`Create`|`urn:ihe:iti:2007:ProvideAndRegisterDocumentSet-b`<br>`urn:ihe:iti:2007:RegisterDocumentSet-b`|
|`Delete`|`urn:ihe:iti:2010:DeleteDocumentSet`<br>`urn:ihe:iti:2017:RemoveDocuments`|

### Example #1 - Allow certain types of healthcare personell
```json
{
  "id": "90bd12ea-1a26-417f-a035-f3708f4e0198",
  "Rules": [
    {
      "attributeId": "urn:no:ehelse:saml:1.0:subject:SecurityLevel",
      "value": "4"
    },
    {
      "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:role:code",
      "value": "LE;SP"
    },
    {
      "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem",
      "value": "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060"
    },
    {
      "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:code",
      "value": "TREAT;ETREAT;COC;BTG"
    },
    {
      "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:codeSystem",
      "value": "urn:oid:2.16.840.1.113883.1.11.20448;2.16.840.1.113883.1.11.20448"
    }
  ],
  "actions": [
    "ReadDocumentList"
  ],
  "effect": "Permit"
}
```
*Example of a policy where LE and SP (healthcare personell with role **Lege** and **Sykepleier**) are allowed to read a document list (**ReadDocumentList**). Due to the nature of the `deny-overrides` combining algorithm, only values defined in the policy are permitted*

## Endpoints for managing Access control
API-endpoints for performing CRUD-operations on policies are available. These serve as easy-to-use interfaces for configuring access control for **PJD.XcaDocumentSource**

### Get All Policies as Policy Set
| Property  | Description |
|---|---|
| HTTP action | GET |
| Short description | Upload a Document reference and/or associated document to the registry or repository |
| Endpoint URL | /api/policy/upload |
| Request Query | `xml`(O) - Boolean value<br>Return **XACML XML**-formatted or **DTO JSON**-formatted response | 
| Response Object | `PolicySetDto` (`application/json`)<br>`<PolicySet>` (`application/xml`) |

#### Example 
##### Request
```
GET <baseurl>/api/policy/getall?xml=true
```
<details>
<summary><big><strong> View example JSON Response</strong></big></summary>

```json
{
  "setId": "62427a73-0c33-45dc-b269-70ea80d0fac5",
  "combiningAlgorithm": "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:deny-overrides",
  "policies": [
    {
      "id": "90bd12ea-1a26-417f-a035-f3708f4e0198",
      "rules": [
        {
          "attributeId": "urn:no:ehelse:saml:1.0:subject:SecurityLevel",
          "value": "4"
        },
        {
          "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:role:code",
          "value": "LE;SP"
        },
        {
          "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem",
          "value": "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060"
        },
        {
          "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:code",
          "value": "TREAT;ETREAT;COC;BTG"
        },
        {
          "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:codeSystem",
          "value": "urn:oid:2.16.840.1.113883.1.11.20448;2.16.840.1.113883.1.11.20448"
        }
      ],
      "actions": [
        "ReadDocumentList"
      ],
      "effect": "Permit"
    },
    {
      "id": "deny-certain-roles",
      "rules": [
        {
          "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:role:code",
          "value": "XX;VE;FB"
        },
        {
          "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem",
          "value": "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060"
        }
      ],
      "effect": "Deny"
    }
  ]
}
```

</details>

<details>
<summary><big><strong> View example XML Response</strong></big></summary>

```xml
  <xacml:PolicySet PolicySetId="urn:uuid:62427a73-0c33-45dc-b269-70ea80d0fac5" PolicyCombiningAlgId="urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:deny-overrides" Version="1.0" xmlns:xacml="urn:oasis:names:tc:xacml:2.0:policy:schema:os">
    <xacml:Target />
    <xacml:Policy PolicyId="urn:uuid:90bd12ea-1a26-417f-a035-f3708f4e0198" RuleCombiningAlgId="urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:deny-overrides" Version="1.0">
      <xacml:CombinerParameters />
      <xacml:Target>
        <xacml:Actions>
          <xacml:Action>
            <xacml:ActionMatch MatchId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
              <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">ReadDocumentList</xacml:AttributeValue>
              <xacml:ActionAttributeDesignator AttributeId="urn:oasis:names:tc:xacml:1.0:action:action-id" DataType="http://www.w3.org/2001/XMLSchema#string" />
            </xacml:ActionMatch>
          </xacml:Action>
        </xacml:Actions>
      </xacml:Target>
      <xacml:Rule RuleId="urn:rule:8e0dc7910e6742128f0dd78f103860f8" Effect="Permit">
        <xacml:Condition>
          <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:and">
            <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
              <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">4</xacml:AttributeValue>
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                <xacml:SubjectAttributeDesignator AttributeId="urn:no:ehelse:saml:1.0:subject:SecurityLevel" DataType="http://www.w3.org/2001/XMLSchema#string" />
              </xacml:Apply>
            </xacml:Apply>
            <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:or">
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">LE</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:code" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">SP</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:code" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
            </xacml:Apply>
            <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:or">
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">urn:oid:2.16.578.1.12.4.1.1.9060</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">2.16.578.1.12.4.1.1.9060</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
            </xacml:Apply>
            <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:or">
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">TREAT</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:code" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">ETREAT</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:code" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">COC</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:code" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">BTG</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:code" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
            </xacml:Apply>
            <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:or">
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">urn:oid:2.16.840.1.113883.1.11.20448</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:codeSystem" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">2.16.840.1.113883.1.11.20448</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse:codeSystem" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
            </xacml:Apply>
          </xacml:Apply>
        </xacml:Condition>
      </xacml:Rule>
    </xacml:Policy>
    <xacml:Policy PolicyId="urn:uuid:deny-certain-roles" RuleCombiningAlgId="urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:deny-overrides" Version="1.0">
      <xacml:CombinerParameters />
      <xacml:Target />
      <xacml:Rule RuleId="urn:rule:740cc207a25240a5a98632b86c302b69" Effect="Deny">
        <xacml:Condition>
          <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:and">
            <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:or">
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">XX</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:code" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">VE</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:code" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">FB</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:code" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
            </xacml:Apply>
            <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:or">
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">urn:oid:2.16.578.1.12.4.1.1.9060</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
              <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">2.16.578.1.12.4.1.1.9060</xacml:AttributeValue>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                  <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem" DataType="http://www.w3.org/2001/XMLSchema#string" />
                </xacml:Apply>
              </xacml:Apply>
            </xacml:Apply>
          </xacml:Apply>
        </xacml:Condition>
      </xacml:Rule>
    </xacml:Policy>
  </xacml:PolicySet>
```

</details>


### Get Single Policy
Gets a single policy. Can return it either as **DTO JSON**-format or **XML XACML**-format.

#### Example 
##### Request
```
GET <baseurl>/api/policy/getsingle?id=deny-certain-roles&xml=true
```
<details>
<summary><big><strong> View example JSON Response</strong></big></summary>

```json
{
    "id": "deny-certain-roles",
    "rules": [
        {
            "matchId": null,
            "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:role:code",
            "dataType": null,
            "value": "XX;VE;FB"
        },
        {
            "matchId": null,
            "attributeId": "urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem",
            "dataType": null,
            "value": "urn:oid:2.16.578.1.12.4.1.1.9060;2.16.578.1.12.4.1.1.9060"
        }
    ],
    "subjects": null,
    "roles": null,
    "organizations": null,
    "resources": null,
    "actions": null,
    "effect": "Deny"
}
```

</details>

<details>
<summary><big><strong> View example XML Response</strong></big></summary>

```xml
<xacml:Policy xmlns:xacml="urn:oasis:names:tc:xacml:2.0:policy:schema:os" PolicyId="urn:uuid:deny-certain-roles" RuleCombiningAlgId="urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:deny-overrides" Version="1.0">
    <xacml:CombinerParameters/>
    <xacml:Target/>
    <xacml:Rule RuleId="urn:rule:fd5473b8530640faa53b0449d3c05a80" Effect="Deny">
        <xacml:Condition>
            <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:and">
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:or">
                    <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                        <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">XX</xacml:AttributeValue>
                        <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                            <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:code" DataType="http://www.w3.org/2001/XMLSchema#string"/>
                        </xacml:Apply>
                    </xacml:Apply>
                    <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                        <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">VE</xacml:AttributeValue>
                        <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                            <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:code" DataType="http://www.w3.org/2001/XMLSchema#string"/>
                        </xacml:Apply>
                    </xacml:Apply>
                    <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                        <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">FB</xacml:AttributeValue>
                        <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                            <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:code" DataType="http://www.w3.org/2001/XMLSchema#string"/>
                        </xacml:Apply>
                    </xacml:Apply>
                </xacml:Apply>
                <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:or">
                    <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                        <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">urn:oid:2.16.578.1.12.4.1.1.9060</xacml:AttributeValue>
                        <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                            <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem" DataType="http://www.w3.org/2001/XMLSchema#string"/>
                        </xacml:Apply>
                    </xacml:Apply>
                    <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-equal">
                        <xacml:AttributeValue DataType="http://www.w3.org/2001/XMLSchema#string">2.16.578.1.12.4.1.1.9060</xacml:AttributeValue>
                        <xacml:Apply FunctionId="urn:oasis:names:tc:xacml:1.0:function:string-one-and-only">
                            <xacml:SubjectAttributeDesignator AttributeId="urn:oasis:names:tc:xspa:1.0:subject:role:codeSystem" DataType="http://www.w3.org/2001/XMLSchema#string"/>
                        </xacml:Apply>
                    </xacml:Apply>
                </xacml:Apply>
            </xacml:Apply>
        </xacml:Condition>
    </xacml:Rule>
</xacml:Policy>
```

</details>


