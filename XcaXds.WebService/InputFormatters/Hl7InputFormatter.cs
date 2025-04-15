using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using XcaXds.Commons.Models.Hl7;

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
        return typeof(Hl7RawMessage).IsAssignableFrom(type);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        var request = context.HttpContext.Request;
        using var reader = new StreamReader(request.Body, encoding);
        var content = await reader.ReadToEndAsync();

        var hl7Serializer = new Hl7MessageSerializer();

        var mossyo = hl7Serializer.Deserialize(content);

        var gobb  = hl7Serializer.Serialize(mossyo);

        var hl7RawMessage = new Hl7RawMessage() { Raw = content };
        return await InputFormatterResult.SuccessAsync(hl7RawMessage);
    }

}
