using Microsoft.AspNetCore.Mvc.Testing;
using XcaXds.BusinessLogic.Services;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Tests.Helpers;
using Task = System.Threading.Tasks.Task;

namespace XcaXds.Tests.UnitTests;

public class UnitTests_BusinessLogic_Miscellaneous(WebApplicationFactory<WebService.Program> factory, ITestOutputHelper output) : IntegrationTests_DefaultFixture(factory, output), IClassFixture<WebApplicationFactory<WebService.Program>>
{
    [Fact]
    public async Task GetBusinessRuleDescriptor()
    {
        var plaintext = _businessRulesDescriptorService.WriteBusinessRulesPlainText();
        var json = _businessRulesDescriptorService.WriteBusinessRulesJsonFormatted();
        var obfuscate = _businessRulesDescriptorService.WriteEntriesToObfuscateJsonFormatted();
    }

    [Fact]
    public async Task TryFilterByKjernejournalForskriften()
    {
        var metadata = TestHelpers.GenerateComprehensiveRegistryMetadata(500, "13116900216", true).AsRegistryObjectList().OfType<ExtrinsicObjectType>().ToArray();

        var excpectedRegistryObjects = BusinessLogicFiltersRegistry.FilterByKjernejournalForskriften(metadata).ToArray();
    }
}
