# Custom Document Registry Format

To get around the complexities of the metadata standards which ebRIM and IHE ITI infrastructure enforces, a custom data-structure is defined. It is made to correspond to the fields in the IHE Metadata, but does away with generic `<Classification>`, `<ExternalIdentifier>` and `<Slot>` types for simpler, less genric types.

## DocumentReferenceDto overview
The classes defining the structure are found in `<Solution>/XcaXds.Commons/Models/DocumentReferenceDto`. 
### Transformer service
A service layer is implemented, allowing for the format to be transformed to and from ebRIM types. The service can be injected in a class using Dependency Injection.

### Mapping Table (Custom classes to ebRIM/IHE XDS Metadata)
| Property | ebRIM Representation | Description
|---|---|---|
||||