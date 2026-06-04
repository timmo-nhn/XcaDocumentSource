namespace XcaXds.Commons.Models.Custom.BusinessLogic;

public sealed class BusinessLogicResult<T>
{
    public BusinessLogicResult(bool ruleApplied, IEnumerable<T> registryObjects, string name)
    {
        RuleApplied = ruleApplied;
        RegistryObjects = registryObjects;
        Name = name;
    }

    public BusinessLogicResult()
    {
    }

    public bool RuleApplied { get; set; }
    public IEnumerable<T>? RegistryObjects { get; set; }
    public string? Name { get; set; }
}