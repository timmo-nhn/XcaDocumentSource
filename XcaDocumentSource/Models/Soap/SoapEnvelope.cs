using System.Text.Json.Serialization;
using System.Xml.Serialization;
using XcaDocumentSource.Models.Soap.Xds;

namespace XcaDocumentSource.Models.Soap;


[Serializable]
[XmlRoot("Envelope", Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
public class SoapEnvelope
{
    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
    public SoapHeader Header { get; set; }

    //[XmlElement("Body", Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
    //public object Body { get; set; }

    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
    public SoapBody? Body {  get; set; }    
}

[Serializable]
[XmlType(AnonymousType = true, Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
public partial class SoapBody
{
    //private AdhocQueryResponse? _adhocQueryResponseField;
    //private MethodResponse _methodResponseField;

    //[MessageBodyMember(Namespace = Constants.Xds.Namespaces.Query, Order = 0)]
    //[XmlElement(ElementName = "AdhocQueryResponse", Namespace = DocumentSharing.Xds.Constants.Xds.Namespaces.Query, Order = 0)]
    [XmlElement(Namespace = Soap.Constants.Xds.Namespaces.Query)]
    public AdhocQueryRequest? AdhocQueryRequest { get; set; }

    /*[XmlElement(Namespace = PJD.Xds.Constants.Xds.Namespaces.Xdsb)]
    public RetrieveDocumentSetResponseType? RetrieveDocumentSetResponse { get; set; }

    // Added #RetrieveValueSet
    [XmlElement(ElementName = "RetrieveValueSetResponse", Namespace = PJD.Xds.Constants.Xds.Namespaces.Svs)]
    public RetrieveValueSetResponse? RetrieveValueSetResponse { get; set; }

    // Added #RetrieveMultipleValueSets
    [XmlElement(ElementName = "RetrieveMultipleValueSetsResponse", Namespace = PJD.Xds.Constants.Xds.Namespaces.Svs)]
    public RetrieveMultipleValueSetsResponse? RetrieveMultipleValueSetsResponse { get; set; }

    // Added #ProvideAndRegisterRequest response - ITI-41 Response
    [XmlElement(ElementName = "RegistryResponse", Namespace = PJD.Xds.Constants.Xds.Namespaces.Rs)]
    public RegistryResponseType? RegistryResponse { get; set; }

    // Added #MCCI_IN000002UV01 - ITI-44 Acknowledgement Response
    [XmlElement(ElementName = "MCCI_IN000002UV01", Namespace = PJD.Xds.Constants.Xds.Namespaces.Hl7V3)]
    public MCCI_IN000002UV01? MCCI_IN000002UV01 { get; set; }

    // Added #PRPA_IN201310UV02 - ITI-45 Response
    [XmlElement(ElementName = "PRPA_IN201310UV02", Namespace = PJD.Xds.Constants.Xds.Namespaces.Hl7V3)]
    public PRPA_IN201310UV02? PRPA_IN201310UV02 { get; set; }

    [XmlElement(Namespace = "http://company.com/")]
    public MethodResponse MethodResponse
    {
        get => _methodResponseField;
        set => _methodResponseField = value;
    }

    //[XmlElement(Namespace = DocumentSharing.Xds.Constants.Xds.Namespaces.Svs)]
    //public SoapFault? Fault { get; set; }
    //[XmlElement]
    [XmlElement(Namespace = SimpleSoap.Constants.Soap.Namespaces.SoapEnvelope)]
    public Fault? Fault { get; set; } */
}


[Serializable]
[XmlRoot(Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
public class SoapEnvelope<T>
{
    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.Xsd)]
    public T Body { get; set; }

    public SoapHeader Header { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true, Namespace = Soap.Constants.Soap.Namespaces.Xsi)]
public class SoapHeader
{
    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.Addressing)]
    public string Action { get; set; }

    [XmlElement("MessageID", Namespace = Soap.Constants.Soap.Namespaces.Addressing)]
    public string MessageId { get; set; }

    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.Addressing)]
    public string To { get; set; }

    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.Addressing)]
    public SoapReplyTo? ReplyTo { get; set; }

    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.Addressing)]
    public SoapFaultTo? FaultTo { get; set; }

    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SecurityExt)]
    public Security Security { get; set; }
}


[Serializable]
[XmlType(AnonymousType = true, Namespace = Soap.Constants.Soap.Namespaces.Xsi)]
public class SoapFaultTo
{
    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.Addressing)]
    public string Address = "http://www.w3.org/2005/08/addressing/anonymous";
}

[Serializable]
[XmlType(AnonymousType = true, Namespace = Soap.Constants.Soap.Namespaces.Xsi)]
public class SoapReplyTo
{
    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.Addressing)]
    public string Address = "http://www.w3.org/2005/08/addressing/anonymous";
}

[Serializable]
[XmlType(AnonymousType = true, Namespace = Soap.Constants.Soap.Namespaces.SecurityExt)]
public class Security
{
    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SecurityUtility)]
    public SoapTimestamp Timestamp { get; set; }

    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.Saml2)]
    public Assertion? Assertion { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true, Namespace = Soap.Constants.Soap.Namespaces.SecurityExt)]
public partial class SoapTimestamp
{
    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SecurityUtility)]
    public string Created { get; set; }

    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SecurityUtility)]
    public string Expires { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true, Namespace = Soap.Constants.Soap.Namespaces.SecurityExt)]
public partial class Assertion
{
}


[Serializable]
[XmlType(AnonymousType = true)]
public class Fault
{
    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
    public Code Code { get; set; }

    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
    public Reason Reason { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
public class Code
{
    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
    public string Value { get; set; }

    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
    public Subcode Subcode { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
public class Subcode
{
    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
    public string Value { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
public class Reason
{
    [XmlElement(Namespace = Soap.Constants.Soap.Namespaces.SoapEnvelope)]
    public string Text { get; set; }
}

