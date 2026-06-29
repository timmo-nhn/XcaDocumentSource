using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using XcaXds.Commons.Attributes;

namespace XcaXds.WebService.Startup;

public class RequiresApiKeyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous =
            context.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any() ||
            (context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any() ?? false);

        if (hasAllowAnonymous)
        {
            return;
        }

        var hasRequiresApiKey =
            context.MethodInfo.GetCustomAttributes(typeof(RequiresApiKeyAttribute), true).Any() ||
            (context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(RequiresApiKeyAttribute), true).Any() ?? false);

        if (!hasRequiresApiKey)
        {
            return;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("ApiKeyScheme", context.Document, null),
                []
            }
        });
    }
}
