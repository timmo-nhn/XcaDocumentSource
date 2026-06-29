namespace XcaXds.Shared.ConfigBinder;

public static class ConfigBinder
{
    public static ApplicationConfig BindKeyValueEnvironmentVariablesToXdsConfiguration(Dictionary<string, string> xdsConfigEnvVars)
    {
        // Keep each configuration value retrieval separate for better error handling and debugging, especially when parsing different data types (e.g., bool, int, string).
        var appConfig = new ApplicationConfig();

        appConfig.SamlValidateSamlTokenIntegrity = bool.Parse(xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__ValidateSamlTokenIntegrity") ?? "false");
        appConfig.CanOverrideValidateSamlTokenIntegrityWithQueryParameter = bool.Parse(xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__CanOverrideValidateSamlTokenIntegrityWithQueryParameter") ?? "false");
        appConfig.TimeoutInSeconds = int.Parse(xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__TimeoutInSeconds") ?? "0");
        appConfig.HomeCommunityId = xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__HomeCommunityId") ?? throw new ArgumentNullException("Missing HomeCommunityId");
        appConfig.RepositoryUniqueId = xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__RepositoryUniqueId") ?? throw new ArgumentNullException("Missing RepositoryuniqueId");
        appConfig.DocumentUploadSizeLimitKb = int.Parse(xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__DocumentUploadSizeLimitKb") ?? "0");
        appConfig.BypassPolicyEnforcementPoint = bool.Parse(xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__BypassPolicyEnforcementPoint") ?? "false");
        appConfig.WrapRetrievedDocumentInCda = bool.Parse(xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__WrapRetrievedDocumentInCda") ?? "true");
        appConfig.MultipartResponseForIti43AndIti39 = bool.Parse(xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__MultipartResponseForIti43AndIti39") ?? "true");
        appConfig.SamlValidationCertificatesRaw = xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__CertificatesRaw")?.Split(";") ?? [];
        appConfig.SamlValidationSigningCertificateUrls = xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__SigningCertificateUrls")?.Split(";") ?? [];
        appConfig.SamlValidationValidAudiences = xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__ValidAudiences")?.Split(";") ?? [];
        appConfig.SamlValidationValidIssuers = xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__ValidIssuers")?.Split(";") ?? [];
        appConfig.AtnaLogExporterEndpoint = xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__AtnaLogExporterEndpoint");
        appConfig.ClamAvEndpoint = xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__ClamavEndpoint");
        appConfig.HostName = xdsConfigEnvVars.GetValueOrDefault("HOSTNAME");
        appConfig.ApiKey = xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__ApiKey");
        appConfig.ApiKeyEnabled = bool.Parse(xdsConfigEnvVars.GetValueOrDefault("XdsConfiguration__ApiKeyEnabled") ?? "true");

        return appConfig;
    }
}