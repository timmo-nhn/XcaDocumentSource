using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Extensions
{
    public static class SoapExtensions
    {
        public static SoapRequestResult<SoapEnvelope> CreateSoapFault(string fault, string faultCode, string? subCode = null)
        {
            var resultEnvelope = new SoapRequestResult<SoapEnvelope>()
            {
                Value = new SoapEnvelope()
                {
                    Header = new()
                    {
                        Action = Constants.Soap.Namespaces.AddressingSoapFault,
                    },
                    Body = new()
                    {
                        Fault = new()
                        {
                            Code = new()
                            {
                                Value = faultCode,
                                Subcode = subCode != null ? new() { Value = subCode } : null
                            },
                            Reason = new()
                            {
                                Text = fault
                            }
                        }
                    }
                }
            };
            //var serializer = new SoapXmlSerializer();
            //var result = serializer.SerializeSoapMessageToXmlString(envelope);
            return resultEnvelope;
        }
        
        public static SoapRequestResult<SoapEnvelope> CreateSoapRegistryResponse(RegistryResponseType message)
        {
            var resultEnvelope = new SoapRequestResult<SoapEnvelope>()
            {
                Value = new SoapEnvelope()
                {
                    Header = new()
                    {
                        Action = Constants.Soap.Namespaces.Addressing,
                    },
                    Body = new()
                    {
                        RegistryResponse = message
                    }
                }
            };
            if (resultEnvelope.Value is SoapEnvelope soapEnvelope)
            {
                if (soapEnvelope.Body.RegistryResponse is not null)
                {
                    // Base Success property on whether Value...RegistryErrorList has any errors
                    resultEnvelope.IsSuccess = bool.Equals(false, soapEnvelope.Body.RegistryResponse.RegistryErrorList.RegistryError
                        .Any(re => re.Severity == Constants.Xds.ErrorSeverity.Error));
                }
            }

            //var serializer = new SoapXmlSerializer();
            //var result = serializer.SerializeSoapMessageToXmlString(envelope);
            return resultEnvelope;
        }

        //public static SoapEnvelope CreateRegistryResponse()
        //{
        //    var registryResponse = new SoapEnvelope()
        //    {
        //        Header = new()
        //        {
        //            Action = Constants.Soap.Namespaces.Addressing,
        //        },
        //        Body = new()
        //        {
        //            RegistryResponse = new() { Status = ResponseStatusType.Success.ToString(), ResponseSlotList = [new() { }] }
        //        }
        //    };
        //    //var serializer = new SoapXmlSerializer();
        //    //var result = serializer.SerializeSoapMessageToXmlString(envelope);
        //    return registryResponse;
        //}

    }
}
