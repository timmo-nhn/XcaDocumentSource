using System.Xml;
using System.Xml.Serialization;
using XcaXds.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;

namespace XcaXds.Source;

[XmlRoot("Registry")]
public class RegistryWrapper
{
    internal string _registryPath;
    internal string _registryFile;

    public RegistryWrapper()
    {
        string baseDirectory = AppContext.BaseDirectory;
        _registryPath = Path.Combine(baseDirectory, "..", "..", "..", "..", "XdsDocumentSource", "Registry");
        _registryFile = Path.Combine(_registryPath, "Registry.xml");
    }

    [XmlElement("RegistryPackage", typeof(RegistryPackageType), Namespace = Constants.Soap.Namespaces.Rim)]
    [XmlElement("ExtrinsicObject", typeof(ExtrinsicObjectType), Namespace = Constants.Soap.Namespaces.Rim)]
    [XmlElement("Association", typeof(AssociationType), Namespace = Constants.Soap.Namespaces.Rim)]
    public List<IdentifiableType> RegistryObjectList { get; set; }

    public RegistryWrapper? GetDocumentRegistryContent()
    {
        if (!Directory.Exists(_registryPath))
        {
            Directory.CreateDirectory(_registryPath);
        }

        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);
        var emptyRegistryPlaceHolder = new RegistryWrapper() { RegistryObjectList = [] };

        if (!File.Exists(_registryFile))
        {
            File.WriteAllText(_registryFile, sxmls.SerializeSoapMessageToXmlString(emptyRegistryPlaceHolder));
        }

        var registryFileContent = File.ReadAllText(_registryFile);

        if (string.IsNullOrWhiteSpace(registryFileContent))
        {
            File.WriteAllText(_registryFile, sxmls.SerializeSoapMessageToXmlString(emptyRegistryPlaceHolder));
            registryFileContent = File.ReadAllText(_registryFile);
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
        var soapXml = new SoapXmlSerializer(XmlSettings.Soap);
        var registryObjectString = soapXml.SerializeSoapMessageToXmlString(registryContent);
        if (File.Exists(_registryFile))
        {
            File.WriteAllText(_registryFile, registryObjectString);
            return true;
        }
        return false;
    }

    //public RegistryWrapper? GetRegistryContentForPatient(string patientId)
    //{
    //    if (!Directory.Exists(_registryPath))
    //    {
    //        Directory.CreateDirectory(_registryPath);
    //    }

    //    if (!File.Exists(_registryFile))
    //    {
    //        File.WriteAllText(_registryFile, "<Registry></Registry>");
    //        return null;
    //    }

    //    var registryFileContent = File.ReadAllText(_registryFile);

    //    if (string.IsNullOrWhiteSpace(registryFileContent))
    //    {
    //        File.WriteAllText(_registryFile, "<Registry></Registry>");
    //        return null;
    //    }
    //    var registryObject = new RegistryWrapper();
    //    var serializer = new XmlSerializer(typeof(RegistryWrapper));
    //    try
    //    {
    //        using (var stringReader = new StringReader(registryFileContent))
    //        {
    //            registryObject = (RegistryWrapper)serializer.Deserialize(stringReader);

    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception($"Serialization error in registry\n{ex.Message}");
    //    }


    //    var patientIdValue = registryObject.ExtrinsicObjects
    //        .SelectMany(eo => eo.ExternalIdentifier)
    //        .FirstOrDefault(ei => ei.IdentificationScheme == Constants.Xds.Uuids.DocumentEntry.PatientId)?
    //        .Value;

    //    var patientRegistryObjects = registryObject.ExtrinsicObjects
    //        .Where(eo => eo.ExternalIdentifier
    //        .Any(ei => ei.Value == patientIdValue))
    //        .ToList();

    //    return new RegistryWrapper() { ExtrinsicObjects = patientRegistryObjects };
    //}
}
