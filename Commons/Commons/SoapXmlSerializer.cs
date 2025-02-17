﻿using System.Text;
using System.Xml;
using System.Xml.Serialization;
using XcaXds.Commons.Extensions;

namespace XcaXds.Commons.Services;

public enum XmlSettings
{
    Soap
}

public class SoapXmlSerializerResult
{
    public string? Content { get; set; }
    public bool IsSuccess { get; set; }
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

    public async Task<T> DeserializeSoapMessageAsync<T>(string xmlString)
    {
        var byteArray = Encoding.UTF8.GetBytes(xmlString);
        var memStream = new MemoryStream(byteArray);
        return await DeserializeSoapMessageAsync<T>(memStream);
    }
     
    public async Task<T> DeserializeSoapMessageAsync<T>(Stream xmlStream)
    {
        var serializer = new XmlSerializer(typeof(T));

        using (var streamReader = new StreamReader(xmlStream))
        {
            var xmlContent = await streamReader.ReadToEndAsync();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            // Having a "type" attribute on the <Body> tag causes an exception when deserializing
            // <s:Body p7:type="RegistryStoredQueryRequest" xmlns:p7="http://www.w3.org/2001/XMLSchema-instance">
            // so strip it away before deserializing, as its not used
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

    public SoapXmlSerializerResult SerializeSoapMessageToXmlString(object soapElement, XmlWriterSettings? settings = null)
    {
        if (soapElement == null) throw new ArgumentNullException(nameof(soapElement));

        settings ??= _xmlWriterSettings;
        var serializer = new XmlSerializer(soapElement.GetType());

        try
        {
            using (var stringWriter = new StringWriter())
            using (var writer = XmlWriter.Create(stringWriter, settings))
            {
                serializer.Serialize(writer, soapElement);
                return new SoapXmlSerializerResult() { Content = stringWriter.ToString(), IsSuccess = true };
            }
        }
        catch (Exception ex)
        {
            var soapFault = SoapExtensions.CreateSoapFault("Serialization Error", faultReason: ex.Message, detail: ex.InnerException?.Message);

            try
            {
                var faultSerializer = new XmlSerializer(soapFault.Value.GetType());

                using (var stringWriter = new StringWriter())
                using (var writer = XmlWriter.Create(stringWriter, settings))
                {
                    faultSerializer.Serialize(writer, soapFault.Value);
                    return new SoapXmlSerializerResult() { Content = stringWriter.ToString(), IsSuccess = false };
                }
            }
            catch
            {
                return new SoapXmlSerializerResult()
                {
                    Content = $"<SoapFault><Reason>Serialization Failed {ex.Message}</Reason></SoapFault>",
                    IsSuccess = false
                };
            }
        }
    }
}
