﻿using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Hl7.DataType;

namespace XcaXds.Commons.Extensions;

public static class FindDocumentEntry
{
    public static IEnumerable<DocumentEntryDto> ByDocumentEntryPatientId(
        this IEnumerable<DocumentEntryDto> source, CX? patientId)
    {
        if (string.IsNullOrWhiteSpace(patientId?.IdNumber)) return Enumerable.Empty<DocumentEntryDto>();
        return source
            .Where(eo => eo?.SourcePatientInfo?.PatientId?.Id == patientId.IdNumber &&
            eo?.SourcePatientInfo?.PatientId?.System.NoUrn() == patientId.AssigningAuthority.UniversalId.NoUrn());
    }

    public static IEnumerable<DocumentEntryDto> ByDocumentEntryStatus(
        this IEnumerable<DocumentEntryDto> source, string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return source;
        return source
            .Where(eo => string.Equals(eo?.AvailabilityStatus, "urn:oasis:names:tc:ebxml-regrep:StatusType:" + status,StringComparison.InvariantCultureIgnoreCase));
    }
}

public static class Extensions
{
    public static IEnumerable<RegistryObjectDto> Replace(this IEnumerable<RegistryObjectDto> source, RegistryObjectDto oldValue, RegistryObjectDto newValue)
    {
        if (oldValue == null || newValue == null) return source;

        return source.Select(regObj => regObj.Id == oldValue.Id ? newValue : regObj);
    }

    public static List<RegistryObjectDto> DeprecateEntry(this List<RegistryObjectDto> registry, string idtoDeprecate)
    {
        return registry.Select(entry =>
        {
            if (entry is DocumentEntryDto docEntry && docEntry.Id == idtoDeprecate)
            {
                docEntry.AvailabilityStatus = Constants.Xds.StatusValues.Deprecated;
            }
            return entry;
        }).ToList();
    }

}
