using Hl7.Fhir.Model;
using System.Xml.Serialization;
using XcaXds.Shared;

namespace XcaXds.Commons.Models.Soap.XdsTypes;

[Serializable]
[XmlType(Namespace = Constants.Xds.Namespaces.Rim)]
public partial class AdhocQueryType : RegistryObjectType
{
    public void SetSlotValue(string patientId, string? bundlePatientIdCx)
    {
        Slot ??= [];
        Slot.FirstOrDefault(s => s.Name == patientId)?.SetValue(bundlePatientIdCx);
    }
}
