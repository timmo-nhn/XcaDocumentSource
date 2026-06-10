using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Runtime.CompilerServices;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.Services;

public class TerminologyService
{
    private readonly ILogger<TerminologyService> _logger;

    private Dictionary<string, ComprehensiveCodeSystem[]> CodeSystems { get; set; } = [];
    
    public TerminologyService(ILogger<TerminologyService> logger)
    {
        _logger = logger;
    }

    public void AddCodeSystem(string name, ComprehensiveCodeSystem[] codeSystems)
    {
        _logger.LogInformation($"Adding code system {name} with {codeSystems.Length} entries...");
        CodeSystems.Add(name, codeSystems);
        _logger.LogInformation($"Added code system {name} with {codeSystems.Length} entries");
    }

    public ComprehensiveCodeSystem[] GetCodeSystemByKey(string name)
    {
        return CodeSystems[name];
    }

    public ComprehensiveCodeSystem[] GetCodeSystemBySystem(string system)
    {
        return CodeSystems.Values.SelectMany(cs => cs).Where(cs => cs.SystemOid == system).ToArray();
    }

    public KeyValuePair<string, string>? GetValueFromCodeSystem(ComprehensiveCodeSystem[] confidentialityCodeSystems, string inputValue)
    {
        _logger.LogInformation($"Getting value {inputValue} from code systems {string.Join(", ", confidentialityCodeSystems.Select(cc => cc.SystemOid))}");
        var fetchedValue = confidentialityCodeSystems.GetValueSystemOid(inputValue); 
        
        if(fetchedValue != null )
        {
            _logger.LogInformation($"Got value {fetchedValue?.Value} from code system {fetchedValue?.Key}");

            return fetchedValue;
        }
        _logger.LogWarning($"Could not find value {inputValue} in code systems {string.Join(", ", confidentialityCodeSystems.Select(cc => cc.SystemOid))}");
        return null;
    }
}
