# Technical Implementation Details

## Appsettings.json-file
The **XcaXds.WebService**-solution has an **appsettings.json**-file (found in `<solution>/XcaXds.WebService/appsettings.json`). The section `XdsConfiguration` defines parameters which are used by the XcaDocumentSource solution as a whole.  
The **OIDs** for the **HomecommunityId** and **RepositoryId** is defined and can be changed there if nescesarry.


## Document Registry/Repository Solution
The Document Registry/Repository-solution is created to be easily interchangeable with existing Document Source solutions. It is based on a simple file-based Registry and Repository.  
The Document Registry is a XML-file with a 1:1 representation of the serialized document metadata, such as what is returned in **ITI-messages**.
A RegistryService is registered with Dependency Injection (DI), which holds an instance of the Document Registry. When a document is uploaded or deleted using one of **ITI-41**, **ITI-42** or **ITI-62**, the DI-instance of the Registry is updated to correspond to the modifications made. 
> **⚠️ Note!** <br> If the **Registry.xml** file is edited directly by hand, such as using a text-editor, the application must be restarted or "bumped" by uploading or deleting registry objects with an **ITI-message**, thus triggering an update to the DI-instance.

## Registry/Repository Wrapper
The Registry and RepositoryWrapper-services (at `<solution>/XcaXds.Source/Services/RegistryWrapper.cs`) are wrappers for the actual Registry/Repository Implementation.
When an implementer wants to connect XcaDocumentSource to an existing Registry/Repostiory solution, it should theoretically only be nescesarry to modify this file to correspond to the structure expected from the existing registry solution, although further modifications may be nescesarry.  
The OID for the folder is the Repository-Id as defined in the **appsettings.json**-file and is created automatically when a document is first uploaded.  
The unique ID of the document is represented as the file-system name of the file.

## Base architecture
Below is a diagram of the architecture out of the box.
```mermaid
%%{init: {'theme':'dark'}}%%

flowchart LR

nhnxca[NHN XCA <br> Initiating Gateway]

subgraph "XcaDocumentSource"
    pep[PEP]
    xcares[XCA <br> Responding Gateway]

    subgraph "Services"
        regep[Registry Endpoint<br><pre>/RegistryService</pre>]
        repep[Repository Endpoint<br><pre>/RepositoryService</pre>]
        regs[RegistryService]
        reps[RepositoryService]
    end

    subgraph "Registry/Repository"
        regw[Registry Wrapper]
        repw[Repository Wrapper]
        subgraph "File System"
            regfile[Registry File<br><pre>Registry.xml</pre>]
            repfol[Repository Folder]
        end
    end
end
nhnxca--ITI-38/ITI-39-->pep

pep-->xcares
pep<--Access control-->PDP

xcares --Forwards ITI-38--> regep
xcares --Forwards ITI-39--> repep

regep --> regs
repep --> reps

regs --> regw
reps --> repw

regw-->regfile
repw-->repfol
```

## Example architecture
Below is an example of how an implementer would have modified Registry and Repository-wrappers to connect to an existing document storage solution.
```mermaid
%%{init: {'theme':'dark'}}%%

flowchart LR

nhnxca[NHN XCA <br> Initiating Gateway]

subgraph "XcaDocumentSource"
    pep[PEP]
    xcares[XCA <br> Responding Gateway]
    subgraph "Services"
        regep[Registry Endpoint<br><pre>/RegistryService</pre>]
        repep[Repository Endpoint<br><pre>/RepositoryService</pre>]
        regs[RegistryService]
        reps[RepositoryService]
    end

    subgraph "Registry/Repository"
        regw[Registry Wrapper]
        repw[Repository Wrapper]
    end
end
subgraph "Database/etc."
    regrepdb[(Existing <br>Document Storage <br>Solution)]
end

nhnxca--ITI-38/ITI-39-->pep

pep-->xcares
pep<--Access control-->PDP

xcares --Forwards ITI-38--> regep
xcares --Forwards ITI-39--> repep

regep --> regs
repep --> reps

regs --> regw
reps --> repw
regw-->regrepdb
repw-->regrepdb

```