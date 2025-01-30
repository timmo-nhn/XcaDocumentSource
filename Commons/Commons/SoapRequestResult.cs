using XcaXds.Commons.Models.Soap;
namespace XcaXds.Commons;

public class SoapRequestResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public Fault? FaultResult { get; set; }

    public static SoapRequestResult<T> Success(T result)
    {
        var soapResult = new SoapRequestResult<T>
        {
            IsSuccess = true,
            Value = result
        };

        return soapResult;
    }

    public static SoapRequestResult<SoapEnvelope> Fault(Fault fault)
    {
        var soapFaultEnvelope = new SoapEnvelope()
        {
            Header = new()
            {
                Action = Constants.Soap.Namespaces.AddressingSoapFault
            },
            Body = new() { Fault = fault }
        };

        var soapResult = new SoapRequestResult<SoapEnvelope>
        {
            IsSuccess = false,
            Value = soapFaultEnvelope,
            FaultResult = fault
        };

        return soapResult;
    }
}
