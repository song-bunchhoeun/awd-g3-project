using System.ComponentModel.DataAnnotations;
using DGC.eKYC.Business.DTOs.CustomExceptions;

namespace DGC.eKYC.Business.Services.Validations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class OtpCodeValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        try
        {
            if (value is not string str)
            {
                throw new CustomHttpResponseException(400, "field value is not string");
            }

            if (!int.TryParse(str, out int otpCode))
            {
                throw new CustomHttpResponseException(400, "OTP code must be digit number");
            }
            
            if (otpCode < 0 || otpCode > 999999)
            {
                throw new CustomHttpResponseException(400, "OTP code must be 6 digits");
            }

            return ValidationResult.Success;
        }
        catch 
        {
            return new ValidationResult("The OTP value is not valid. It must be a 6-digit number.");
        }
    }
}