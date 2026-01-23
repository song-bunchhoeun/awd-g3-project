namespace DGC.eKYC.Business.Validations.ModelState;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public partial class CallBackUrlValidationAttribute : ValidationAttribute
{
    // Regex for Mobile Schemes: starts with alpha, can contain dots/hyphens, followed by ://
    // Regex for Web: standard http/https patterns
    [GeneratedRegex(@"^([a-zA-Z][a-zA-Z0-9+.-]*:\/\/)|(https?:\/\/)")]
    private static partial Regex SchemeRegex();

    protected override ValidationResult? IsValid(object? callbackUrl, ValidationContext validationContext)
    {
        try
        {
            if (callbackUrl is not string url || string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(callbackUrl), "Url is not the correct data type for callback URI");

            if (url.Length > 512) // Common maximum URL length
                throw new ArgumentOutOfRangeException(nameof(callbackUrl), "The callback URI exceeds the maximum allowed length of 512 ASCII characters.");

            // 1. Check basic URI well-formedness
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentNullException(nameof(callbackUrl), "The string is not a well-formed absolute URI.");

            // 2. Validate Scheme via Regex (Supports https://, http://, and myapp://)
            if (!SchemeRegex().IsMatch(url))
                throw new ArgumentNullException(nameof(callbackUrl), "This scheme is not allowed for callback URI");

            return ValidationResult.Success;
        }
        catch (Exception ex)
        {
            return new ValidationResult(ex.Message, ["CallBackUrl"]);
        }

    }
}