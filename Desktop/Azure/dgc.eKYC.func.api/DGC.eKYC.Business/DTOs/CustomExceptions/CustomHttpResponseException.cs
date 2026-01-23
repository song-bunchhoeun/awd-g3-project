namespace DGC.eKYC.Business.DTOs.CustomExceptions;

public class CustomHttpResponseException(
    int statusCode, 
    object? value = null, 
    int? retryAfter = null)
    : Exception
{
    public int StatusCode { get; } = statusCode;

    public int? RetryAfter { get; } = retryAfter;

    public object? Value { get; } = value;
}