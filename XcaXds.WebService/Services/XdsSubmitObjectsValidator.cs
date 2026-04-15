using System.Text.RegularExpressions;
using XcaXds.Commons.Models;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.WebService.Services;

/// <summary>
/// Validate the content in a SubmitObjectsRequest before processing it further. 
/// This includes checks for valid inputs and ensures no malicious content is being uploaded.
/// </summary>
public partial class XdsSubmitObjectsValidator
{
    private readonly ILogger<XdsSubmitObjectsValidator> _logger;

    public XdsSubmitObjectsValidator(ILogger<XdsSubmitObjectsValidator> logger)
    {
        _logger = logger;
    }

    public XdsValidationResponse[] ValidateSubmitObjectsRequest(SubmitObjectsRequest request)
    {
        var validationResults = new List<XdsValidationResponse>();

        var extrinsicObjects = request.RegistryObjectList?.OfType<ExtrinsicObjectType>().ToArray() ?? [];
        var registryPackages = request.RegistryObjectList?.OfType<RegistryPackageType>().ToArray() ?? [];

        foreach (var documentEntry in extrinsicObjects)
        {
            validationResults.AddRange(ValidateTitle(validationResults, documentEntry.Name?.LocalizedString));
        }

        foreach (var submissionset in registryPackages)
        {
            var titles = submissionset.Name?.LocalizedString ?? [];
        }

        return validationResults.ToArray();
    }

    private static List<XdsValidationResponse> ValidateTitle(List<XdsValidationResponse> validationResults, LocalizedStringType[]? titles)
    {
        var response = new List<XdsValidationResponse>();

        foreach (var title in titles ?? [])
        {
            var titleValue = title.Value;
            if (string.IsNullOrWhiteSpace(titleValue)) continue;

            var match = RegexTitle().Matches(titleValue);
            if (match.Count > 0)
            {
                response.Add(new($"Tile must match regex: {RegexTitle().ToString()}"));
            }
        }

        return response;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9\s\.,'\-_]+$")]
    private static partial Regex RegexTitle();
}