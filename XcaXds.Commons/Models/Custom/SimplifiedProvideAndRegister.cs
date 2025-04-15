using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XcaXds.Commons.Models.Custom;

public class SimplifiedProvideAndRegister
{
    public string SubmissionSetStatus { get; set; }
    public DateTime CreationTime { get; set; }
    public string LanguageCode { get; set; }
    public DateTime ServiceStartTime { get; set; }
    public DateTime ServiceStopTime { get; set; }
    public string SourcePatientId { get; set; }
    public SourcePatientInfo SourcePatientInfo { get; set; }
    public string RepositoryUniqueId { get; set; }
    public int Size { get; set; }
    public string SubmissionTitle { get; set; }
    public List<string> ClassificationAuthorPerson { get; set; }
    public List<AuthorInstitution> ClassificationAuthorInstitution { get; set; }
    public string ClassificationFormatCode { get; set; }
    public string ClassificationHealthCareFacilityTypeCode { get; set; }
    public string ClassificationPracticeSettingCode { get; set; }
    public string ClassificationDocumentClassCode { get; set; }
    public string ClassificationDocumentTypeCode { get; set; }
    public string ClassificationConfidentialityCode { get; set; }
    public string ExternalIdentifierUniqueId { get; set; }
    public string ExternalIdentifierPatientId { get; set; }


}
