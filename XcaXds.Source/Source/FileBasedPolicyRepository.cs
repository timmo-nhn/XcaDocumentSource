using System.Text;
using System.Xml;
using Abc.Xacml;
using Abc.Xacml.Policy;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces;

namespace XcaXds.Source.Source;

public class FileBasedPolicyRepository : IPolicyRepository
{
    private string _policyRepositoryPath;
    private readonly object _lock = new();

    public FileBasedPolicyRepository()
    {
        // When running in a container the path will be different
        var customPath = Environment.GetEnvironmentVariable("POLICY_REPOSITORY_FILE_PATH");
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _policyRepositoryPath = customPath;
        }
        else
        {
            string baseDirectory = AppContext.BaseDirectory;
            _policyRepositoryPath = Path.Combine(baseDirectory, "..", "..", "..", "..", "XcaXds.Source", "PolicyRepository");
        }
    }

    public XacmlPolicySet GetAllPolicies()
    {
        var target = new XacmlTarget();

        var policySet = new XacmlPolicySet(new Uri(Constants.Xacml.CombiningAlgorithms.V20_PolicyCombining_PermitOverrides), target);

        lock (_lock)
        {
            var policyFiles = Directory.GetFiles(_policyRepositoryPath);

            foreach (var policyFilePath in policyFiles)
            {
                var policyFile = File.ReadAllText(policyFilePath);
                using (XmlReader reader = XmlReader.Create(new StringReader(policyFile)))
                {
                    var serialize = new Xacml20ProtocolSerializer();
                    policySet.Policies.Add(serialize.ReadPolicy(reader));
                }
            }
        }

        return policySet;
    }

    public bool AddPolicy(XacmlPolicy xacmlPolicy)
    {
        var serializer = new Xacml20ProtocolSerializer();

        var settings = new XmlWriterSettings()
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        };


        var policyXml = string.Empty;

        try
        {
            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    serializer.WritePolicy(xmlWriter, xacmlPolicy);
                    policyXml = stringWriter.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            return false;
        }

        lock (_lock)
        {
            File.WriteAllText(_policyRepositoryPath + xacmlPolicy.PolicyId.ToString(), policyXml);
        }

        return true;
    }

    public bool DeletePolicy(XacmlPolicy xacmlPolicy, string? id)
    {
        var filePath = _policyRepositoryPath + (id ?? xacmlPolicy.PolicyId.ToString());
        if (!File.Exists(filePath))
        {
            return false;
        }
        
        File.Delete(filePath);
        return true;
    }

    public bool UpdatePolicy(XacmlPolicy xacmlPolicy, string policyId)
    {

        throw new NotImplementedException();
    }
}
