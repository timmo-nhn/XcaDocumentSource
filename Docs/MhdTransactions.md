# MHD (Mobile access to Health Documents)- and FHIR-endpoints
Mobile access to Health Documents provides a simpler way to read documents and document metadata, using the FHIR standard.
## ITI-67 - Find Document References
The ITI-67 transaction utilizes HTTP request query strings for searching. The endpoint maps these to an ITI-18 **Registy Stored Query** transaction internally based on the input.
[Find Document References - profiles.ihe.net ↗](https://profiles.ihe.net/ITI/MHD/ITI-67.html)

```
GET <baseurl>R4/fhir/DocumentReference/_search?patient=13116900216&status=current
```
