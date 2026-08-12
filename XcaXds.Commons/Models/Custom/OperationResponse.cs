namespace XcaXds.Commons.Models.Custom;

/// <summary>
/// Represents a response from an operation, typically used to encapsulate the result of a service call or method execution.
/// (ie. Database operations)
/// </summary>
public class OperationResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    public static OperationResponse Success(string? message = null)
    {
        return new OperationResponse { IsSuccess = true, Message = message };
    }

    public static OperationResponse Failure(string message)
    {
        return new OperationResponse() { Message = message };
    }
}

public class OperationResponse<TResult> : OperationResponse
{
    public TResult? Value { get; set; }

    public static OperationResponse<TResult> Success(TResult result, string? message = null)
    {
        return new OperationResponse<TResult> { IsSuccess = true, Message = message, Value = result };
    }

    public OperationResponse<TResult> SetResultObject(TResult result)
    {
        return new OperationResponse<TResult>
        {
            IsSuccess = IsSuccess,
            Message = Message,
            Value = result
        };
    }
}
