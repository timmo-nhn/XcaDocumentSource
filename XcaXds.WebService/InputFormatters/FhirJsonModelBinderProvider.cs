using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Text;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;

using Task = System.Threading.Tasks.Task;

namespace XcaXds.WebService.InputFormatters;

public class FhirJsonModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(Base))
        {
            return new BinderTypeModelBinder(typeof(DocumentEntryDtoModelBinder));
        }

        return null;
    }
}

public class FhirJsonModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var request = bindingContext.HttpContext.Request;
        var response = bindingContext.HttpContext.Response;

        if (!request.ContentType?.Contains(Constants.MimeTypes.FhirJson) ?? true)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        try
        {
            var fhirJsonSerializer = new FhirJsonDeserializer();

            var content = fhirJsonSerializer.DeserializeResource(await HttpRequestResponseExtensions.GetHttpRequestBodyAsStringAsync(request));

            bindingContext.Result = ModelBindingResult.Success(content);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}