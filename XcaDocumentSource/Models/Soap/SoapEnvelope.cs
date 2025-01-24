using System.Xml.Serialization;
using XcaDocumentSource.Models.Soap.Actions;
using XcaDocumentSource.Models.Soap.XdsTypes;
using XcaDocumentSource.Services;

namespace XcaDocumentSource.Models.Soap;

[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Soap.Namespaces.SoapEnvelope)]
[XmlInclude(typeof(RegistryStoredQueryRequest))]
[XmlInclude(typeof(ProvideAndRegisterDocumentSetbRequest))]
[XmlInclude(typeof(RetrieveDocumentSetResponseType))]
[XmlInclude(typeof(RetrieveDocumentSetbResponse))]
[XmlInclude(typeof(RetrieveDocumentSetbRequest))]
[XmlRoot("Envelope", Namespace = Constants.Soap.Namespaces.SoapEnvelope)]
public class SoapEnvelope
{
    [XmlElement(Namespace = Constants.Soap.Namespaces.SoapEnvelope)]
    public SoapHeader Header { get; set; }

    [XmlElement(Namespace = Constants.Soap.Namespaces.SoapEnvelope)]
    public SoapBody Body { get; set; }

    internal string CreateSoapFault(string fault, string faultCode, string? subCode = null)
    {
        var envelope = new SoapEnvelope()
        {
            Header = new()
            {
                Action = Constants.Soap.Namespaces.AddressingSoapFault,
            },
            Body = new()
            {
                Fault = new()
                {
                    Code = new()
                    {
                        Value = faultCode,
                        Subcode = subCode != null ? new() { Value = subCode } : null
                    },
                    Reason = new()
                    {
                        Text = fault
                    }
                }
            }
        };
        var serializer = new SoapXmlSerializer();
        var result = serializer.SerializeSoapMessageToXmlString(envelope);
        return result;
    }
}


[Serializable]
[XmlType(AnonymousType = true, Namespace = Constants.Soap.Namespaces.SoapEnvelope)]
public partial class SoapBody
{
    [XmlAttribute(AttributeName = "type", Namespace = Constants.Soap.Namespaces.Xsi)]
    public string? Type { get; set; }

    [XmlElement(Namespace = Constants.Xds.Namespaces.Query)]
    public AdhocQueryRequest? AdhocQueryRequest { get; set; }

    [XmlElement(Namespace = Constants.Xds.Namespaces.Xdsb)]
    public ProvideAndRegisterDocumentSetRequestType? ProvideAndRegisterDocumentSetRequest { get; set; }

    [XmlElement(Namespace = Constants.Xds.Namespaces.Query)]
    public AdhocQueryResponse? AdhocQueryResponse { get; set; }

    [XmlElement(Namespace = Constants.Xds.Namespaces.Xdsb)]
    public RetrieveDocumentSetResponseType? RetrieveDocumentSetResponse { get; set; }

    [XmlElement(ElementName = "RegistryResponse", Namespace = Constants.Xds.Namespaces.Rs)]
    public RegistryResponseType? RegistryResponse { get; set; }

    [XmlElement(Namespace = Constants.Soap.Namespaces.SoapEnvelope)]
    public Fault? Fault { get; set; }
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
    public Subcode? Subcode { get; set; }
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
