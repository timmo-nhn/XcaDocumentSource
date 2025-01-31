using XcaXds.Commons;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.WebService.Extensions
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
            return resultEnvelope;
        }



        public static SoapEnvelope CreateSoapTypedResponse<T>(SoapEnvelope message)
        {
            var resultEnvelope = new SoapEnvelope()
            {
                Header = new()
                {
                    Action = Constants.Soap.Namespaces.Addressing,
                },
                Body = new SoapBody()
            };

            // Get property of SoapBody which matches T
            var propertyInfo = typeof(SoapBody).GetProperties()
                .FirstOrDefault(p => p.PropertyType == typeof(T));

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                var bodyProperty = message.Body.GetType().GetProperty(propertyInfo.Name);

                if (bodyProperty != null)
                {
                    var value = bodyProperty.GetValue(message.Body);

                    propertyInfo.SetValue(resultEnvelope.Body, value);
                }
            }

            return resultEnvelope;
        }
    }
}
