using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Extensions;

public static class AssociationExtensions
{
    /// <summary>
    /// Mark ExtrinsicObject as Deprecated. Will not show up in ITI-18 requests
    /// </summary>
    public static void DeprecateDocumentEntry(
        this IEnumerable<IdentifiableType> source, string id, out bool success)
    {
        success = false;
        if (id == null) return;

        var documentEntryToDeprecate = source.OfType<ExtrinsicObjectType>().FirstOrDefault(eo => eo.Id?.NoUrn() == id.NoUrn());

        if (documentEntryToDeprecate == null) return;

        documentEntryToDeprecate.Status = Constants.Xds.StatusValues.Deprecated;
        success = true;
    }
}
