using System;
using System.Globalization;
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
/// When a property has Mult value M, the method input is a string array. If not, its just string
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
    /// | Parameter Name (ITI-18)      | Attribute                   | Opt | Mult |
    /// |------------------------------|-----------------------------|-----|------|
    /// | $XDSDocumentEntryClassCode   | XDSDocumentEntry.classCode  | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryClassCode(
        this IEnumerable<ExtrinsicObjectType> source, string[] classCodes)
    {
        if (classCodes.Length == 0) return source; // Optional field, return everything if not specified
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.ClassCode)
            .Any(co => classCodes.Contains(co.NodeRepresentation)));
    }

    /// <summary>
    /// | Parameter Name (ITI-18)      | Attribute                   | Opt | Mult |
    /// |------------------------------|-----------------------------|-----|------|
    /// | $XDSDocumentEntryTypeCode    | XDSDocumentEntry.typeCode   | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryTypeCode(
        this IEnumerable<ExtrinsicObjectType> source, string[] typeCodes)
    {
        if (typeCodes.Length == 0) return source; // Optional field, return everything if not specified
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.TypeCode)
            .Any(co => typeCodes.Contains(co.NodeRepresentation)));
    }

    /// <summary>
    /// | Parameter Name (ITI-18)               | Attribute                             | Opt | Mult |
    /// |---------------------------------------|---------------------------------------|-----|------|
    /// | $XDSDocumentEntryPracticeSettingCode  | XDSDocumentEntry.practiceSettingCode  | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryPracticeSettingCode(
        this IEnumerable<ExtrinsicObjectType> source, string[] practiceSettingCodes)
    {
        if (practiceSettingCodes.Length == 0) return source; // Optional field, return everything if not specified
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.PracticeSettingCode)
            .Any(co => practiceSettingCodes.Contains(co.NodeRepresentation)));
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
        this IEnumerable<ExtrinsicObjectType> source, string[] healthcareFacilityTypeCodes)
    {
        if (healthcareFacilityTypeCodes.Length == 0) return source;
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.HealthCareFacilityTypeCode)
            .Any(co => healthcareFacilityTypeCodes.Contains(co.NodeRepresentation)));
    }

    /// <summary>
    /// | Parameter Name (ITI-18)        | Attribute                      | Opt | Mult |
    /// |--------------------------------|--------------------------------|-----|------|
    /// | $XDSDocumentEntryEventCodeList | XDSDocumentEntry.eventCodeList | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryEventCodeList(
        this IEnumerable<ExtrinsicObjectType> source, string[] eventCodeList)
    {
        if (eventCodeList.Length == 0) return source;

        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.EventCodeList)
            .Any(co => eventCodeList.Contains(co.NodeRepresentation)));
    }

    /// <summary>
    /// | Parameter Name (ITI-18)              | Attribute                            | Opt | Mult |
    /// |--------------------------------------|--------------------------------------|-----|------|
    /// | $XDSDocumentEntryConfidentialityCode | XDSDocumentEntry.confidentialityCode | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryConfidentialityCode(
        this IEnumerable<ExtrinsicObjectType> source, string[] confidentialityCodes)
    {
        if (confidentialityCodes.Length == 0) return source; // Optional field, return everything if not specified
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.ConfidentialityCode)
            .Select(cf =>
            {
                var ccode = cf.NodeRepresentation + "^^" + cf.GetSlot("codingScheme").First().GetFirstValue();
                return ccode;
            })
            .Any(code => confidentialityCodes.Contains(code)));
    }

    /// <summary>
    /// | Parameter Name (ITI-18)       | Attribute               | Opt | Mult |
    /// |-------------------------------|-------------------------|-----|------|
    /// | $XDSDocumentEntryAuthorPerson | XDSDocumentEntry.author | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryAuthorPerson(
        this IEnumerable<ExtrinsicObjectType> source, string[] authorPersons)
    {
        if (authorPersons.Length == 0) return source; // Optional field, return everything if not specified
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.Author)
            .Select(cf => cf.GetSlot(Constants.Xds.SlotNames.AuthorPerson).First().GetFirstValue())
            .Any(author => authorPersons.Contains(author)));
    }

    /// <summary>
    /// | Parameter Name (ITI-18)     | Attribute                   | Opt | Mult |
    /// |-----------------------------|-----------------------------|-----|------|
    /// | $XDSDocumentEntryFormatCode | XDSDocumentEntry.formatCode | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentFormatCode(
        this IEnumerable<ExtrinsicObjectType> source, string[] formatCodes)
    {
        if (formatCodes.Length == 0) return Enumerable.Empty<ExtrinsicObjectType>();  // Required field, return nothing if not specified
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.FormatCode)
            .Any(co => formatCodes.Contains(co.NodeRepresentation)));
    }

    /// <summary>
    /// | Parameter Name (ITI-18) | Attribute                           | Opt | Mult |
    /// |-------------------------|-------------------------------------|-----|------|
    /// | $XDSDocumentEntryStatus | XDSDocumentEntry.availabilityStatus | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryStatus(
        this IEnumerable<ExtrinsicObjectType> source, string[] statuses)
    {
        if (statuses.Length == 0) return Enumerable.Empty<ExtrinsicObjectType>();  // Required field, return nothing if not specified
        return source.Where(eo => statuses.Contains(eo.Status));
    }

    /// <summary>
    /// | Parameter Name (ITI-18) | Attribute                   | Opt | Mult |
    /// |-------------------------|-----------------------------|-----|------|
    /// | $XDSDocumentEntryType   | XDSDocumentEntry.objectType | O   | M    |
    /// </summary>
    public static IEnumerable<ExtrinsicObjectType> ByDocumentEntryType(
        this IEnumerable<ExtrinsicObjectType> source, string[] typeCodes)
    {
        if (typeCodes.Length == 0) return Enumerable.Empty<ExtrinsicObjectType>();  // Required field, return nothing if not specified
        return source.Where(eo => eo.Classification
            .Where(cf => cf.ClassificationScheme == Constants.Xds.Uuids.DocumentEntry.TypeCode)
            .Any(co => typeCodes.Contains(co.NodeRepresentation)));
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
}

public class SearchCodedValues
{

}
