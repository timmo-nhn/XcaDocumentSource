using Microsoft.AspNetCore.Mvc;
using XcaXds.Commons.Commons;
using XcaXds.Shared.Commons;

namespace XcaXds.WebService.Models.Custom;

public class CustomContentResult : ContentResult
{
    public CustomContentResult(string content, int statusCode, string contentType = Constants.MimeTypes.Json)
    {
        Content = content;
        ContentType = contentType;
        StatusCode = statusCode;
    }
}
