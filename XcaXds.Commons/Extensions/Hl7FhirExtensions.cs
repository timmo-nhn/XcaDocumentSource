using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using XcaXds.Commons.Models.Custom;

namespace XcaXds.Commons.Extensions;

public static class Hl7FhirExtensions
{
    public static Resource? GetResourceFromStream(Stream? requestBody)
    {
        if (requestBody == null) return null;

        var fhirparser = new FhirJsonDeserializer();
        requestBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(requestBody, leaveOpen: true);
        var json = reader.ReadToEnd();
        requestBody.Seek(0, SeekOrigin.Begin);
        return fhirparser.TryDeserializeResource(json, out var instance, out _) ? instance : null;
    }

    public static DateRange GetDateTimeRangeFromDateParameters(string timingAndDate)
    {
        timingAndDate = timingAndDate.Replace("%3A", ":");
        var modifier = timingAndDate.Substring(0, 2);
        var date = timingAndDate.Substring(2);
        var datetime = DateTime.Parse(date);

        switch (modifier)
        {
            case "eq":
                var queryInstantEq = DateTime.Parse(date);
                return new DateRange(queryInstantEq, queryInstantEq.AddDays(1).Trim(TimeSpan.TicksPerDay).AddTicks(-1));

            case "gt":
                var queryInstantGt = DateTime.Parse(date);
                return new DateRange(queryInstantGt, null);

            case "lt":
                var queryInstantLt = DateTime.Parse(date);
                return new DateRange(null, queryInstantLt);

            case "ge":
                var queryInstantGe = DateTime.Parse(date);
                return new DateRange(queryInstantGe.AddTicks(-1), null);

            case "le":
                var queryInstantLe = DateTime.Parse(date);
                return new DateRange(null, queryInstantLe.AddTicks(-1));

            case "sa":
                var queryInstantSa = DateTime.Parse(date);
                return new DateRange(queryInstantSa, null);

            case "eb":
                var queryInstantEb = DateTime.Parse(date);
                return new DateRange(null, queryInstantEb.AddTicks(-1));

            case "ap":
                var queryInstantAp = DateTime.Parse(date);
                return new DateRange(queryInstantAp.AddDays(-10), queryInstantAp.AddDays(10));

            default:
                break;
        }


        throw new NotImplementedException();
    }

    static DateTime Trim(this DateTime date, long roundTicks)
    {
        return new DateTime(date.Ticks - date.Ticks % roundTicks, date.Kind);
    }

    public static ResourceReference GetResourceAsResourceReference(Resource resource)
    {
        return new ResourceReference() { Reference = $"#{resource.Id}" };
    }

    public static List<ResourceReference> GetResourceAsResourceReference(List<Resource> resource)
    {
        return resource.Select(res => GetResourceAsResourceReference(res)).ToList();
    }
}