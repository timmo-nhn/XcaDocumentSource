using System;
using System.Collections.Generic;
using System.Text;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Models.Custom.BusinessLogic;

public record ObfuscationEntry(
    string Name,
    CodedValue[] CodeSystems
);
