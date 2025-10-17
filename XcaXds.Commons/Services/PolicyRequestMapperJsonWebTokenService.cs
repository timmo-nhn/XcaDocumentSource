using Abc.Xacml.Context;
using Microsoft.AspNetCore.Http;

namespace XcaXds.Commons.Services;


public class PolicyRequestMapperJsonWebTokenService
{
    //FIXME maybe some day, do something about JWT aswell?!
    public static XacmlContextRequest? GetXacml20RequestFromJsonWebToken(IHeaderDictionary headers)
    {
        XacmlContextRequest contextRequest = null;
        return contextRequest;
    }
}