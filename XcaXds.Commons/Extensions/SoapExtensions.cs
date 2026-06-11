using System.Xml.Serialization;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.Actions;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Shared.Commons;

namespace XcaXds.Commons.Extensions;

public static class SoapExtensions
{
    public static object? CreateAsyncAcceptedMessage(SoapEnvelope soapEnvelope)
    {
        throw new NotImplementedException();
    }

    public static RegistryErrorList? RegistryErrorsFromSoapEnvelope(SoapEnvelope? soapEnvelopeResponse)
    {
        var registryErrors = soapEnvelopeResponse?.Body.RegistryResponse?.RegistryErrorList;
        var queryErrors = soapEnvelopeResponse?.Body.AdhocQueryResponse?.RegistryErrorList;
        var retrieveErrors = soapEnvelopeResponse?.Body.RetrieveDocumentSetResponse?.RegistryResponse?.RegistryErrorList;
        var registerErrors = soapEnvelopeResponse?.Body.RegisterDocumentSetResponse?.RegistryErrorList;
        var provideErrors = soapEnvelopeResponse?.Body.ProvideAndRegisterDocumentSetResponse?.RegistryResponse?.RegistryErrorList;

        RegistryErrorType[] allErrors = [..registryErrors?.RegistryError ?? [], ..queryErrors?.RegistryError ?? [], ..retrieveErrors?.RegistryError ?? [], ..registerErrors?.RegistryError ?? [], ..provideErrors?.RegistryError ?? []];

        if (allErrors.Length == 0) return null;

        var highestSeverity = allErrors.MaxBy(err => err.GetSeverityLevel())?.Severity;


        return new RegistryErrorList()
        {
            HighestSeverity = highestSeverity,
            RegistryError = allErrors
        };
    }


    public static SoapRequestResult<SoapEnvelope> CreateSoapFault(string faultCode, string? subCode = null, string? detail = null, string? faultReason = null)
    {
        var resultEnvelope = new SoapRequestResult<SoapEnvelope>()
        {
            Value = new SoapEnvelope()
            {
                Header = new SoapHeader()
                {
                    Action = Constants.Soap.Namespaces.AddressingSoapFault,
                },
                Body = new SoapBody()
                {
                    Fault = new Fault()
                    {
                        Code = new Code()
                        {
                            Value = faultCode,
                            Subcode = string.IsNullOrWhiteSpace(subCode) ? null : new() { Value = subCode ?? string.Empty }
                        },
                        Reason = new Reason()
                        {
                            Text = string.IsNullOrEmpty(faultReason) ? "soapenv:Reciever" : faultReason
                        },
                        Detail = string.IsNullOrWhiteSpace(detail) ? null : new Detail() { Value = new() { Action = detail } }
                    }
                }
            }
        };

        return resultEnvelope;
    }

    public static SoapRequestResult<SoapEnvelope> CreateSoapResult(this RegistryResponseType message)
    {
        return CreateSoapResultRegistryResponse(message);
    }

    public static SoapRequestResult<SoapEnvelope> CreateSoapResultRegistryResponse(RegistryResponseType message)
    {
        var resultEnvelope = new SoapRequestResult<SoapEnvelope>()
        {
            Value = new SoapEnvelope()
            {
                Header = new SoapHeader(),
                Body = new SoapBody()
                {
                    RegistryResponse = message
                }
            }
        };

        if (resultEnvelope.Value.Body.RegistryResponse != null)
        {
            var isSuccess = bool.Equals(false, (resultEnvelope.Value.Body.RegistryResponse.RegistryErrorList?.RegistryError?
                .Any(re => re.Severity == Constants.Xds.ErrorSeverity.Error) ?? false));
            resultEnvelope.Value.Header.Action = isSuccess == true ? Constants.Soap.Namespaces.Addressing : Constants.Soap.Namespaces.AddressingSoapFault;
            resultEnvelope.IsSuccess = isSuccess;
        }

        return resultEnvelope;
    }

    public static SoapRequestResult<SoapEnvelope> CreateSoapResultResponse(SoapEnvelope message)
    {
        var resultEnvelope = new SoapRequestResult<SoapEnvelope>()
        {
            Value = new SoapEnvelope()
            {
                Header = message.Header,
                Body = message.Body
            }
        };

        resultEnvelope.Value.Header.Action = message.Header.Action;
        resultEnvelope.Value.Header.RelatesTo = message.Header.MessageId;

        // Base Success property on whether Value...RegistryErrorList has any errors
        if (resultEnvelope.Value.Body.RegistryResponse?.RegistryErrorList == null)
        {
            resultEnvelope.IsSuccess = true;
        }
        else
        {
            var isSuccess = bool.Equals(false, resultEnvelope.Value.Body.RegistryResponse.RegistryErrorList.RegistryError?
                .Any(re => re.Severity == Constants.Xds.ErrorSeverity.Error));
            resultEnvelope.Value.Header?.Action = isSuccess == true ? Constants.Soap.Namespaces.Addressing : Constants.Soap.Namespaces.AddressingSoapFault;
            resultEnvelope.IsSuccess = isSuccess;
        }

        return resultEnvelope;
    }

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
                    return (T)serializer.Deserialize(reader)!;
                }
            }
        }
    }

    public static SoapHeader GetResponseHeaderFromRequest(SoapEnvelope envelope)
    {
        return new SoapHeader()
        {
            Action = envelope.GetCorrespondingResponseAction(),
        };
    }

    public static void PutRegistryResponseInTheCorrectPlaceAccordingToSoapAction(SoapEnvelope soapEnvelopeResponse, RegistryResponseType registryResponse)
    {
        switch (soapEnvelopeResponse.Header.Action)
        {
            case Constants.Xds.OperationContract.Iti18Reply:
            case Constants.Xds.OperationContract.Iti18ReplyAsync:
            case Constants.Xds.OperationContract.Iti38Reply:
            case Constants.Xds.OperationContract.Iti38ReplyAsync:
                soapEnvelopeResponse.Body ??= new();
                soapEnvelopeResponse.Body.AdhocQueryResponse ??= new();
                soapEnvelopeResponse.Body.AdhocQueryResponse.RegistryErrorList = registryResponse.RegistryErrorList;
                soapEnvelopeResponse.Body.AdhocQueryResponse.Status = registryResponse.Status;
                break;

            case Constants.Xds.OperationContract.Iti43Reply:
            case Constants.Xds.OperationContract.Iti43ReplyAsync:
            case Constants.Xds.OperationContract.Iti39Reply:
            case Constants.Xds.OperationContract.Iti39ReplyAsync:
                soapEnvelopeResponse.Body ??= new();
                soapEnvelopeResponse.Body.RetrieveDocumentSetResponse ??= new();
                soapEnvelopeResponse.Body.RetrieveDocumentSetResponse.RegistryResponse = registryResponse;
                break;

            case Constants.Xds.OperationContract.Iti41Reply:
            case Constants.Xds.OperationContract.Iti41ReplyAsync:
                soapEnvelopeResponse.Body ??= new();
                soapEnvelopeResponse.Body.ProvideAndRegisterDocumentSetResponse ??= new();
                soapEnvelopeResponse.Body.ProvideAndRegisterDocumentSetResponse.RegistryResponse = registryResponse;
                break;

            case Constants.Xds.OperationContract.Iti42Reply:
            case Constants.Xds.OperationContract.Iti42ReplyAsync:
                soapEnvelopeResponse.Body ??= new();
                soapEnvelopeResponse.Body.RegisterDocumentSetResponse ??= new();
                soapEnvelopeResponse.Body.RegisterDocumentSetResponse.RegistryErrorList = registryResponse.RegistryErrorList;
                soapEnvelopeResponse.Body.RegisterDocumentSetResponse.Status = registryResponse.Status;
                break;

            default:
                soapEnvelopeResponse.Body ??= new();
                soapEnvelopeResponse.Body.RegistryResponse = registryResponse;
                break;
        }
    }

    public static (string code, string display) GetTransactionCodeFromSoapAction(string soapAction)
    {
        return soapAction switch
        {
            "ITI-65" => ("ITI-65", "Provide Bundle"),
            Constants.Xds.OperationContract.Iti18Action => ("ITI-18", "Registry Stored Query"),
            Constants.Xds.OperationContract.Iti38Action => ("ITI-38", "Cross Gateway Query"),
            Constants.Xds.OperationContract.Iti39Action => ("ITI-39", "Cross Gateway Retrieve"),
            Constants.Xds.OperationContract.Iti41Action => ("ITI-41", "Provide and Register Document Set"),
            Constants.Xds.OperationContract.Iti42Action => ("ITI-42", "Register Document Set"),
            Constants.Xds.OperationContract.Iti43Action => ("ITI-43", "Retrieve Document Set"),
            Constants.Xds.OperationContract.Iti62Action => ("ITI-62", "Delete Document Set"),
            Constants.Xds.OperationContract.Iti86Action => ("ITI-86", "Remove Documents"),
            _ => ("ITI-18", "Registry Stored Query")
        };
    }

    // public static SuccessType SoapEnvelopeIsSuccess(SoapEnvelope? soapEnvelope)
    // {
    //     var errors = RegistryErrorsFromSoapEnvelope(soapEnvelope);

    //     var 
    // }
}