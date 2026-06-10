namespace XcaXds.WebService.Startup;

public static class ConfigBinder
{
    public static ApplicationConfig BindKeyValueEnvironmentVariablesToXdsConfiguration(List<KeyValuePair<string, string>> xdsConfigEnvVars)
    {
        return new()
        {
            ValidateSamlTokenIntegrity = bool.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__ValidateSamlTokenIntegrity").Value ?? "false"),
            TimeoutInSeconds = int.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__TimeoutInSeconds").Value ?? "0"),
            HomeCommunityId = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__HomeCommunityId").Value,
            RepositoryUniqueId = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__RepositoryUniqueId").Value,
            DocumentUploadSizeLimitKb = int.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__DocumentUploadSizeLimitKb").Value ?? "0"),
            BypassPolicyEnforcementPoint = bool.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__BypassPolicyEnforcementPoint").Value ?? "false"),
            WrapRetrievedDocumentInCda = bool.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__WrapRetrievedDocumentInCda").Value ?? "true"),
            MultipartResponseForIti43AndIti39 = bool.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__MultipartResponseForIti43AndIti39").Value ?? "true"),
            CertificatesRaw = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__CertificatesRaw").Value.Split(";"),
            SigningCertificateUrls = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__SigningCertificateUrls").Value.Split(";"),
            ValidAudiences = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__ValidAudiences").Value.Split(";"),
            ValidIssuers = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__ValidIssuers").Value.Split(";"),
            AtnaLogExporterEndpoint = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__AtnaLogExporterEndpoint").Value,
            ClamAvEndpoint = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__ClamavEndpoint").Value,
            HostName = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "HOSTNAME").Value,
            //FriendlyName = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__HelsenorgeSigningCertUrl").Value,
            //IpAddress = xdsConfigEnvVars.FirstOrDefault(f => f.Key.EndsWith("HOSTNAME")).Value
        };
    }
}