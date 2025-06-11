using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Text.Json;
using XcaXds.Commons.Models.Custom.DocumentEntry;

namespace XcaXds.WebService.InputFormatters;

public class DocumentEntryDtoModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(DocumentEntryDto))
        {
            return new BinderTypeModelBinder(typeof(DocumentEntryDtoModelBinder));
        }

        return null;
    }
}

public class DocumentEntryDtoModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var request = bindingContext.HttpContext.Request;
        var response = bindingContext.HttpContext.Response;

        if (!request.ContentType?.Contains("json") ?? true)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        try
        {
            var content = JsonSerializer.Deserialize<DocumentEntryDto>(request.Body);
            bindingContext.Result = ModelBindingResult.Success(content);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}

