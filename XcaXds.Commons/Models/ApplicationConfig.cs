
public class ApplicationConfig
{
    public int TimeoutInSeconds { get; set; }
    public bool WrapRetrievedDocumentInCda { get; set; }
    public bool MultipartResponseForIti43AndIti39 { get; set; }
    public string HomeCommunityId { get; set; } = "2.16.578.1.12.4.5.100.1.1";
    public string RepositoryUniqueId { get; set; } = "2.16.578.1.12.4.5.100.1.1.2";
    public bool IgnorePEPForLocalhostRequests { get; set; }
    public bool BypassPolicyEnforcementPoint { get; set; }
    public int DocumentUploadSizeLimitKb { get; set; }
    public bool ValidateSamlTokenIntegrity { get; set; }
    public string HelseidCert { get; set; } = string.Empty;
    public string HelsenorgeCert { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string AtnaLogExporterEndpoint { get; set; } = string.Empty;
}
