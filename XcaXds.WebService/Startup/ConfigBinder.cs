namespace XcaXds.WebService.Startup;

public static class ConfigBinder
{
    public static ApplicationConfig BindKeyValueEnvironmentVariablesToXdsConfiguration(List<KeyValuePair<string, string>> xdsConfigEnvVars)
    {
        return new()
        {
            TimeoutInSeconds = int.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__TimeoutInSeconds").Value ?? "0"),
            HomeCommunityId = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__HomeCommunityId").Value,
            RepositoryUniqueId = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__RepositoryUniqueId").Value,
            DocumentUploadSizeLimitKb = int.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__DocumentUploadSizeLimitKb").Value ?? "0"),
            WrapRetrievedDocumentInCda = bool.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__WrapRetrievedDocumentInCda").Value ?? "false"),
            MultipartResponseForIti43 = bool.Parse(xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__MultipartResponseForIti43").Value ?? "false"),
        };
    }
}