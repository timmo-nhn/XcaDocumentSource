using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace XcaXds.Commons.Services;

public enum XmlSettings
{
    Soap
}

public class SoapXmlSerializer
{
    private XmlWriterSettings? _xmlWriterSettings;
    public SoapXmlSerializer(XmlWriterSettings xmlSettings)
    {
        _xmlWriterSettings = xmlSettings;
    }
    public SoapXmlSerializer(XmlSettings xmlSettings)
    {
        _xmlWriterSettings = new XmlWriterSettings() { Indent = true, Encoding = Encoding.UTF8, OmitXmlDeclaration = true };
    }
    public SoapXmlSerializer()
    {
        
    }
    public async Task<T> DeserializeSoapMessageAsync<T>(Stream xmlStream)
    {
        var serializer = new XmlSerializer(typeof(T));

        using (var streamReader = new StreamReader(xmlStream))
        {
            var xmlContent = await streamReader.ReadToEndAsync();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("s", Constants.Soap.Namespaces.SoapEnvelope);
            namespaceManager.AddNamespace("p7", Constants.Soap.Namespaces.Xsi);

            var bodyElement = xmlDoc.SelectSingleNode("//s:Body", namespaceManager);


            if (bodyElement == null)
            {
                bodyElement = xmlDoc.SelectSingleNode("//Body");
            }

            if (bodyElement != null && bodyElement.Attributes["type", "http://www.w3.org/2001/XMLSchema-instance"] != null)
            {
                bodyElement.Attributes.RemoveNamedItem("type", "http://www.w3.org/2001/XMLSchema-instance");
                Console.WriteLine("Removed 'type' attribute.");
            }

            var modifiedXmlContent = xmlDoc.OuterXml;

            using (var stringReader = new StringReader(modifiedXmlContent))
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
