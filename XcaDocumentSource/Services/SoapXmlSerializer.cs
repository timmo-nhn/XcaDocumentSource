using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using XcaDocumentSource.Models.Soap;

namespace XcaDocumentSource.Services;

public class SoapXmlSerializer
{
    public async Task<T> DeserializeSoapMessageAsync<T>(Stream xmlStream)
    {
        var serializer = new XmlSerializer(typeof(T));

        // Wrap the synchronous deserialization in an asynchronous context
        using (var streamReader = new StreamReader(xmlStream))
        {
            var xmlContent = await streamReader.ReadToEndAsync();
            using (var stringReader = new StringReader(xmlContent))
            {
                return (T)serializer.Deserialize(stringReader);
            }
        }
    }
    public SoapEnvelope DeserializeSoapMessageAsync(Stream xmlStream)
    {
        var xmlSerializer = new XmlSerializer(typeof(SoapEnvelope));

        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("soap", "http://www.w3.org/2003/05/soap-envelope");
        namespaces.Add("ns4", "urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0");
        namespaces.Add("ns2", "urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0");

        // Deserialize
        using (var stringReader = new StreamReader(xmlStream))
        {
            var soapEnvelope = (SoapEnvelope)xmlSerializer.Deserialize(stringReader);
            return soapEnvelope;
        }
    }
    public T DeserializeSoapMessage<T>(Stream soapElement) where T : class
    {
        var soapElements = XElement.Load(soapElement);
        return DeserializeSoapMessage<T>(soapElement);
    }

    public string SerializeSoapMessageToXmlString(object soapElement)
    {
        if (soapElement == null) throw new ArgumentNullException(nameof(soapElement));

        try
        {
            var serializer = new XmlSerializer(soapElement.GetType());

            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, soapElement);
                return stringWriter.ToString();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to serialize the SOAP element to an XML string.", ex);
        }
    }
}
