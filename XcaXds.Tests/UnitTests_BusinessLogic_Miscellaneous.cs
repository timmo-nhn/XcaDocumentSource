using XcaXds.BusinessLogic.BusinessLogic;
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
        var plaintext = BusinessRulesDescriptor.BusinessRulesPlainText;
        var json = BusinessRulesDescriptor.BusinessRulesJson;
        var obfuscate = BusinessRulesDescriptor.EntriesToObfuscateJson;
    }

    [Fact]
    public async Task TryFilterByKjernejournalForskriften()
    {
        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(500, "13116900216", true).AsRegistryObjectList().OfType<ExtrinsicObjectType>().ToArray();

        var excpectedRegistryObjects = BusinessLogicFilters.FilterByKjernejournalForskriften(metadata).ToArray();
    }
}
