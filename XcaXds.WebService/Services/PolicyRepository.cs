using Abc.Xacml.Policy;
using Abc.Xacml.Runtime;

namespace XcaXds.WebService.Services;

public class PolicyRepository : IXacmlPolicyRepository
{
    private string _policyRepositoryPath;

    public PolicyRepository()
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
            _policyRepositoryPath = Path.Combine(baseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Registry");
        }

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
