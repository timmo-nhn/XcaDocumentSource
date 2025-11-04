using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using XcaXds.Commons.Models.TrustFramework;

namespace XcaXds.Commons.Extensions;

public static class TillitsrammeverkParser
{
    public static TrustFrameworkModel? ParseFromClaim(string claimValue)
    {
        return JsonSerializer.Deserialize<TrustFrameworkModel>(claimValue);
    }
}
