using System;
using System.Collections.Generic;
using System.Text;

namespace XcaXds.Commons.Models.Custom.BusinessLogic;

public class BusinessRulesDocument
{
    public List<RuleDto> Rules { get; set; } = [];
}

public class RuleDto
{
    public string Name { get; set; } = "";
    public List<object> Conditions { get; set; } = [];
    public object Result { get; set; } = new();
}

public class FilterResultDto
{
    public string Type { get; set; } = "";
    public List<string>? Allow { get; set; }
    public List<string>? Deny { get; set; }
}