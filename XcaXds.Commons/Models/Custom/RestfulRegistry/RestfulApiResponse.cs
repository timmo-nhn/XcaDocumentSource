﻿namespace XcaXds.Commons.Models.Custom.RestfulRegistry;


public class RestfulApiResponse
{
    public bool Success { get; set; } = true;

    public List<Error> Errors { get; set; }


    public void AddError(string code, string message)
    {
        Success = false;
        Errors.Add(new()
        {
            Code = string.IsNullOrWhiteSpace(code) ? null : code,
            Message = string.IsNullOrWhiteSpace(message) ? null : message

        });
    }
}

public class Error
{
    public string? Code { get; set; }
    public string? Message { get; set; }
}