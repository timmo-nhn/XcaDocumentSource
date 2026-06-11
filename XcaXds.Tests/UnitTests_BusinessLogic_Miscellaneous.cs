using XcaXds.BusinessLogic.Services;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Tests.Helpers;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests;

public class UnitTests_BusinessLogic_Miscellaneous
{
    [Fact]
    public async Task GetBusinessRuleDescriptor()
    {
        var plaintext = BusinessRulesDescriptorService.BusinessRulesPlainText;
        var json = BusinessRulesDescriptorService.BusinessRulesJson;
        var obfuscate = BusinessRulesDescriptorService.EntriesToObfuscateJson;
    }

    [Fact]
    public async Task TryFilterByKjernejournalForskriften()
    {
        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(500, "13116900216", true).AsRegistryObjectList().OfType<ExtrinsicObjectType>().ToArray();

        var excpectedRegistryObjects = BusinessLogicFiltersRegistry.FilterByKjernejournalForskriften(metadata).ToArray();
    }
}
