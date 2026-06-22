using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using XcaXds.Shared.Extensions;
using XcaXds.Shared.Models.Custom;
using XcaXds.Terminology.Interfaces;

namespace XcaXds.Terminology.ValueSetMappers.Hl7;

public class Hl7FhirCodeSystemMapper : ICodeSystemMapper
{
    private string _displayDiscriminator;

    /// <summary>
    /// The HL7 FHIR CodeSystem resource can contain multiple code systems within the same resource, differentiated by the "display" property of the concepts.
    /// </summary>
    public Hl7FhirCodeSystemMapper(string displayDiscriminator)
    {
        _displayDiscriminator = displayDiscriminator;
    }

    public Hl7FhirCodeSystemMapper() { }

    public ComprehensiveCodeSystem? MapToComprehensiveCodeSystem(string rawInput)
    {
        var hl7Parser = new FhirJsonDeserializer();
        var codeSystem = hl7Parser.Deserialize<CodeSystem>(rawInput);

        var allConcepts = codeSystem.Concept
            .FirstOrDefault(c => string.IsNullOrWhiteSpace(_displayDiscriminator) || c.Display == _displayDiscriminator)?
            .Concept?.Select(c => new CodeSystemValue(c.Code, c.Display))
            .ToArrayOrNull()
            ??
            codeSystem.Concept
            .SelectMany(c => c.Concept)
            .Where(c => string.IsNullOrWhiteSpace(_displayDiscriminator) || c.Display == _displayDiscriminator)
            .Select(c => new CodeSystemValue(c.Code, c.Display))
            .ToArrayOrNull()
            ??
            codeSystem.Concept
            .Where(c => string.IsNullOrWhiteSpace(_displayDiscriminator) || c.Display == _displayDiscriminator)
            .Select(c => new CodeSystemValue(c.Code, c.Display))
            .ToArrayOrNull();

        return new ComprehensiveCodeSystem()
        {
            System = codeSystem.Identifier.FirstOrDefault()?.Value?.NoUrn(),
            SystemsAlternate = [.. new[] { codeSystem.Url }.OfType<string>()],
            Values = allConcepts
        };
    }
}