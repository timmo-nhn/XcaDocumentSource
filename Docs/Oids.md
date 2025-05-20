## Object Identifiers (OID)
>**Note:** For OIDs to effectively work, there must be some level of governance when creating and managing OIDs. Norsk helsenett (NHN) should have a comprehensive overview of the OIDs related to PHR.  
NHN should be informed of the OID of a new document source.

OID (Object Identifiers) are unique identifiers for objects or things. **Anything can have an OID. In practice, OIDs are simply a set of numbers and dots (.) which make up a hierarchical structure**. In PHR, OIDs are used to unambiguously identify a system or facility. The OIDs might get translated by the systems into the actual URL, which means the URL can change, but the OID stays the same. OIDs are also used in logging. OIDs have a "tree/path"-like structure, and can be represented by its numerical or text variant.  
More about OIDs on [NHN's Developer portal ↗](https://utviklerportal.nhn.no/informasjonstjenester/pasientens-journaldokumenter-i-kjernejournal/mer-om-tjenesten/oider/) (In Norwegian).  

### Governing Object Identifiers
Even though OIDs are simply numbers and dots, its the way that its governed and controlled which defines its effectiveness in practice. Having good control over an OID structure leads to effective communication and identification.   

>**🔶 Implementation Note x** <br> In **XcaDocumentSource**, OIDs are used for **RepositoryID** and **HomecommunityID**.

The **Norwegian profile of IHE XDS metadata** defines the use of OIDs for identifying communities. Norsk helsenett (NHN) governs an OID-base and is the primary issuer of an OID to a community. Each Norwegian health region also governs their own OIDbase and can choose to issue their own homecommunity ID.  
The OID-base which NDE governs has the following OID structure for document sharing:
* 2.16.578.1.12.4.1.7 – Document sharing root OID
  * 2.16.578.1.12.4.1.7.1 – Community base OIDs governed by NHN
    * 2.16.578.1.12.4.1.7.1.1 – National community
> **⚠️ Alert x:** <br> Historically, this OID-base has belonged to The Norwegian Directorate of eHealth (NDE/e-Helse) for PHR (formely known as Dokumentdeling)
