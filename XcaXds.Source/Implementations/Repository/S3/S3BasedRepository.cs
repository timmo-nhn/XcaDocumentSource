using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Shared.Extensions;

namespace XcaXds.Source.Implementations.Repository.S3;

public class S3BasedRepository : IRepository
{
    private readonly ApplicationConfig _appConfig;
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    private static readonly Regex SafeFileNameRegex = new(@"^[a-zA-Z0-9\-_\.^]+$", RegexOptions.Compiled);
    private static readonly Regex SafeCharacters = new(@"[^a-zA-Z0-9\-_\.^]+", RegexOptions.Compiled);

    public S3BasedRepository(ApplicationConfig appConfig, IConfiguration configuration)
    {
        _appConfig = appConfig;
        _bucketName = GetRequiredConfigurationValue(configuration, "S3:Bucket", "S3__Bucket", "S3_BUCKET");

        var endpoint = GetOptionalConfigurationValue(configuration, "S3:Endpoint", "S3__Endpoint", "S3_ENDPOINT");
        var region = GetOptionalConfigurationValue(configuration, "S3:Region", "S3__Region", "S3_REGION") ?? "eu-west-1";
        var accessKey = GetOptionalConfigurationValue(configuration, "S3:AccessKey", "S3__AccessKey", "S3_ACCESS_KEY");
        var secretKey = GetOptionalConfigurationValue(configuration, "S3:SecretKey", "S3__SecretKey", "S3_SECRET_KEY");
        var forcePathStyle = bool.TryParse(GetOptionalConfigurationValue(configuration, "S3:ForcePathStyle", "S3__ForcePathStyle", "S3_FORCE_PATH_STYLE"), out var parsedForcePathStyle) && parsedForcePathStyle;

        var s3Config = new AmazonS3Config()
        {
            ForcePathStyle = forcePathStyle
        };

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(region);
        }
        else
        {
            s3Config.ServiceURL = endpoint;
            s3Config.AuthenticationRegion = region;
        }

        _s3Client = string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey)
            ? new AmazonS3Client(s3Config)
            : new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), s3Config);
    }

    public byte[]? Read(string documentUniqueId)
    {
        if (!IsValidIdentifier(documentUniqueId, out _))
            return null;

        var key = FindKeyForDocumentId(documentUniqueId);
        if (string.IsNullOrWhiteSpace(key))
            return null;

        using var getResponse = _s3Client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        }).GetAwaiter().GetResult();

        using var memoryStream = new MemoryStream();
        getResponse.ResponseStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public OperationResponse Write(string documentId, byte[] data, string? patientId = null)
    {
        documentId = SafeCharacters.Replace(documentId, "");
        patientId = SafeCharacters.Replace(patientId ?? "", "");

        if (!IsValidIdentifier(documentId, out var invalidDocumentCharacters))
            return OperationResponse.Failure($"Invalid Document ID {documentId}, Invalid characters {invalidDocumentCharacters}");

        if (!IsValidIdentifier(patientId, out var invalidPatientCharacters))
            return OperationResponse.Failure($"Invalid Patient ID {patientId}, Invalid characters {invalidPatientCharacters}");

        try
        {
            var key = $"{_appConfig.RepositoryUniqueId}/{patientId}/{documentId.NoUrn()}";
            using var stream = new MemoryStream(data);

            _s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream
            }).GetAwaiter().GetResult();

            return OperationResponse.Success($"Document written to s3://{_bucketName}/{key}");
        }
        catch (AmazonS3Exception ex)
        {
            return OperationResponse.Failure($"Failed to write document '{documentId}' to S3: {ex.Message}");
        }
    }

    public OperationResponse Delete(string? documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            return OperationResponse.Failure("No Document ID provided");

        documentId = SafeCharacters.Replace(documentId, "");

        if (!IsValidIdentifier(documentId, out var invalidCharacters))
            return OperationResponse.Failure($"Invalid Document ID {documentId}, Invalid characters {invalidCharacters}");

        var key = FindKeyForDocumentId(documentId);
        if (string.IsNullOrWhiteSpace(key))
            return OperationResponse.Failure("Document not found");

        try
        {
            _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            }).GetAwaiter().GetResult();

            return OperationResponse.Success($"Document {documentId} deleted successfully");
        }
        catch (AmazonS3Exception ex)
        {
            return OperationResponse.Failure($"Failed to delete document '{documentId}' from S3: {ex.Message}");
        }
    }

    public bool SetNewOid(string repositoryUniqueId, out string? oldId)
    {
        oldId = _appConfig.RepositoryUniqueId;
        return false;
    }

    private string? FindKeyForDocumentId(string documentUniqueId)
    {
        var normalizedDocumentId = documentUniqueId.NoUrn();
        var prefix = $"{_appConfig.RepositoryUniqueId}/";
        string? continuationToken = null;

        do
        {
            var listResponse = _s3Client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = prefix,
                ContinuationToken = continuationToken
            }).GetAwaiter().GetResult();

            var key = listResponse.S3Objects?
                .Select(obj => obj.Key)
                .FirstOrDefault(existingKey => Path.GetFileName(existingKey) == normalizedDocumentId);

            if (!string.IsNullOrWhiteSpace(key))
                return key;

            continuationToken = listResponse.IsTruncated == true ? listResponse.NextContinuationToken : null;
        }
        while (!string.IsNullOrWhiteSpace(continuationToken));

        return null;
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

    private static bool IsValidIdentifier(string input, out string invalidCharacters)
    {
        invalidCharacters = string.Empty;
        if (string.IsNullOrEmpty(input))
            return false;

        var matches = SafeFileNameRegex.Matches(input);
        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                invalidCharacters += match.Value;
            }
        }

        return string.IsNullOrEmpty(invalidCharacters);
    }
}
