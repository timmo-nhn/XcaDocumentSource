using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Extensions;

/// <summary>
/// Extension-methods for filtering data based on search parameters specified in ITI-18 Registry Stored Query
/// Works with types in XcaXds.Commons.Models.Soap.XdsTypes 
/// Documentation on format and optionality
/// https://profiles.ihe.net/ITI/TF/Volume2/ITI-18.html#3.18.4.1.2.3.7
/// 
/// -- Format --
/// Parameter Name:     Part of the request (ITI-18)
/// Attribute:          Part of the Document Registry Object    
///
/// Opt(Optionality):   [R]=Required, [O]=Optional
/// Mult(Multiple):     [-]=zero or one, [M]=zero or many
/// When a property has Mult value [M], the method input is a string array. If not ([-]), its just string
///
/// -- Example --
/// | Parameter Name (ITI-18)      | Attribute                   | Opt | Mult |
/// |------------------------------|-----------------------------|-----|------|
/// | $XDSDocumentEntryPatientId   | XDSDocumentEntry.patientId  | R|O | —|M  |
/// </summary>

public static class RegistrySearchExtensions
{
    /// <summary>
    /// | Parameter Name (ITI-18)      | Attribute                   | Opt | Mult |
    /// |------------------------------|-----------------------------|-----|------|
    /// | $XDSDocumentEntryPatientId   | XDSDocumentEntry.patientId  | R   | —    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByPatientId(
        this IEnumerable<ExtrinsicObjectType> source, string patientId)
    {
        if (string.IsNullOrWhiteSpace(patientId)) return Enumerable.Empty<ExtrinsicObjectType>();  // Required field, return nothing if not specified
        return source.Where(eo => eo.ExternalIdentifier.Any(ei => ei.Value.Contains(patientId)));
    }

    /// <summary>
    /// | Parameter Name (ITI-18)       | Attribute                   | Opt | Mult |
    /// |-------------------------------|-----------------------------|-----|------|
    /// | $XDSDocumentEntryClassCode(1) | XDSDocumentEntry.classCode  | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryClassCode(
        this IEnumerable<ExtrinsicObjectType> source, List<string[]> classCodes)
    {
        if (classCodes == null || classCodes.Count == 0) return source; // Optional field, return everything if not specified
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.ClassCode)
            .Select(cf => cf.NodeRepresentation + "^^" + cf.GetSlot(Constants.Xds.SlotNames.CodingScheme).FirstOrDefault()?.GetFirstValue())
            .All(co => classCodes.Any(cc => cc.Contains(co))));
    }

    /// <summary>
    /// | Parameter Name (ITI-18)      | Attribute                   | Opt | Mult |
    /// |------------------------------|-----------------------------|-----|------|
    /// | $XDSDocumentEntryTypeCode(1) | XDSDocumentEntry.typeCode   | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryTypeCode(
        this IEnumerable<ExtrinsicObjectType> source, List<string[]> typeCodes)
    {
        if (typeCodes == null || typeCodes.Count == 0) return source; // Optional field, return everything if not specified
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.TypeCode)
            .Select(cf => cf.NodeRepresentation + "^^" + cf.GetSlot(Constants.Xds.SlotNames.CodingScheme).FirstOrDefault()?.GetFirstValue())
            .All(co => typeCodes.Any(cc => cc.Contains(co))));
    }

    /// <summary>
    /// | Parameter Name (ITI-18)                 | Attribute                             | Opt | Mult |
    /// |-----------------------------------------|---------------------------------------|-----|------|
    /// | $XDSDocumentEntryPracticeSettingCode(1) | XDSDocumentEntry.practiceSettingCode  | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryPracticeSettingCode(
        this IEnumerable<ExtrinsicObjectType> source, List<string[]> practiceSettingCodes)
    {
        if (practiceSettingCodes == null || practiceSettingCodes.Count == 0) return source; // Optional field, return everything if not specified
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.ClassCode)
            .Select(cf => cf.NodeRepresentation + "^^" + cf.GetSlot(Constants.Xds.SlotNames.CodingScheme).FirstOrDefault()?.GetFirstValue())
            .All(co => practiceSettingCodes.Any(cc => cc.Contains(co))));
    }

    /// <summary>
    /// | Parameter Name (ITI-18)            | Attribute                                     | Opt | Mult |
    /// |------------------------------------|-----------------------------------------------|-----|------|
    /// | $XDSDocumentEntryCreationTimeFrom  | Lower value of XDSDocumentEntry.creationTime  | O   | -    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryCreationTimeFrom(
        this IEnumerable<ExtrinsicObjectType> source, string creationTimeFrom)
    {
        if (string.IsNullOrWhiteSpace(creationTimeFrom)) return source;  // Optional field, return everything if not specified
        var dateTime = DateTime.ParseExact(creationTimeFrom, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);

        return source.Where(eo =>
            eo.GetSlot(Constants.Xds.SlotNames.CreationTime)?.GetValues()?
            .Select(dt => DateTime.ParseExact(dt, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture))
            .Any(parsedDt => parsedDt >= dateTime) == true);
    }

    /// <summary>
    /// | Parameter Name (ITI-18)            | Attribute                                     | Opt | Mult |
    /// |------------------------------------|-----------------------------------------------|-----|------|
    /// | $XDSDocumentEntryCreationTimeTo    | Upper value of XDSDocumentEntry.creationTime  | O   | -    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryCreationTimeTo(
       this IEnumerable<ExtrinsicObjectType> source, string creationTimeFrom)
    {
        if (string.IsNullOrWhiteSpace(creationTimeFrom)) return source;  // Optional field, return everything if not specified
        var dateTime = DateTime.ParseExact(creationTimeFrom, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);

        return source.Where(eo =>
            eo.GetSlot(Constants.Xds.SlotNames.CreationTime)?.GetValues()?
            .Select(dt => DateTime.ParseExact(dt, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture))
            .Any(parsedDt => parsedDt <= dateTime) == true);
    }


    /// <summary>
    /// | Parameter Name (ITI-18)               | Attribute                                         | Opt | Mult |
    /// |---------------------------------------|---------------------------------------------------|-----|------|
    /// | $XDSDocumentEntryServiceStartTimeFrom | Lower value of XDSDocumentEntry.serviceStartTime  | O   | -    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryServiceStartTimeFrom(
        this IEnumerable<ExtrinsicObjectType> source, string startTimeFrom)
    {
        if (string.IsNullOrWhiteSpace(startTimeFrom)) return source;  // Optional field, return everything if not specified
        var dateTime = DateTime.ParseExact(startTimeFrom, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);

        return source.Where(eo =>
            eo.GetSlot(Constants.Xds.SlotNames.ServiceStartTime)?.GetValues()?
            .Select(dt => DateTime.ParseExact(dt, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture))
            .Any(parsedDt => parsedDt >= dateTime) == true);
    }

    /// <summary>
    /// | Parameter Name (ITI-18)             | Attribute                                        | Opt | Mult |
    /// |-------------------------------------|--------------------------------------------------|-----|------|
    /// | $XDSDocumentEntryServiceStartTimeTo | Upper value of XDSDocumentEntry.serviceStartTime | O   | —    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryServiceStartTimeTo(
        this IEnumerable<ExtrinsicObjectType> source, string startTimeTo)
    {
        if (string.IsNullOrWhiteSpace(startTimeTo)) return source;  // Optional field, return everything if not specified
        var dateTime = DateTime.ParseExact(startTimeTo, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);

        return source.Where(eo =>
            eo.GetSlot(Constants.Xds.SlotNames.ServiceStartTime)?.GetValues()?
            .Select(dt => DateTime.ParseExact(dt, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture))
            .Any(parsedDt => parsedDt <= dateTime) == true);
    }

    /// <summary>
    /// | Parameter Name (ITI-18)              | Attribute                                        | Opt | Mult |
    /// |--------------------------------------|--------------------------------------------------|-----|------|
    /// | $XDSDocumentEntryServiceStopTimeFrom | Lower value of XDSDocumentEntry.serviceStartTime | O   | —    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryServiceStopTimeFrom(
        this IEnumerable<ExtrinsicObjectType> source, string stopTimeFrom)
    {
        if (string.IsNullOrWhiteSpace(stopTimeFrom)) return source;  // Optional field, return everything if not specified
        var dateTime = DateTime.ParseExact(stopTimeFrom, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);

        return source.Where(eo =>
            eo.GetSlot(Constants.Xds.SlotNames.ServiceStopTime)?.GetValues()?
            .Select(dt => DateTime.ParseExact(dt, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture))
            .Any(parsedDt => parsedDt >= dateTime) == true);
    }

    /// <summary>
    /// | Parameter Name (ITI-18)            | Attribute                                       | Opt | Mult |
    /// |------------------------------------|-------------------------------------------------|-----|------|
    /// | $XDSDocumentEntryServiceStopTimeTo | Upper value of XDSDocumentEntry.serviceStopTime | O   | —    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryServiceStopTimeTo(
        this IEnumerable<ExtrinsicObjectType> source, string stopTimeTo)
    {
        if (string.IsNullOrWhiteSpace(stopTimeTo)) return source;  // Optional field, return everything if not specified
        var dateTime = DateTime.ParseExact(stopTimeTo, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture);

        return source.Where(eo =>
            eo.GetSlot(Constants.Xds.SlotNames.ServiceStopTime)?.GetValues()?
            .Select(dt => DateTime.ParseExact(dt, Constants.Hl7.Dtm.DtmFormat, CultureInfo.InvariantCulture))
            .Any(parsedDt => parsedDt <= dateTime) == true);
    }

    /// <summary>
    /// | Parameter Name (ITI-18)                     | Attribute                                   | Opt | Mult |
    /// |---------------------------------------------|---------------------------------------------|-----|------|
    /// | $XDSDocumentEntryHealthcareFacilityTypeCode | XDSDocumentEntry.healthcareFacilityTypeCode | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryHealthcareFacilityTypeCode(
        this IEnumerable<ExtrinsicObjectType> source, List<string[]> healthcareFacilityTypeCodes)
    {
        if (healthcareFacilityTypeCodes == null || healthcareFacilityTypeCodes.Count == 0) return source; // Optional field, return everything if not specified
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.ClassCode)
            .Select(cf => cf.NodeRepresentation + "^^" + cf.GetSlot(Constants.Xds.SlotNames.CodingScheme).FirstOrDefault()?.GetFirstValue())
            .All(hcfTypeCode => healthcareFacilityTypeCodes.Any(hcfTypeCodes => hcfTypeCodes.Contains(hcfTypeCode))));
    }

    /// <summary>
    /// | Parameter Name (ITI-18)        | Attribute                      | Opt | Mult |
    /// |--------------------------------|--------------------------------|-----|------|
    /// | $XDSDocumentEntryEventCodeList | XDSDocumentEntry.eventCodeList | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryEventCodeList(
        this IEnumerable<ExtrinsicObjectType> source, List<string[]> eventCodeList)
    {
        if (eventCodeList == null || eventCodeList.Count == 0) return source; // Optional field, return everything if not specified

        return source.Where(eo =>
        {
            // Get all the confidentiality codes for the current ExtrinsicObject (eo)
            var eventCodesForExtrinsicObject = eo.Classification
                .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.EventCodeList)
                .Select(cf => cf.NodeRepresentation + "^^" + cf.GetSlot(Constants.Xds.SlotNames.CodingScheme).FirstOrDefault()?.GetFirstValue())
                .ToArray(); // Collect all confidentiality codes into an array

            // Check if all groups match (AND logic for groups)
            return eventCodeList.All(singleEventCodeList =>
                // OR logic inside each group: does any code from the group match any of the extracted confidentiality codes?
                singleEventCodeList.Any(eventCode => eventCodesForExtrinsicObject.Contains(eventCode))
            );
        });
    }

    /// <summary>
    /// | Parameter Name (ITI-18)              | Attribute                            | Opt | Mult |
    /// |--------------------------------------|--------------------------------------|-----|------|
    /// | $XDSDocumentEntryConfidentialityCode | XDSDocumentEntry.confidentialityCode | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryConfidentialityCode(
        this IEnumerable<ExtrinsicObjectType> source, List<string[]> confidentialityGroups)
    {
        if (confidentialityGroups == null || confidentialityGroups.Count == 0) return source; // Optional field, return everything if not specified

        return source.Where(eo =>
        {
            // Get all the confidentiality codes for the current ExtrinsicObject (eo)
            var confidentialityCodesForExtrinsicObject = eo.Classification
                .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
                .Select(cf => cf.NodeRepresentation + "^^" + cf.GetSlot(Constants.Xds.SlotNames.CodingScheme).FirstOrDefault()?.GetFirstValue())
                .ToArray(); // Collect all confidentiality codes into an array

            // Check if all groups match (AND logic for groups)
            return confidentialityGroups.All(singleConfidentialityGroup =>
                // OR logic inside each group: does any code from the group match any of the extracted confidentiality codes?
                singleConfidentialityGroup.Any(confCode => confidentialityCodesForExtrinsicObject.Contains(confCode))
            );
        });
    }

    /// <summary>
    /// | Parameter Name (ITI-18)       | Attribute               | Opt | Mult |
    /// |-------------------------------|-------------------------|-----|------|
    /// | $XDSDocumentEntryAuthorPerson | XDSDocumentEntry.author | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryAuthorPerson(
        this IEnumerable<ExtrinsicObjectType> source, List<string[]> authorPersons)
    {
        if (authorPersons == null || authorPersons.Count == 0) return source; // Optional field, return everything if not specified

        return source.Where(eo =>
        {
            // Get all the author persons for the current ExtrinsicObject (eo)
            var authorsFromExtrinsicObject = eo.Classification
                .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.Author)
                .SelectMany(cf => cf.GetSlot(Constants.Xds.SlotNames.AuthorPerson)
                    .Select(s => s.GetFirstValue()))
                .ToArray(); // Collect all author persons into an array

            // Check if all groups match (AND logic for groups)
            return authorPersons.All(authorPersonGroup =>
                // OR logic inside each group
                authorPersonGroup.Any(authorPerson => authorsFromExtrinsicObject.Contains(authorPerson)));
        });
    }

    /// <summary>
    /// | Parameter Name (ITI-18)     | Attribute                   | Opt | Mult |
    /// |-----------------------------|-----------------------------|-----|------|
    /// | $XDSDocumentEntryFormatCode | XDSDocumentEntry.formatCode | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentFormatCode(
        this IEnumerable<ExtrinsicObjectType> source, List<string[]> formatCodes)
    {
        if (formatCodes == null || formatCodes.Count == 0) return source; // Optional field, return everything if not specified
        return source.Where(eo =>
        {
            // Get all the author persons for the current ExtrinsicObject (eo)
            var formatCodesFromExtrinsicObject = eo.Classification
                .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.FormatCode)
                .SelectMany(cf => cf.GetSlot(Constants.Xds.SlotNames.CodingScheme)
                    .Select(s => s.GetFirstValue()))
                .ToArray(); // Collect all author persons into an array

            // Check if all groups match (AND logic for groups)
            return formatCodes.All(formatCodeGroup =>
                // OR logic inside each group
                formatCodeGroup.Any(formatCode => formatCodesFromExtrinsicObject.Contains(formatCode)));
        });
    }

    /// <summary>
    /// | Parameter Name (ITI-18) | Attribute                           | Opt | Mult |
    /// |-------------------------|-------------------------------------|-----|------|
    /// | $XDSDocumentEntryStatus | XDSDocumentEntry.availabilityStatus | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryStatus(
        this IEnumerable<ExtrinsicObjectType> source, List<string[]> statuses)
    {
        if (statuses == null || statuses.Count == 0) return source; // Optional field, return everything if not specified

        return source.Where(eo =>
            statuses.All(group => group.Any(status => status == eo.Status)) // AND logic for groups, OR logic inside groups
        );
    }

    /// <summary>
    /// | Parameter Name (ITI-18) | Attribute                   | Opt | Mult |
    /// |-------------------------|-----------------------------|-----|------|
    /// | $XDSDocumentEntryType   | XDSDocumentEntry.objectType | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryType(
        this IEnumerable<ExtrinsicObjectType> source, List<string[]> typeCodes)
    {
        if (typeCodes == null || typeCodes.Count == 0) return source; // Optional field, return everything if not specified
        return source.Where(eo =>
        {
            var typeCodesFromExtrinsicObject = eo.Classification
                .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.FormatCode)
                .SelectMany(cf => cf.GetSlot(Constants.Xds.SlotNames.CodingScheme)
                    .Select(s => s.GetFirstValue()))
                .ToArray(); // Collect all author persons into an array

            // Check if all groups match (AND logic for groups)
            return typeCodes.All(typeCodeGroup =>
                // OR logic inside each group
                typeCodeGroup.Any(typeCode => typeCodesFromExtrinsicObject.Contains(typeCode)));
        });
    }

    public static string[] GetValues(this SlotType[] slotTypes, bool codeMultipleValues = true)
    {
        if (slotTypes == null || slotTypes.Length == 0)
        {
            return Array.Empty<string>();
        }

        return slotTypes
            .SelectMany(slot => slot.GetValues(codeMultipleValues) ?? [])
            .ToArray();
    }

    public static List<string[]> GetValuesGrouped(this SlotType[] slotTypes, bool codeMultipleValues = true)
    {
        if (slotTypes == null || slotTypes.Length == 0)
        {
            return new List<string[]>();
        }

        return slotTypes
            .Select(slot => slot.GetValues(codeMultipleValues) ?? Array.Empty<string>())
            .Where(values => values.Length > 0)
            .ToList();
    }

}

public class SearchCodedValues
{

}
