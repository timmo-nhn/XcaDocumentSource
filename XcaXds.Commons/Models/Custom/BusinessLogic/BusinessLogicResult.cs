using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Custom.BusinessLogic;

public sealed class BusinessLogicResult
{
    public BusinessLogicResult(bool ruleApplied, IEnumerable<IdentifiableType> registryObjects, string name)
    {
        RuleApplied = ruleApplied;
        RegistryObjects = registryObjects;
        Name = name;
    }

    public BusinessLogicResult()
    {
    }

    public bool RuleApplied { get; set; }
    public IEnumerable<IdentifiableType>? RegistryObjects { get; set; }
    public string? Name { get; set; }
}

public delegate BusinessLogicResult BusinessLogicRule(IEnumerable<IdentifiableType> registryObjects,BusinessLogicParameters businessLogic);