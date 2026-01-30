namespace XcaXds.WebService.Middleware.PolicyEnforcementPoint.InputStrategies
{
    public interface IPolicyInputStrategy
    {
        bool CanHandle(string? contentType);
        Task<PolicyInputResult> BuildAsync(HttpContext context, string requestBody);
    }

}
