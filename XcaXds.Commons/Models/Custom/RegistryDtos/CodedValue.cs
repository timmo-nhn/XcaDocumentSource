using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using XcaXds.Commons.Commons;

namespace XcaXds.Commons.Models.Custom.RegistryDtos;

[DebuggerDisplay("CodedValue (Code = {Code}, CodeSystem = {CodeSystem}, DisplayName = {DisplayName})")]
public class CodedValue
{
    public CodedValue() { }

    public CodedValue(string? code, string? codeSystem)
    {
        Code = code;
        CodeSystem = codeSystem;
    }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? Code { get; set; }
    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? CodeSystem { get; set; }
    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? DisplayName { get; set; }
}