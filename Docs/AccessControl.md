# Authorization and Access Control

## Policy Enforcement Point
### Using The Policy Enforcement Point With API-Controllers
The Policy Enforcement Point is registered as a **middleware-component** in the **ASP.NET Middleware Pipeline** and intercepts the requests before they enter the controllers endpoint.
An extension method is also used to define it in the applications `Program.cs`-file, similar to other components.
```c#
app.UsePolicyEnforcementPointMiddleware();
```
*Excerpt from XcaXds.WebService's `Program.cs`-file*  

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
        pr[PR]
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

## Endpoints for managing Access control