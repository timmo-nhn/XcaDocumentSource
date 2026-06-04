using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.BusinessLogic.Models.Custom.BusinessLogic;

public record ObfuscationEntry(
    string Name,
    CodedValue[] CodeSystems
);
