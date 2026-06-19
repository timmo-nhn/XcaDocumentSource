using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace XcaXds.Shared.Models.Custom;

[DebuggerDisplay("CodedValue (Code = {Code}, CodeSystem = {CodeSystem}, DisplayName = {DisplayName})")]
public class CodedValue
{
    public CodedValue() { }

    public CodedValue(string? code, string? codeSystem)
    {
        Code = code;
        CodeSystem = codeSystem;
    }

    public CodedValue(string? code, string? codeSystem, string? displayName)
    {
        Code = code;
        CodeSystem = codeSystem;
        DisplayName = displayName;
    }

    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? Code { get; set; }
    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? CodeSystem { get; set; }
    [MaxLength(Constants.Properties.MaxStringLength)]
    public string? DisplayName { get; set; }
}