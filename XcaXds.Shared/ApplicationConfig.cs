
public class ApplicationConfig
{
    public string? ApiKey { get; set; } = string.Empty;
    public bool ApiKeyEnabled { get; set; }
    public string? AtnaLogExporterEndpoint { get; set; } = string.Empty;
    public bool BypassPolicyEnforcementPoint { get; set; }
    public bool ClamAvEnabled { get; set; }
    public string? ClamAvEndpoint { get; set; } = string.Empty;
    public bool CanOverrideValidateSamlTokenIntegrityWithQueryParameter { get; set; }
    public int DocumentUploadSizeLimitKb { get; set; }
    public string? FriendlyName { get; set; }
    public string HomeCommunityId { get; set; } = "2.16.578.1.12.4.5.100.1.1";
    public string? HostName { get; set; } = string.Empty;
    public bool IgnorePEPForLocalhostRequests { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public bool MultipartResponseForIti43AndIti39 { get; set; }
    public int TimeoutInSeconds { get; set; }
    public string RepositoryUniqueId { get; set; } = "2.16.578.1.12.4.5.100.1.1.2";
    public bool SamlValidateSamlTokenIntegrity { get; set; }
    public string[] SamlValidationCertificatesRaw { get; set; } = [];
    public string[] SamlValidationSigningCertificateUrls { get; set; } = [];
    public string[] SamlValidationValidAudiences { get; set; } = [];
    public string[] SamlValidationValidIssuers { get; set; } = [];
    public bool WrapRetrievedDocumentInCda { get; set; }
}
