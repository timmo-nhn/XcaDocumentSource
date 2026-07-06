namespace XcaXds.Commons.Interfaces;

public class VirusScanResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    public static VirusScanResult Success(string message) => new() { IsSuccess = true, Message = message };
    public static VirusScanResult Failure(string message) => new() { IsSuccess = false, Message = message };
}

public class VirusScanResult<TResult> : VirusScanResult
{
    public TResult? ScannerResult { get; set; }

    public static VirusScanResult<TResult> Success(string message, TResult result) =>
        new() { IsSuccess = true, Message = message, ScannerResult = result };

    public static VirusScanResult<TResult> Failure(string message, TResult? result = default) =>
        new() { IsSuccess = false, Message = message, ScannerResult = result };
}