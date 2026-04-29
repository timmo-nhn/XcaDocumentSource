namespace XcaXds.Commons.Attributes;

/// <summary>
/// Declares an endpoint as being eligible for export of ATNA log (Audit Trail and Node Authentication) to the AtnaLogExporter endpoint defined in the application configuration.<para/>
/// The endpoint may have to add items to the <pre>Httpcontext</pre> to create a full ATNA-message <para/>
/// Resources: <para/>
/// - <a href="https://build.fhir.org/auditevent.html">Resource AuditEvent - build.fhir.org</a><para/>
/// - <a href="https://profiles.ihe.net/ITI/TF/Volume1/ch-9.html">Audit Trail and Node Authentication (ATNA) Profile - profiles.ihe.net</a><para/>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ExportsAtnaAuditLogAttribute : Attribute
{
    public bool Enabled { get; set; } = true;
}