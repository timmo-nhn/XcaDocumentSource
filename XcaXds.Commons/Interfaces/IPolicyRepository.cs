using Abc.Xacml.Policy;

namespace XcaXds.Commons.Interfaces;

public interface IPolicyRepository
{
    public XacmlPolicySet GetAllPolicies();
    public bool AddPolicy(XacmlPolicy xacmlPolicy);
    public bool DeletePolicy(XacmlPolicy xacmlPolicy, string? id);
    public bool UpdatePolicy(XacmlPolicy xacmlPolicy, string policyId);
}
