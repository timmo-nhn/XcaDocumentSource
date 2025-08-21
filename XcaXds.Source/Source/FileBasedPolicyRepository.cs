using System.Text.Json;
using Abc.Xacml.Policy;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;

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

    public PolicySetDto GetAllPolicies()
    {
        var target = new XacmlTarget();

        var policySetDto = new PolicySetDto()
        {
            CombiningAlgorithm = Constants.Xacml.CombiningAlgorithms.V20_PolicyCombining_PermitOverrides,
        };

        lock (_lock)
        {
            var policyFiles = Directory.GetFiles(_policyRepositoryPath);

            foreach (var policyFilePath in policyFiles)
            {
                var policyFileContent = File.ReadAllText(policyFilePath);
                var policyDto = JsonSerializer.Deserialize<PolicyDto>(policyFileContent);
                if (policyDto != null)
                {
                    policySetDto.Policies ??= new();
                    policySetDto.Policies.Add(policyDto);
                }
            }
        }

        return policySetDto;
    }

    public bool AddPolicy(PolicyDto? policyDto)
    {
        if (policyDto == null) return false;

        var jsonPolicyDto = JsonSerializer.Serialize(policyDto, Constants.JsonDefaultOptions.DefaultSettings);

        lock (_lock)
        {
            File.WriteAllText(_policyRepositoryPath + policyDto.Id, jsonPolicyDto);
        }

        return true;
    }

    public bool DeletePolicy(string? id)
    {
        var filePath = _policyRepositoryPath + id;

        if (!File.Exists(filePath))
            return false;

        lock (_lock)
        {
            File.Delete(filePath);
        }
        
        return true;

    }

    public bool UpdatePolicy(PolicyDto? xacmlPolicy, string policyId)
    {

        throw new NotImplementedException();
    }
}
