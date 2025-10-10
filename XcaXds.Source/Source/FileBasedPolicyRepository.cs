using Abc.Xacml.Policy;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;

namespace XcaXds.Source.Source;

public class FileBasedPolicyRepository : IPolicyRepository
{
    private string _policyRepositoryPath;
    private readonly object _lock = new();
    private readonly ILogger<FileBasedPolicyRepository> _logger;
    public FileBasedPolicyRepository(ILogger<FileBasedPolicyRepository> logger)
    {
        _logger = logger;
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
            CombiningAlgorithm = Constants.Xacml.CombiningAlgorithms.V20_PolicyCombining_DenyOverrides,
        };

        lock (_lock)
        {
            var policyFiles = Directory.GetFiles(_policyRepositoryPath);

            foreach (var policyFilePath in policyFiles)
            {
                try
                {
                    var policyFileContent = File.ReadAllText(policyFilePath);
                    var policyDto = JsonSerializer.Deserialize<PolicyDto>(policyFileContent, Constants.JsonDefaultOptions.DefaultSettings);
                    if (policyDto?.Id != null)
                    {
                        // Normalize so OID values arent prefixed with "urn:oid:"
                        policyDto.Subjects?.ForEach(sb => sb.Value = sb.Value?.NoUrn());
                        policyDto.Resources?.ForEach(sb => sb.Value = sb.Value?.NoUrn());

                        policySetDto.Policies ??= new();
                        policySetDto.Policies.Add(policyDto);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error while deserializing Policy with ID: {policyFilePath.Split("/").Last()}");
                    _logger.LogWarning(ex.ToString());
                }
            }
        }
        _logger.LogInformation($"Successfully read {policySetDto.Policies?.Count ?? 0} policies from repository");
        return policySetDto;
    }

    public bool AddPolicy(PolicyDto? policyDto)
    {
        if (policyDto == null) return false;

        var jsonPolicyDto = JsonSerializer.Serialize(policyDto, Constants.JsonDefaultOptions.DefaultSettings);

        lock (_lock)
        {
            File.WriteAllText(Path.Combine(_policyRepositoryPath, policyDto.Id), jsonPolicyDto);
        }

        return true;
    }

    public bool DeletePolicy(string? id)
    {
        var filePath = Path.Combine(_policyRepositoryPath, id);

        if (!File.Exists(filePath))
            return false;

        lock (_lock)
        {
            File.Delete(filePath);
        }
        
        return true;

    }

    public bool UpdatePolicy(PolicyDto policyDto, string? policyId = null)
    {
        if (policyDto == null) return false;
        // FIXME add better update handling stuff?
        DeletePolicy(policyDto.Id);
        AddPolicy(policyDto);
        if (policyId != policyDto.Id)
        {
            // If this is true, it's assumed that the user wants to rename the policy
            
        }

        return true;
    }
}
