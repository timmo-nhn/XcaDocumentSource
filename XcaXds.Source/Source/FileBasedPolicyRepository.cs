using Microsoft.Extensions.Logging;
using System.Text.Json;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Shared.Constants;

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

        _policyRepositoryPath = Path.GetFullPath(_policyRepositoryPath);

        Directory.CreateDirectory(_policyRepositoryPath);

        _logger.LogInformation($"Policy repository path: {_policyRepositoryPath}");
    }

    public string GetPolicyRepositoryPath()
    {
        return _policyRepositoryPath;
    }

    public PolicySet GetAllPolicies()
    {
        var policySetDto = new PolicySet()
        {
        };

        lock (_lock)
        {
            ExecuteWithRetry(() =>
            {
                var policyFiles = Directory.GetFiles(_policyRepositoryPath);

                foreach (var policyFilePath in policyFiles)
                {
                    if (IsTemporaryFile(policyFilePath) || Path.GetFileName(policyFilePath).StartsWith(".")) continue;

                    var policyFileContent = File.ReadAllText(policyFilePath);
                    var policyDto = JsonSerializer.Deserialize<AbacPolicy>(policyFileContent, Constants.JsonDefaultOptions.DefaultSettings);
                    if (policyDto?.Id != null)
                    {
                        policySetDto.Policies ??= new();
                        policySetDto.Policies.Add(policyDto);
                    }

                }
            });
        }
        _logger.LogInformation($"Successfully read {policySetDto.Policies?.Count ?? 0} policies from policy repository");
        return policySetDto;
    }

    public bool AddPolicy(AbacPolicy? policyDto)
    {
        if (policyDto == null || string.IsNullOrWhiteSpace(policyDto.Id)) return false;

        var jsonPolicyDto = JsonSerializer.Serialize(policyDto, Constants.JsonDefaultOptions.DefaultSettings);

        lock (_lock)
        {
            ExecuteWithRetry(() =>
            {
                File.WriteAllText(Path.Combine(_policyRepositoryPath, policyDto.Id), jsonPolicyDto);
            });
        }

        return true;
    }

    public bool DeletePolicy(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;

        var filePath = Path.Combine(_policyRepositoryPath, id);

        if (!File.Exists(filePath))
            return false;

        lock (_lock)
        {
            ExecuteWithRetry(() =>
            {
                File.Delete(filePath);
            });
        }

        return true;
    }

    public bool DeleteAllPolicies()
    {
        var policyFiles = Directory.GetFiles(_policyRepositoryPath);

        lock (_lock)
        {
            ExecuteWithRetry(() =>
            {
                foreach (var file in policyFiles)
                {
                    if (Path.GetFileName(file).StartsWith('.')) continue;

                    File.Delete(file);
                }
            });
        }

        return true;
    }

    public bool UpdatePolicy(AbacPolicy? policyDto, string? policyId = null)
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

    private bool IsTemporaryFile(string fileName)
    {
        return fileName.EndsWith("~") || fileName.EndsWith(".tmp", StringComparison.CurrentCultureIgnoreCase) || Path.GetFileName(fileName).StartsWith("~$");
    }

    private void ExecuteWithRetry(Action action, int retries = 3)
    {
        for (int i = 1; i <= retries; i++)
        {
            try
            {
                _logger.LogInformation("Attempt {att}/{max}", i, retries);
                action();
                return;
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx.ToString());
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }
    }
}
