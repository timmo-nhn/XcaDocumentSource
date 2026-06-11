using System.Text.RegularExpressions;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared.Commons;

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

    public XdsValidationResponse[] ValidateSubmitObjectsRequest(SubmitObjectsRequest? request)
    {
        return ValidateSubmitObjectsRequest(request?.RegistryObjectList);
    }

    public XdsValidationResponse[] ValidateSubmitObjectsRequest(IdentifiableType[]? request)
    {
        var validationResults = new List<XdsValidationResponse>();

        var extrinsicObjects = request?.OfType<ExtrinsicObjectType>().ToArray() ?? [];
        var registryPackages = request?.OfType<RegistryPackageType>().ToArray() ?? [];

        foreach (var documentEntry in extrinsicObjects)
        {
            ValidateTitle(validationResults, documentEntry.Name?.LocalizedString, $"DocumentEntry ({documentEntry.Id}).Name.LocalizedStringType");
            ValidateClassifications(validationResults, documentEntry.Classification, $"DocumentEntry ({documentEntry.Id}).Classification");
            ValidateExternalIdentifiers(validationResults, documentEntry.ExternalIdentifier, $"DocumentEntry ({documentEntry.Id}).ExternalIdentifier");
        }

        foreach (var submissionSet in registryPackages)
        {
            ValidateTitle(validationResults, submissionSet.Name?.LocalizedString, $"SubmissionSet ({submissionSet.Id})");
            ValidateClassifications(validationResults, submissionSet.Classification, $"DocumentEntry ({submissionSet.Id}).Classification");
            ValidateExternalIdentifiers(validationResults, submissionSet.ExternalIdentifier, $"DocumentEntry ({submissionSet.Id}).ExternalIdentifier");
        }

        return [.. validationResults];
    }

    private void ValidateExternalIdentifiers(List<XdsValidationResponse> validationResults, ExternalIdentifierType[]? externalIdentifiers, string location)
    {
        foreach (var externalIdentifier in externalIdentifiers ?? [])
        {
            if (externalIdentifier.IdentificationScheme == Constants.Xds.Uuids.SubmissionSet.SourceId)
            {
                MatchOid(validationResults, externalIdentifier.Value, location);
            }
            else
            {
                MatchString(validationResults, externalIdentifier.Value, location);
            }
            
            ValidateSlots(validationResults, externalIdentifier.Slot);
        }
    }

    private void ValidateClassifications(List<XdsValidationResponse> validationResults, ClassificationType[]? classifications, string location)
    {
        foreach (var classification in classifications ?? [])
        {
            MatchString(validationResults, classification.NodeRepresentation, location);
            ValidateSlots(validationResults, classification.Slot);
        }
    }

    private void MatchOid(List<XdsValidationResponse> validationResults, string value, string location)
    {
        if (RegexOid().Count(value ?? "") == 0)
        {
            var response = new XdsValidationResponse($"Value must match regex: {RegexOid()}");
            var illegalCharacters = GetIllegalCharactersFromString(value, RegexOid());

            if (!string.IsNullOrWhiteSpace(location))
            {
                response.Message += "\n Location: " + location + "\n Value: " + value + "\n Illegal characters: " + illegalCharacters;
            }

            validationResults.Add(response);

        }
    }

    private void MatchString(List<XdsValidationResponse> validationResults, string? stringToValidate, string? location)
    {
        if (RegexAllowedCharacters().Count(stringToValidate ?? "") == 0)
        {
            var response = new XdsValidationResponse($"Value must match regex: {RegexAllowedCharacters()}");
            var illegalCharacters = GetIllegalCharactersFromString(stringToValidate, RegexAllowedCharacters());

            if (!string.IsNullOrWhiteSpace(location))
            {
                response.Message += "\n Location: " + location + "\n Value: " + stringToValidate + "\n Illegal characters: " + illegalCharacters;
            }

            validationResults.Add(response);

        }
    }

    private void ValidateSlots(List<XdsValidationResponse> validationResults, SlotType[]? slots)
    {
        for (int i = 0; i < slots?.Length; i++)
        {
            SlotType? slot = slots[i];
            foreach (var value in slot.ValueList?.Value ?? [])
            {
                MatchString(validationResults, value, $"({slot.Name}).Value[{i}] ");
            }
        }
    }

    private void ValidateTitle(List<XdsValidationResponse> validationResults, LocalizedStringType[]? titles, string? location)
    {
        foreach (var title in titles ?? [])
        {
            MatchString(validationResults, title.Value, location);
        }
    }

    private static string? GetIllegalCharactersFromString(string? titleValue, Regex regex)
    {
        if (string.IsNullOrWhiteSpace(titleValue)) return null;

        var illegalChars = string.Empty;
        foreach (var letter in titleValue)
        {
            if (regex.Count(letter.ToString()) > 0)
            {
                continue;
            }
            illegalChars += letter;
        }
        return illegalChars;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9æøåÆØÅáÁéÉíÍóÓúÚýÝ\s.,:;()\-–——_÷*'""/+%&@£$€{\[\]}§|!?\^]*$")]
    private static partial Regex RegexAllowedCharacters();

    [GeneratedRegex(@"^[\d\.]+$")]
    private static partial Regex RegexOid();
}