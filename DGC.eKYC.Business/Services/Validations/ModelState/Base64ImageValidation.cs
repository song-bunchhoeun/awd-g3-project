using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using DGC.eKYC.Business.DTOs.CustomExceptions;

namespace DGC.eKYC.Business.Services.Validations.ModelState;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class Base64ImageValidationAttribute : ValidationAttribute
{
    // Optional: pattern for basic Base64 validation
    private static readonly Regex Base64Regex = new Regex(@"^[a-zA-Z0-9\+/]*={0,2}$", RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        try
        {
            var base64String = value as string;

            if (string.IsNullOrEmpty(base64String))
            {
                throw new CustomHttpResponseException(400);
            }

            // Remove data URI scheme if present, e.g. "data:image/png;base64,..."
            if (base64String.Contains(","))
            {
                base64String = base64String.Substring(base64String.IndexOf(",") + 1);
            }

            // Basic Base64 format check
            if (!Base64Regex.IsMatch(base64String))
            {
                return new ValidationResult("Invalid Base64 string format.");
            }


            var imageBytes = Convert.FromBase64String(base64String);

            // Quick check of file signatures for common image formats (PNG, JPEG, GIF, BMP)
            if (!IsValidImageHeader(imageBytes))
            {
                return new ValidationResult("The Base64 string does not represent a valid image.");
            }

            return ValidationResult.Success;
        }
        catch (Exception)
        {
            return new ValidationResult("Invalid Base64 string.");
        }
    }

    private static bool IsValidImageHeader(byte[] bytes)
    {
        if (bytes.Length < 4) return false;

        // PNG signature: 89 50 4E 47
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return true;

        // JPEG signature: FF D8 FF
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return true;

        // GIF signature: GIF87a or GIF89a (47 49 46 38 37 61 or 47 49 46 38 39 61)
        if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
            return true;

        // BMP signature: 42 4D
        if (bytes[0] == 0x42 && bytes[1] == 0x4D)
            return true;

        return false;
    }
}