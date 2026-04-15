namespace XcaXds.Commons.Models;

public class XdsValidationResponse
{
    public XdsValidationResponse() { }

    public XdsValidationResponse(string errorMessage)
    {
        Message = errorMessage;
        IsValid = false;
    }

    public bool IsValid { get; set; }
    public string? Message { get; set; }
}