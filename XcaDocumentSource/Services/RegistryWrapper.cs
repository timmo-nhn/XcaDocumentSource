using Microsoft.Win32;
using System.Xml;
using System.Xml.Serialization;
using XcaGatewayService.Extensions;
using XcaGatewayService.Models.Soap;

namespace XcaGatewayService.Services
{
    [XmlRoot("Registry")]
    public class RegistryWrapper
    {
        private string _registryPath;
        private string _registryFile;

        public RegistryWrapper()
        {
            string baseDirectory = AppContext.BaseDirectory;
            _registryPath = Path.Combine(baseDirectory, "..", "..", "..", "Registry");
            _registryFile = Path.Combine(_registryPath, "Registry.xml");
        }

        [XmlElement("ExtrinsicObject")]
        public List<ExtrinsicObjectType> ExtrinsicObjects { get; set; }

        public RegistryWrapper? GetDocumentRegistryContent()
        {
            if (!Directory.Exists(_registryPath))
            {
                Directory.CreateDirectory(_registryPath);
            }

            if (!File.Exists(_registryFile))
            {
                File.WriteAllText(_registryFile, "<Registry></Registry>");
            }

            var registryFileContent = File.ReadAllText(_registryFile);

            if (string.IsNullOrWhiteSpace(registryFileContent))
            {
                File.WriteAllText(_registryFile, "<Registry></Registry>");
            }
            try
            {
                var serializer = new XmlSerializer(typeof(RegistryWrapper));
                using (var stringReader = new StringReader(registryFileContent))
                {
                    var registryObject = (RegistryWrapper)serializer.Deserialize(stringReader);
                    return registryObject;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Serialization error in registry\n{ex.Message}");
            }

        }

        public bool UpdateDocumentRegistry(RegistryWrapper registryContent)
        {
            var soapXml = new SoapXmlSerializer();
            var registryObjectString = soapXml.SerializeSoapMessageToXmlString(registryContent, new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true, IndentChars = "  " });
            if (File.Exists(_registryFile))
            {
                File.WriteAllText(_registryFile, registryObjectString);
                return true;
            }
            return false;
        }
    }
}
