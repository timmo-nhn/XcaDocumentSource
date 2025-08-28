using Microsoft.AspNetCore.Mvc;

namespace XcaXds.Commons.Models.Custom;

public class HttpResponseMessageResult : IActionResult
{
    private readonly HttpResponseMessage _httpResponseMessage;

    public HttpResponseMessageResult(HttpResponseMessage httpResponseMessage)
    {
        _httpResponseMessage = httpResponseMessage;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = (int)_httpResponseMessage.StatusCode;

        foreach (var header in _httpResponseMessage.Content.Headers)
        {
            response.Headers[header.Key] = header.Value.ToArray();
        }

        await _httpResponseMessage.Content.CopyToAsync(response.Body);
    }
}
