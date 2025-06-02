using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Text.Json;
using XcaXds.Commons.Models.Custom.DocumentEntryDto;

namespace XcaXds.WebService.InputFormatters;

public class JsonDocumentEntryDtoModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(DocumentEntryDto))
        {
            return new BinderTypeModelBinder(typeof(JsonDocumentEntryDtoModelBinder));
        }

        return null;
    }
}

public class JsonDocumentEntryDtoModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var request = bindingContext.HttpContext.Request;
        var response = bindingContext.HttpContext.Response;

        if (!request.ContentType?.Contains("xml") ?? true)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        JsonSerializer.Deserialize()
    }
}

