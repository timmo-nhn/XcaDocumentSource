using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;
using XcaXds.Commons.Models.Hl7.V2;
namespace XcaXds.WebService.InputFormatters;

public class Hl7InputFormatter : TextInputFormatter
{
    public Hl7InputFormatter()
    {
        SupportedMediaTypes.Add("application/hl7-v2");
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.ASCII);
    }

    protected override bool CanReadType(Type type)
    {
        return typeof(Message).IsAssignableFrom(type);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        var request = context.HttpContext.Request;

        using var reader = new StreamReader(request.Body, encoding);
        var content = await reader.ReadToEndAsync();

        try
        {
            content = content
                .Replace("\r\n", "\r")
                .Replace("\n", "\r");

            var hl7RawMessage = new Message(content);
            hl7RawMessage.ParseMessage();

            return await InputFormatterResult.SuccessAsync(hl7RawMessage);
        }
        catch (Exception ex)
        {

            return await InputFormatterResult.FailureAsync();
        }
    }
}
