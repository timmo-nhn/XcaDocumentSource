using XcaXds.Commons.Models.Custom.ApiKey;

namespace XcaXds.WebService.Startup;

public static class ApiKeyBinder
{
    public static ApiKeyHolder BindApiKeyEnvironmentVariablesToApiKey(List<KeyValuePair<string, string>> xdsConfigEnvVars)
    {
        return new()
        {
            ApiKey = xdsConfigEnvVars.FirstOrDefault(f => f.Key == "XdsConfiguration__ApiKey").Value
        };
    }
}

