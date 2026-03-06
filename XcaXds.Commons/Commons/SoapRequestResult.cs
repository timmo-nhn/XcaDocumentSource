using XcaXds.Commons.Models.Soap;
namespace XcaXds.Commons.Commons;

public class SoapRequestResult<T> where T : notnull
{
    public bool IsSuccess { get; set; }
    public T Value { get; set; } = default!;
    public Fault? FaultResult { get; set; }


    public SoapRequestResult<T> Success(T result)
    {
        var soapResult = new SoapRequestResult<T>
        {
            IsSuccess = true,
            Value = result
        };

        return soapResult;
    }

    public SoapRequestResult<T> Fault(T fault)
    {
        var soapResult = new SoapRequestResult<T>
        {
            IsSuccess = false,
            Value = fault,
        };

        return soapResult;
    }
}
