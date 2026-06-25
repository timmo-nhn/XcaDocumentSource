namespace XcaXds.Shared.ConfigBinder;

public static class ConfigBinder
{
    public static ApplicationConfig BindKeyValueEnvironmentVariablesToXdsConfiguration(List<KeyValuePair<string, string>> xdsConfigEnvVars)
    {
        // Keep each configuration value retrieval separate for better error handling and debugging, especially when parsing different data types (e.g., bool, int, string).
        var appConfig = new ApplicationConfig();

        appConfig.ValidateSamlTokenIntegrity = bool.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__ValidateSamlTokenIntegrity").Value ?? "false");
		appConfig.CanOverrideValidateSamlTokenIntegrityWithQueryParameter = bool.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__CanOverrideValidateSamlTokenIntegrityWithQueryParameter").Value ?? "false");		
		appConfig.TimeoutInSeconds = int.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__TimeoutInSeconds").Value ?? "0");
        appConfig.HomeCommunityId = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__HomeCommunityId").Value;
        appConfig.RepositoryUniqueId = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__RepositoryUniqueId").Value;
        appConfig.DocumentUploadSizeLimitKb = int.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__DocumentUploadSizeLimitKb").Value ?? "0");
        appConfig.BypassPolicyEnforcementPoint = bool.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__BypassPolicyEnforcementPoint").Value ?? "false");
        appConfig.WrapRetrievedDocumentInCda = bool.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__WrapRetrievedDocumentInCda").Value ?? "true");
        appConfig.MultipartResponseForIti43AndIti39 = bool.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__MultipartResponseForIti43AndIti39").Value ?? "true");
        appConfig.CertificatesRaw = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__CertificatesRaw").Value?.Split(";") ?? [];
        appConfig.SigningCertificateUrls = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__SigningCertificateUrls").Value?.Split(";") ?? [];
        appConfig.ValidAudiences = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__ValidAudiences").Value?.Split(";") ?? [];
        appConfig.ValidIssuers = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__ValidIssuers").Value?.Split(";") ?? [];
        appConfig.AtnaLogExporterEndpoint = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__AtnaLogExporterEndpoint").Value;
        appConfig.ClamAvEndpoint = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__ClamavEndpoint").Value;
        appConfig.HostName = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "HOSTNAME").Value;

        return appConfig;
    }
}