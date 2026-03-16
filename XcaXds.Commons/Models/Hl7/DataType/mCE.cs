using System;
using System.Collections.Generic;
using System.Text;
using XcaXds.Commons.Serializers;

namespace XcaXds.Commons.Models.Hl7.DataType;

/// <summary>
/// Minimal CodedElement
/// https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html#3.18.4.1.2.3.4
/// </summary>
public class mCE : Hl7Object
{
    [Hl7(Sequence = 1)]
    public string? IdNumber { get; set; }

    [Hl7(Sequence = 2)]
    public string? DisplayName { get; set; }

    [Hl7(Sequence = 3)]
    public string? AssigningAuthority { get; set; }
}
