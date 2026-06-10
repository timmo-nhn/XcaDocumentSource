using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using XcaXds.Terminology.Extensions;
using XcaXds.Terminology.Mappers;
using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.ValueSetMappers.Hl7;

public class Hl7FhirCodeSystemMapper : ICodeSystemMapper
{
    public ComprehensiveCodeSystem? MapToComprehensiveCodeSystem(string rawInput)
    {
        var hl7Parser = new FhirJsonDeserializer();
        var codeSystem = hl7Parser.Deserialize<CodeSystem>(rawInput);

        return new ComprehensiveCodeSystem()
        {
            SystemOid = codeSystem.Identifier.FirstOrDefault()?.Value?.NoUrn(),
            SystemUrl = codeSystem.Url,
            Values = codeSystem.Concept?.FirstOrDefault(c => c.Display == "Confidentiality")?
            .Concept.Select(c => new CodeSystemValue
            {
                Name = c.Display,
                Value = c.Code
            }).ToArray()
        };
    }
}