using System.Xml.Serialization;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Extensions;

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

    public static SoapRequestResult<SoapEnvelope> CreateSoapRegistryResponse(RegistryResponseType message, bool fault = false)
    {
        var resultEnvelope = new SoapRequestResult<SoapEnvelope>()
        {
            Value = new SoapEnvelope()
            {
                Header = new()
                {
                    Action = fault is true ? Constants.Soap.Namespaces.AddressingSoapFault: Constants.Soap.Namespaces.Addressing,
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
                if (soapEnvelope.Body.RegistryResponse.RegistryErrorList is null)
                {
                    resultEnvelope.IsSuccess = true;
                }
                else
                {
                    var isSuccess = bool.Equals(false, soapEnvelope.Body.RegistryResponse.RegistryErrorList.RegistryError
                    .Any(re => re.Severity == Constants.Xds.ErrorSeverity.Error));

                    resultEnvelope.IsSuccess = isSuccess;
                }
            }
        }

        return resultEnvelope;
    }

    //public static SoapEnvelope CreateSoapTypedResponse<T>(SoapEnvelope message) where T: class
    //{
    //    var resultEnvelope = new SoapEnvelope()
    //    {
    //        Header = new()
    //        {
    //            Action = Constants.Soap.Namespaces.Addressing,
    //        },
    //        Body = new SoapBody()
    //    };

    //    // Get property of SoapBody which matches T
    //    var propertyInfo = typeof(SoapBody).GetProperties()
    //        .FirstOrDefault(p => p.PropertyType == typeof(T));

    //    if (propertyInfo != null && propertyInfo.CanWrite)
    //    {
    //        var bodyProperty = message.Body.GetType().GetProperty(propertyInfo.Name);

    //        if (bodyProperty != null)
    //        {
    //            var value = bodyProperty.GetValue(message.Body);

    //            propertyInfo.SetValue(resultEnvelope.Body, value);
    //        }
    //    }

    //    return resultEnvelope;
    //}

    public static T DeepClone<T>(T obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        XmlSerializer serializer = new XmlSerializer(typeof(T));

        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new StreamWriter(memoryStream))
            {
                serializer.Serialize(writer, obj);
                writer.Flush(); // Ensure all data is written

                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(memoryStream))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
        }
    }


    public static string GetResponseAction(string action)
    {
        throw new NotImplementedException();
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
