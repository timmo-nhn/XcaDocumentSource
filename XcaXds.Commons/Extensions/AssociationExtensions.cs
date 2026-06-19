using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared;
using XcaXds.Shared.Extensions;

namespace XcaXds.Commons.Extensions;

public static class AssociationExtensions
{
    /// <summary>
    /// Try to mark ExtrinsicObject as Deprecated. Will not show up in ITI-18 requests
    /// This is done like the TryParse pattern due to the deferred nature of the IEnumerable.
    /// Attempting to manipulate an item in the IEnumerable directly will cause materialization issues.
    ///     These issues arise because of the deferred execution of fetching database entities (yield returns), 
    ///     and the DbRegistryObject => RegistryObjectDto => IdentifiableType transformation process.
    /// </summary>
    public static bool TryDeprecateDocumentEntry(this IEnumerable<IdentifiableType> source, string id, out ExtrinsicObjectType? deprecatedEntry)
    {
        deprecatedEntry = null;
        if (id == null) return false;

        var documentEntryToDeprecate = source.OfType<ExtrinsicObjectType>().FirstOrDefault(eo => eo.Id?.NoUrn() == id.NoUrn());

        if (documentEntryToDeprecate == null) return false;

        documentEntryToDeprecate.Status = Constants.Xds.StatusValues.Deprecated;
        deprecatedEntry = documentEntryToDeprecate;
        return true;
    }
}
