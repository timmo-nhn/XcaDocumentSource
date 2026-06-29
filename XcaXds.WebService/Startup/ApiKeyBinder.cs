using System.Collections.Immutable;
using XcaXds.Commons.Models.Custom.ApiKey;

namespace XcaXds.WebService.Startup;

public static class ApiKeyBinder
{
    public static ApiKeyHolder BindApiKeyEnvironmentVariablesToApiKey(Dictionary<string, string> xdsConfigEnvVars)
    {
        return new()
        {
            ApiKey = xdsConfigEnvVars.ToDictionary().GetValueOrDefault("XdsConfiguration__ApiKey") ?? string.Empty
        };
    }
}