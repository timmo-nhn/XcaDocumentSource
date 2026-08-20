using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Configuration;

namespace XcaXds.Source.Implementations.Repository.S3;

internal sealed class S3StorageConfiguration
{
    public required string RepositoryBucket { get; init; }
    public required string AuditLogDlqBucket { get; init; }
    public required string PolicyBucket { get; init; }
    public required string Region { get; init; }
    public string? Endpoint { get; init; }
    public string? AccessKey { get; init; }
    public string? SecretKey { get; init; }
    public bool ForcePathStyle { get; init; }

    public static S3StorageConfiguration FromConfiguration(IConfiguration configuration)
    {
        var repositoryBucket = GetRequiredConfigurationValue(configuration, "S3:RepositoryBucket", "S3__RepositoryBucket", "S3_REPOSITORY_BUCKET");
        var auditLogDlqBucket = GetRequiredConfigurationValue(configuration, "S3:AuditlogDlqBucket", "S3__AuditlogDlqBucket", "S3_AUDITLOGDLQ_BUCKET");
        var policyBucket = GetRequiredConfigurationValue(configuration, "S3:PolicyBucket", "S3__PolicyBucket", "S3_POLICY_BUCKET");
        var region = GetOptionalConfigurationValue(configuration, "S3:Region", "S3__Region", "S3_REGION") ?? "eu-west-1";
        var endpoint = GetOptionalConfigurationValue(configuration, "S3:Endpoint", "S3__Endpoint", "S3_ENDPOINT");
        var accessKey = GetOptionalConfigurationValue(configuration, "S3:AccessKey", "S3__AccessKey", "S3_ACCESS_KEY");
        var secretKey = GetOptionalConfigurationValue(configuration, "S3:SecretKey", "S3__SecretKey", "S3_SECRET_KEY");
        var forcePathStyle = bool.TryParse(
            GetOptionalConfigurationValue(configuration, "S3:ForcePathStyle", "S3__ForcePathStyle", "S3_FORCE_PATH_STYLE"),
            out var parsedForcePathStyle) && parsedForcePathStyle;

        return new S3StorageConfiguration()
        {
            RepositoryBucket = repositoryBucket,
            PolicyBucket = policyBucket,
            AuditLogDlqBucket = auditLogDlqBucket,
            Region = region,
            Endpoint = endpoint,
            AccessKey = accessKey,
            SecretKey = secretKey,
            ForcePathStyle = forcePathStyle
        };
    }

    public IAmazonS3 CreateClient()
    {
        var s3Config = new AmazonS3Config()
        {
            ForcePathStyle = ForcePathStyle
        };

        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(Region);
        }
        else
        {
            s3Config.ServiceURL = Endpoint;
            s3Config.AuthenticationRegion = Region;
        }

        return string.IsNullOrWhiteSpace(AccessKey) || string.IsNullOrWhiteSpace(SecretKey)
            ? new AmazonS3Client(s3Config)
            : new AmazonS3Client(new BasicAWSCredentials(AccessKey, SecretKey), s3Config);
    }

    public static void EnsureBucketExists(IAmazonS3 s3Client, string bucketName)
    {
        var bucketExists = AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName).GetAwaiter().GetResult();
        if (bucketExists) return;

        try
        {
            s3Client.PutBucketAsync(new PutBucketRequest()
            {
                BucketName = bucketName,
                UseClientRegion = true
            }).GetAwaiter().GetResult();
        }
        catch (AmazonS3Exception ex) when (
            string.Equals(ex.ErrorCode, "BucketAlreadyOwnedByYou", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ex.ErrorCode, "BucketAlreadyExists", StringComparison.OrdinalIgnoreCase))
        {
            // Another process may have created it in the meantime.
        }
    }

    private static string GetRequiredConfigurationValue(IConfiguration configuration, string configKey, string firstEnvironmentKey, string secondEnvironmentKey)
    {
        var value = GetOptionalConfigurationValue(configuration, configKey, firstEnvironmentKey, secondEnvironmentKey);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Missing S3 configuration. Set '{configKey}' in configuration or environment variable '{firstEnvironmentKey}'/'{secondEnvironmentKey}'.");

        return value;
    }

    private static string? GetOptionalConfigurationValue(IConfiguration configuration, string configKey, string firstEnvironmentKey, string secondEnvironmentKey)
    {
        return configuration[configKey]
               ?? Environment.GetEnvironmentVariable(firstEnvironmentKey)
               ?? Environment.GetEnvironmentVariable(secondEnvironmentKey);
    }
}
