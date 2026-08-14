using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom.PolicyDtos;
using XcaXds.Shared;
using XcaXds.Source.Implementations.Repository.S3;

namespace XcaXds.Source.Implementations.PolicyRepository.S3;

public class S3BasedPolicyRepository : IPolicyRepository
{
    private readonly object _lock = new();
    private readonly ILogger<S3BasedPolicyRepository> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _policyPrefix;

    private static readonly Regex SafePolicyIdRegex = new(@"^[a-zA-Z0-9\-_\.]+$", RegexOptions.Compiled);
    private static readonly Regex UnsafePolicyIdCharacters = new(@"[^a-zA-Z0-9\-_\.]+", RegexOptions.Compiled);

    public S3BasedPolicyRepository(ILogger<S3BasedPolicyRepository> logger, IConfiguration configuration)
    {
        _logger = logger;

        var s3Configuration = S3StorageConfiguration.FromConfiguration(configuration);
        _bucketName = s3Configuration.PolicyBucket;
        _s3Client = s3Configuration.CreateClient();
        S3StorageConfiguration.EnsureBucketExists(_s3Client, _bucketName);

        _policyPrefix = configuration["S3:PolicyPrefix"]
                        ?? Environment.GetEnvironmentVariable("S3__PolicyPrefix")
                        ?? Environment.GetEnvironmentVariable("S3_POLICY_PREFIX")
                        ?? "policies";
        _policyPrefix = _policyPrefix.Trim().Trim('/');
    }

    public string GetPolicyRepositoryPath()
    {
        return $"s3://{_bucketName}/{_policyPrefix}";
    }

    public PolicySet GetAllPolicies()
    {
        var policySet = new PolicySet();
        var policies = new List<AbacPolicy>();

        lock (_lock)
        {
            ExecuteWithRetry(() =>
            {
                string? continuationToken = null;
                do
                {
                    var listResponse = _s3Client.ListObjectsV2Async(new ListObjectsV2Request
                    {
                        BucketName = _bucketName,
                        Prefix = $"{_policyPrefix}/",
                        ContinuationToken = continuationToken
                    }).GetAwaiter().GetResult();

                    foreach (var item in listResponse.S3Objects ?? [])
                    {
                        if (string.IsNullOrWhiteSpace(item.Key) || item.Key.EndsWith("/", StringComparison.Ordinal))
                            continue;

                        using var getResponse = _s3Client.GetObjectAsync(new GetObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = item.Key
                        }).GetAwaiter().GetResult();

                        using var reader = new StreamReader(getResponse.ResponseStream);
                        var json = reader.ReadToEnd();
                        var policy = JsonSerializer.Deserialize<AbacPolicy>(json, Constants.JsonDefaultOptions.DefaultSettings);

                        if (policy?.Id != null)
                        {
                            policies.Add(policy);
                        }
                    }

                    continuationToken = listResponse.IsTruncated == true ? listResponse.NextContinuationToken : null;
                }
                while (!string.IsNullOrWhiteSpace(continuationToken));
            });
        }

        policySet.Policies = policies;
        _logger.LogInformation("Successfully read {count} policies from S3 policy repository", policies.Count);
        return policySet;
    }

    public bool AddPolicy(AbacPolicy? policyDto)
    {
        if (policyDto == null || string.IsNullOrWhiteSpace(policyDto.Id))
            return false;

        var sanitizedPolicyId = UnsafePolicyIdCharacters.Replace(policyDto.Id, "");
        if (!IsValidPolicyId(sanitizedPolicyId))
            return false;

        var key = BuildPolicyKey(sanitizedPolicyId);
        var payload = JsonSerializer.Serialize(policyDto, Constants.JsonDefaultOptions.DefaultSettings);

        lock (_lock)
        {
            ExecuteWithRetry(() =>
            {
                _s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    ContentBody = payload,
                    ContentType = "application/json"
                }).GetAwaiter().GetResult();
            });
        }

        return true;
    }

    public bool DeletePolicy(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        var sanitizedPolicyId = UnsafePolicyIdCharacters.Replace(id, "");
        if (!IsValidPolicyId(sanitizedPolicyId))
            return false;

        var key = BuildPolicyKey(sanitizedPolicyId);

        lock (_lock)
        {
            ExecuteWithRetry(() =>
            {
                _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                }).GetAwaiter().GetResult();
            });
        }

        return true;
    }

    public bool DeleteAllPolicies()
    {
        lock (_lock)
        {
            ExecuteWithRetry(() =>
            {
                var keysToDelete = ListAllPolicyKeys();
                if (keysToDelete.Count == 0)
                    return;

                foreach (var chunk in keysToDelete.Chunk(1000))
                {
                    var deleteRequest = new DeleteObjectsRequest
                    {
                        BucketName = _bucketName,
                        Objects = chunk.Select(key => new KeyVersion { Key = key }).ToList()
                    };

                    _s3Client.DeleteObjectsAsync(deleteRequest).GetAwaiter().GetResult();
                }
            });
        }

        return true;
    }

    public bool UpdatePolicy(AbacPolicy? policyDto, string? policyId = null)
    {
        if (policyDto == null || string.IsNullOrWhiteSpace(policyDto.Id))
            return false;

        if (!string.IsNullOrWhiteSpace(policyId) && !string.Equals(policyId, policyDto.Id, StringComparison.Ordinal))
        {
            DeletePolicy(policyId);
        }

        return AddPolicy(policyDto);
    }

    private List<string> ListAllPolicyKeys()
    {
        var keys = new List<string>();
        string? continuationToken = null;

        do
        {
            var listResponse = _s3Client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = $"{_policyPrefix}/",
                ContinuationToken = continuationToken
            }).GetAwaiter().GetResult();

            keys.AddRange(listResponse.S3Objects
                .Where(item => !string.IsNullOrWhiteSpace(item.Key) && !item.Key.EndsWith("/", StringComparison.Ordinal))
                .Select(item => item.Key));

            continuationToken = listResponse.IsTruncated == true ? listResponse.NextContinuationToken : null;
        }
        while (!string.IsNullOrWhiteSpace(continuationToken));

        return keys;
    }

    private string BuildPolicyKey(string policyId)
    {
        return $"{_policyPrefix}/{policyId}";
    }

    private static bool IsValidPolicyId(string policyId)
    {
        return !string.IsNullOrWhiteSpace(policyId) && SafePolicyIdRegex.IsMatch(policyId);
    }

    private void ExecuteWithRetry(Action action, int retries = 3)
    {
        for (var attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                _logger.LogInformation("RetryLogic Attempt {attempt}/{maxAttempts}", attempt, retries);
                action();
                return;
            }
            catch (AmazonS3Exception ex) when (IsNoSuchBucket(ex))
            {
                _logger.LogWarning("S3 policy bucket '{bucketName}' does not exist. Continuing with an empty policy set.", _bucketName);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "S3 policy repository operation failed on attempt {attempt}/{maxAttempts}", attempt, retries);
                if (attempt == retries) throw;
                Thread.Sleep(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
            }
        }
    }

    private static bool IsNoSuchBucket(AmazonS3Exception ex)
    {
        return string.Equals(ex.ErrorCode, "NoSuchBucket", StringComparison.OrdinalIgnoreCase)
               || ex.StatusCode == System.Net.HttpStatusCode.NotFound;
    }
}