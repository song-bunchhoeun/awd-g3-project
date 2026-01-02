using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DGC.eKYC.Business.DTOs.Errors;

public class ErrorResponse(string code, string? message, List<ErrorDetail>? details = null)
{
    [Required]
    public ErrorDetail Error { get; set; } = new ErrorDetail(code, message, details);
}

public class ErrorDetail(string code, string? message, List<ErrorDetail>? details)
{
    [Required]
    public string Code { get; set; } = code;

    public string? Message { get; set; } = message ?? string.Empty;

    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ErrorDetail>? Details { get; set; } = details?.Count > 0 ? details : null;
}
