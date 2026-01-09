using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Custom.BusinessLogic;

public sealed class BusinessLogicResult
{
    public BusinessLogicResult(bool success, IEnumerable<IdentifiableType> registryObjects, string name)
    {
        Success = success;
        RegistryObjects = registryObjects;
        Name = name;
    }

    public BusinessLogicResult()
    {
    }

    public bool Success { get; set; }
    public IEnumerable<IdentifiableType>? RegistryObjects { get; set; }
    public string? Name { get; set; }
}

public delegate BusinessLogicResult BusinessLogicRule(IEnumerable<IdentifiableType> registryObjects,BusinessLogicParameters businessLogic);