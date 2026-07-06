namespace XcaXds.Commons.Models.Custom;

/// <summary>
/// Represents a response from an operation, typically used to encapsulate the result of a service call or method execution.
/// (ie. Database operations)
/// This class can be extended to include additional properties such as status codes, messages, or data payloads as needed.
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
}

public static class OperationResponseExtensions
{
    public static OperationResponse<TResult> SetResultObject<TResult>(this OperationResponse response, TResult result)
    {
        return new()
        {
            IsSuccess = response.IsSuccess,
            Message = response.Message,
            Value = result
        };
    }
}