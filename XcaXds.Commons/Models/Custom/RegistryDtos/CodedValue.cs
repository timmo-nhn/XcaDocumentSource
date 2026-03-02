using System.Diagnostics;

namespace XcaXds.Commons.Models.Custom.RegistryDtos;

[DebuggerDisplay("CodedValue (Code = {Code}, CodeSystem = {CodeSystem}, DisplayName = {DisplayName})")]
public class CodedValue
{
    public string? Code { get; set; }
    public string? CodeSystem { get; set; }
    public string? DisplayName { get; set; }
}