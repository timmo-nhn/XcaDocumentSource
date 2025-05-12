# Policy Enforcement Point
XcaDocumentSource features a setup for a Policy Enforcement Point, which is used to allow PAP/PDP system to access control a specific endpoint in XcaDocumentSource. It is based around [eXtensible Access Control Markup Language (XACML) Version 3.0 - docs.oasis-open.org ↗](https://docs.oasis-open.org/xacml/3.0/xacml-3.0-core-spec-cd-04-en.html) using the **abc.xacml**-library for .NET.  
[ABC.XACML - github ↗](https://github.com/abc-software/abc.xacml)  
## Using The Policy Enforcement Point With API-Controllers
The Policy Enforcement Point is registered as a **middleware-component** in the **ASP.NET Middleware Pipeline** and intercepts the requests before they enter the controllers endpoint.
An extension method is also used to define it in the applications `Program.cs`-file, similar to other components.
```c#
app.UsePolicyEnforcementPointMiddleware();
```
*Excerpt from XcaXds.WebService's `Program.cs`-file*  

A custom attribute is used on each API controller to enable usage of the Policy Enforcement Point. Classes decorated with `[UsePolicyEnforcementPoint]` will go through the Policy Enforcement Point middleware (`PolicyEnforcementPointMiddlware.cs`).

```mermaid
%%{init: {'theme':'dark'}}%%

flowchart LR

incomingrequest[Incoming Request]

subgraph "External PDP/PAP"
    pdp 
    pr
end

subgraph "XcaDocumentSource"
    subgraph "PEP"
        pep{Requesting Controller uses UsePolicyEnforcementPoint?}
        sendpeprequest[Send XACML Request to PDP]
        permitdeny{Permit/Deny}

    end
    
    subgraph "Endpoints"
        subgraph "Other Endpoints"
            epx[Endpoint X]
        end
        subgraph "Endpoints registered with [UsePolicyEnforcementPoint]"
            regep[Registry Endpoint<br><pre>/RegistryService</pre>]
            repep[Repository Endpoint<br><pre>/RepositoryService</pre>]

        end
    end

end
incomingrequest-->pep

pep -- Yes --> sendpeprequest
pep -- No --> epx

sendpeprequest <--Deny/Permit--> pdp

pdp <--> pr

sendpeprequest-->permitdeny
permitdeny --Deny<br>(Return SOAP request)--> incomingrequest
permitdeny --Permit-->regep
permitdeny --Permit-->repep

```
*Flow-diagram of Policy Enforcemnt Point*