using Abc.Xacml;
using Abc.Xacml.Policy;
using Abc.Xacml.Runtime;
using System.Xml;
using XcaXds.Commons;

namespace XcaXds.Source.Source;

public class FileBasedPolicyRepository : IXacmlPolicyRepository
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


    public XacmlPolicy RequestPolicy(Uri policyId)
    {

        throw new NotImplementedException();
    }

    public XacmlPolicySet RequestPolicySet(Uri policySetId)
    {
        throw new NotImplementedException();
    }
}
