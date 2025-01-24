using System.Text;
using System.Text.Unicode;
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

    public string SerializeSoapMessageToXmlString(object soapElement)
    {
        if (soapElement == null) throw new ArgumentNullException(nameof(soapElement));

        try
        {
            using (var stringWriter = new StringWriter())
            {
                var serializer = new XmlSerializer(soapElement.GetType());
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
