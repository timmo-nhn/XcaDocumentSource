using Hl7.Fhir.Model;
using System.Text;
using XcaXds.Source.Source;

namespace XcaXds.WebService.Services;

public class FhirService
{
    private readonly RegistryWrapper _registry;

    public FhirService(RegistryWrapper registry)
    {
        _registry = registry;
    }


    public OperationOutcome PatchResource(Bundle bundle)
    {
        foreach (var entry in bundle.Entry)
        {
            var url = entry.FullUrl;

            if (entry.Resource is not Binary fhirBinary) continue;

            var patchData = Encoding.UTF8.GetString(fhirBinary.Data ?? []);

        }

        return new OperationOutcome();
    }
}