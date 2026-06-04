using XcaXds.BusinessLogic.BusinessLogic;
using XcaXds.BusinessLogic.Models.Custom;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using static XcaXds.Commons.Commons.Constants.CodeSystems.Hl7.PurposeOfUse;

namespace XcaXds.BusinessLogic.BusinessLogic;

public static class DocumentObfuscationExtensions
{
    /// <summary>
    /// Obfuscate document entries in a document list with restrictive confidentialitycodes so their documents are unable to be retrieved </para>
    /// Will not remove the entry from the result list! </para>
    /// Metadata which does not explicitly reveal the document content will be preserved, so the DocumentEntry can be properly displayed (authorInstitution, healthcarefacilitytypecode)
    /// </summary>
    public static List<IdentifiableType> ObfuscateRestrictedDocumentEntries(this List<IdentifiableType> identifiableTypes, BusinessLogicParameters? businessLogic, out int obfuscatedEntriesCount)
    {
        obfuscatedEntriesCount = 0;

        if (identifiableTypes == null) return [];

        var requestAppliesTo = businessLogic?.AppliesTo ?? AppliesTo.Unknown;

        foreach (var identifiableType in identifiableTypes)
        {
            if (identifiableType is ExtrinsicObjectType extrinsicObject)
            {
                var confCodes = extrinsicObject.GetClassifications(Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
                    .Select(cls => new CodedValue()
                    {
                        Code = cls.NodeRepresentation,
                        CodeSystem = cls.GetFirstSlot()?.GetFirstValue()
                    }).ToArray();

                bool obfuscate = requestAppliesTo switch
                {
                    AppliesTo.HelseId => confCodes.Any(ccode => BusinessLogicFilters.HealthcarePersonellConfidentialityCodesToObfuscate.Contains((ccode.Code!, ccode.CodeSystem!))),
                    AppliesTo.Helsenorge => confCodes.Any(ccode => BusinessLogicFilters.CitizenConfidentialityCodesToObfuscate.Contains((ccode.Code!, ccode.CodeSystem!))),
                    _ => false
                };

                // Dont obscure in emergency situations
                if (obfuscate && !string.IsNullOrWhiteSpace(businessLogic?.Purpose?.Code) && businessLogic.Purpose.Code.IsAnyOf(ETREAT, BTG) == true)
                {
                    obfuscate = false;
                }

                if (!obfuscate && requestAppliesTo != AppliesTo.Unknown) continue;

                // HAYO! GUID_OBSCURE Setting ID to Guid.Empty will break client processes that expect a valid UUID, but since the document cannot be retrieved,
                // WARNING: This might cause a risk of exposing metadata that can be used to retrieve the document through other means,
                // though XcaDS Has measures in place to keep this from happening
                //extrinsicObject.Id = Guid.Empty.ToString();

                if (extrinsicObject.Name?.LocalizedString?.FirstOrDefault() != null)
                {
                    extrinsicObject.Name.LocalizedString.First().Value = "Sperret";
                }

                foreach (var slot in extrinsicObject.Slot ?? [])
                {
                    ObfuscateSlot(slot);
                }

                foreach (var classification in extrinsicObject.Classification ?? [])
                {
                    ObfuscateClassification(classification);
                }

                foreach (var externalIdentifier in extrinsicObject.ExternalIdentifier ?? [])
                {
                    ObfuscateExternalIdentifier(externalIdentifier);
                }
                obfuscatedEntriesCount++;
            }
        }

        return identifiableTypes;
    }

    private static void ObfuscateExternalIdentifier(ExternalIdentifierType? externalIdentifier, AppliesTo issuer = AppliesTo.Unknown)
    {
        if (externalIdentifier == null || string.IsNullOrWhiteSpace(externalIdentifier.IdentificationScheme)) return;

        switch (externalIdentifier.IdentificationScheme)
        {
            case Constants.Xds.Uuids.DocumentEntry.UniqueId:
                //externalIdentifier.Value = "*****";

                //// HAYO! GUID_OBSCURE
                //goto default;
                //externalIdentifier.RegistryObject = "-1";
                break;

            case Constants.Xds.Uuids.DocumentEntry.PatientId:
                externalIdentifier.Value = "*****";
                break;

            default:
                break;
        }
    }

    private static void ObfuscateClassification(ClassificationType? classification)
    {
        if (classification == null || string.IsNullOrWhiteSpace(classification.ClassificationScheme)) return;

        switch (classification.ClassificationScheme)
        {
            case Constants.Xds.Uuids.DocumentEntry.PracticeSettingCode:
            case Constants.Xds.Uuids.DocumentEntry.TypeCode:
            case Constants.Xds.Uuids.DocumentEntry.Author:
                classification.ClassifiedObject = Guid.Empty.ToString();
                classification.NodeRepresentation = "*****";

                if (classification.Name != null)
                {
                    classification.Name.LocalizedString = classification.Name.LocalizedString?.Select(nm => new LocalizedStringType() { Value = "*****" }).ToArray();
                }
                foreach (var slot in classification.Slot ?? [])
                {
                    ObfuscateSlot(slot);
                }
                break;

            case Constants.Xds.Uuids.DocumentEntry.ClassCode:
            default:

                break;
        }
    }

    private static void ObfuscateSlot(SlotType? slot)
    {
        if (slot == null) return;

        switch (slot.Name)
        {
            case Constants.Xds.SlotNames.AuthorPerson:
            case Constants.Xds.SlotNames.LegalAuthenticator:
                for (int i = 0; i < slot.ValueList?.Value?.Length; i++)
                {
                    var value = slot.ValueList.Value[i];

                    if (value != null)
                    {
                        var structuredValue = Hl7Object.Parse<XCN>(value);
                        if (structuredValue == null) return;

                        structuredValue.PersonIdentifier = string.IsNullOrWhiteSpace(structuredValue.PersonIdentifier) ? null : "*****";
                        structuredValue.FamilyName = string.IsNullOrWhiteSpace(structuredValue.FamilyName) ? null : "*****";
                        structuredValue.MiddleName = string.IsNullOrWhiteSpace(structuredValue.MiddleName) ? null : "*****";
                        structuredValue.GivenName = string.IsNullOrWhiteSpace(structuredValue.GivenName) ? null : "*****";
                        structuredValue.Prefix = string.IsNullOrWhiteSpace(structuredValue.Prefix) ? null : "*****";
                        structuredValue.Suffix = string.IsNullOrWhiteSpace(structuredValue.Suffix) ? null : "*****";
                        structuredValue.Degree = string.IsNullOrWhiteSpace(structuredValue.Degree) ? null : "*****";

                        var structuredValueString = structuredValue.Serialize();
                        if (!string.IsNullOrWhiteSpace(structuredValueString))
                        {
                            slot.ValueList.Value[i] = structuredValueString;
                        }
                    }
                }
                break;

            case Constants.Xds.SlotNames.AuthorInstitution:
                for (int i = 0; i < slot.ValueList?.Value?.Length; i++)
                {
                    var value = slot.ValueList.Value[i];

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        var structuredValue = Hl7Object.Parse<XON>(value);
                        if (structuredValue == null) return;

                        // Dont obfuscate the value for organization or the last value
                        if (structuredValue.AssigningAuthority?.UniversalId == Constants.Oid.Brreg || i == slot.ValueList.Value.Length) continue;

                        structuredValue.OrganizationIdentifier = string.IsNullOrWhiteSpace(structuredValue.OrganizationIdentifier) ? null : "*****";
                        structuredValue.IdNumber = string.IsNullOrWhiteSpace(structuredValue.IdNumber) ? null : "*****";
                        structuredValue.OrganizationName = string.IsNullOrWhiteSpace(structuredValue.OrganizationName) ? null : "*****";

                        var structuredValueString = structuredValue?.Serialize();

                        if (string.IsNullOrWhiteSpace(structuredValueString)) continue;

                        slot.ValueList.Value[i] = structuredValueString;
                    }
                }

                break;

            case Constants.Xds.SlotNames.AuthorSpecialty:
            case Constants.Xds.SlotNames.AuthorRole:
            case Constants.Xds.SlotNames.CodingScheme:
                for (int i = 0; i < slot.ValueList?.Value?.Length; i++)
                {
                    slot.ValueList.Value[i] = "*****";
                }
                break;

            default:
                break;
        }
    }
}
