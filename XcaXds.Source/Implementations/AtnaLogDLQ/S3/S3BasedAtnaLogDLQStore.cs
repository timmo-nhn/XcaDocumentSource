using Amazon.S3;
using Amazon.S3.Model;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Interfaces;
using XcaXds.Commons.Models.Custom;
using XcaXds.Source.Implementations.Repository.S3;

namespace XcaXds.Source.Implementations.AtnaLogDLQ.S3;

public class S3BasedAtnaLogDLQStore : IAtnaLogDLQStore
{
    private readonly ILogger<S3BasedAtnaLogDLQStore> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _atnalogPrefix = "atnaLogDlq";
    private readonly object _lock = new object();

    public S3BasedAtnaLogDLQStore(ILogger<S3BasedAtnaLogDLQStore> logger, IConfiguration configuration)
    {
        _logger = logger;
        var s3Configuration = S3StorageConfiguration.FromConfiguration(configuration);
        _bucketName = s3Configuration.AtnaLogDlqBucket;
        _s3Client = s3Configuration.CreateClient();
        S3StorageConfiguration.EnsureBucketExists(_s3Client, _bucketName);
    }

    public void DeleteLatestEvent()
    {
        using var getResponse = GetLatestObject();

        if (getResponse == null) return;

        var key = getResponse.Key;

        ExecuteWithRetry(() =>
        {
            _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            }).GetAwaiter().GetResult();
        });
    }

    public AuditEvent? GetLatestEvent()
    {
        using var getResponse = GetLatestObject();

        if (getResponse == null || getResponse.ResponseStream == null) return null;

        _logger.LogInformation("Got item {key} from DLQ", getResponse.Key);

        using var reader = new StreamReader(getResponse.ResponseStream);
        string? json = null;

        // Unescape unicode encoding from S3 storage (/u0022 and such)
        var auditEvent = GlobalExtensions.TryThis(() =>
        {
            var deserializer = new FhirJsonDeserializer();
            var jsonUnescaped = Regex.Unescape(reader.ReadToEnd()).Trim('"');
            return deserializer.Deserialize<AuditEvent>(jsonUnescaped);
        }, out var success, out var exception);

        if (!success && exception != null)
        {
            _logger.LogError("Error while unescaping event with Id: {id}, deleting\nExceptionType (Debug log will show full exception): {ex}", getResponse.Key, exception.GetType().Name);
            _logger.LogDebug("Full exception\n {ex}", exception.ToString());
            DeleteLatestEvent();
        }

        return auditEvent;
    }

    public OperationResponse StoreAuditEvent(AuditEvent auditEvent)
    {
        ArgumentNullException.ThrowIfNull(auditEvent, nameof(auditEvent));
        var serializer = new FhirJsonSerializer();

        var key = BuildKey(auditEvent.Id);
        var payload = JsonSerializer.Serialize(serializer.SerializeToString(auditEvent));

        ExecuteWithRetry(() =>
        {
            _s3Client.PutObjectAsync(new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = key,
                ContentBody = payload,
                ContentType = "application/json"
            }).GetAwaiter().GetResult();
        });

        _logger.LogInformation("Successfully stored AuditEvent in DLQ");
        return OperationResponse.Success("Successfully stored AuditEvent");
    }

    public GetObjectResponse? GetLatestObject()
    {
        GetObjectResponse? getResponse = null;
        string? continuationToken = null;

        do
        {
            var listResponse = _s3Client.ListObjectsV2Async(new ListObjectsV2Request()
            {
                BucketName = _bucketName,
                Prefix = $"{_atnalogPrefix}/",
                ContinuationToken = continuationToken

            }).GetAwaiter().GetResult();

            var firstItem = listResponse.S3Objects?.FirstOrDefault();

            if (firstItem == null) return null;

            // Dispose the response from the previous page before fetching the next one.
            // The final response is returned undisposed - the caller owns it.
            getResponse?.Dispose();

            getResponse = _s3Client.GetObjectAsync(new GetObjectRequest()
            {
                BucketName = _bucketName,
                Key = firstItem.Key
            }).GetAwaiter().GetResult();

            continuationToken = listResponse.IsTruncated == true ? listResponse.NextContinuationToken : null;

        } while (!string.IsNullOrWhiteSpace(continuationToken));

        return getResponse;
    }

    private void ExecuteWithRetry(Action action, int retries = 3)
    {
        lock (_lock)
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
    }

    private string BuildKey(string? id)
    {
        // Id will probably never be null, but we also dont care because this is a FIFO queue
        return $"{_atnalogPrefix}/{id ?? Guid.NewGuid().ToString()}";
    }

    private static bool IsNoSuchBucket(AmazonS3Exception ex)
    {
        return string.Equals(ex.ErrorCode, "NoSuchBucket", StringComparison.OrdinalIgnoreCase)
               || ex.StatusCode == System.Net.HttpStatusCode.NotFound;
    }

}
