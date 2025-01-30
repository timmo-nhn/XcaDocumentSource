using System.Xml;
using System.Xml.Serialization;

namespace XcaGatewayService.Services;

public class SoapXmlSerializer
{
    private XmlWriterSettings? _xmlWriterSettings;
    public SoapXmlSerializer(XmlWriterSettings xmlSettings)
    {
        _xmlWriterSettings = xmlSettings;
    }
    public SoapXmlSerializer()
    {

    }
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

    public string SerializeSoapMessageToXmlString(object soapElement, XmlWriterSettings? settings = null)
    {
        if (soapElement == null) throw new ArgumentNullException(nameof(soapElement));

        settings ??= _xmlWriterSettings;

        try
        {
            var serializer = new XmlSerializer(soapElement.GetType());

            using (var stringWriter = new StringWriter())
            using (var writer = XmlWriter.Create(stringWriter, settings))
            {
                serializer.Serialize(writer, soapElement);
                return stringWriter.ToString();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to serialize the SOAP element to an XML string.", ex);
        }
    }
}
