using System.Xml;
using System.Xml.Serialization;
using XcaXds.Commons;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Services;

namespace XcaXds.Source;

[XmlRoot("Registry")]
public class DocumentRegistry
{
    public DocumentRegistry()
    {
        RegistryObjectList = [];
    }

    [XmlElement("RegistryPackage", typeof(RegistryPackageType), Namespace = Constants.Soap.Namespaces.Rim)]
    [XmlElement("ExtrinsicObject", typeof(ExtrinsicObjectType), Namespace = Constants.Soap.Namespaces.Rim)]
    [XmlElement("Association", typeof(AssociationType), Namespace = Constants.Soap.Namespaces.Rim)]
    public List<IdentifiableType> RegistryObjectList { get; set; }
}

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

    public DocumentRegistry? GetDocumentRegistryContent()
    {
        CheckIfRegistryFileExists();
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);
        var registryFileContent = File.ReadAllText(_registryFile);

        try
        {
            var serializer = new XmlSerializer(typeof(DocumentRegistry));
            using (var stringReader = new StringReader(registryFileContent))
            {
                var registryObject = (DocumentRegistry)serializer.Deserialize(stringReader);
                return registryObject;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Serialization error in registry\n{ex.Message}");
        }
    }

    public SoapRequestResult<string> UpdateDocumentRegistry(DocumentRegistry registryContent)
    {
        CheckIfRegistryFileExists();
        var soapXml = new SoapXmlSerializer(XmlSettings.Soap);
        var srr = new SoapRequestResult<string>();
        try
        {
            var registrySerializerResult = soapXml.SerializeSoapMessageToXmlString(registryContent);
            if (registrySerializerResult.IsSuccess)
            {
                if (File.Exists(_registryFile))
                {
                    File.WriteAllText(_registryFile, registrySerializerResult.Content);
                    return srr.Success("Updated OK");
                }
                return srr.Fault("Registry file not foud");
            }
            return srr.Fault(registrySerializerResult.Content);
        }
        catch (Exception ex)
        {
            return srr.Fault(ex.Message);
        }

    }

    private void CheckIfRegistryFileExists()
    {
        var sxmls = new SoapXmlSerializer(XmlSettings.Soap);
        var emptyRegistryPlaceHolder = new DocumentRegistry() { RegistryObjectList = [] };

        if (!Directory.Exists(_registryPath))
        {
            Directory.CreateDirectory(_registryPath);
        }

        if (!File.Exists(_registryFile))
        {
            File.Create(_registryFile).Close();
            var content = sxmls.SerializeSoapMessageToXmlString(emptyRegistryPlaceHolder);
            try
            {
                File.WriteAllText(_registryFile, content.Content);

            }
            catch (Exception ex)
            {

                throw;
            }
        }
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
