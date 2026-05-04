using Microsoft.AspNetCore.Authorization;

namespace XcaXds.Commons.Attributes;

public class RequiresApiKeyAttribute : AuthorizeAttribute
{
    public RequiresApiKeyAttribute()
    {
        AuthenticationSchemes = "ApiKey";
    }
}
