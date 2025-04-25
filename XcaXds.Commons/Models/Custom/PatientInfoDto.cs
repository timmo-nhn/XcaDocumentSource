using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XcaXds.Commons.Models.Custom;

public class PatientInfoDto
{
    public string PatientId { get; set; }
    public List<string> SourcePatientInfo { get; set; }

}
