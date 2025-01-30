using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Services;

public class SoapEnvelopeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(SoapEnvelope))
        {
            return new BinderTypeModelBinder(typeof(SoapEnvelopeModelBinder));
        }

        return null;
    }
}

public class SoapEnvelopeModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var request = bindingContext.HttpContext.Request;
        if (!request.ContentType?.Contains("xml") ?? true)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        var xmlContent = request.Body;

        if (xmlContent is null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        try
        {
            var xmlSerializer = new SoapXmlSerializer();
            var soapEnvelope = await xmlSerializer.DeserializeSoapMessageAsync<SoapEnvelope>(xmlContent);

            bindingContext.Result = ModelBindingResult.Success(soapEnvelope);
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid XML: {ex.Message}");
            bindingContext.Result = ModelBindingResult.Failed();
        }
    }
}
