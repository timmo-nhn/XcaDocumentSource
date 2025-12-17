using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Models.Custom;

public class BusinessLogicParameters
{
    public CodedValue? QueriedSubject { get; set; }
    public int QueriedSubjectAge { get; set; }
    public CodedValue? Purpose { get; set; }
    public CodedValue? Subject { get; set; }
    public CodedValue? SubjectOrganization { get; set; }
    public int SubjectAge { get; set; }
    public CodedValue? Resource { get; set; }
    public int ResourceAge { get; set; }
    public CodedValue? Role { get; set; }
    public string? Acp { get; set; }
    public string? Bppc {  get; set; }
}