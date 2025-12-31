using System.ComponentModel.DataAnnotations;
using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.ProfileBase;

namespace DGC.eKYC.Business.Services.Validations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class GenderValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        try
        {
            if (Gender.IsValid(value))
            {
                return ValidationResult.Success;
            }

            throw new CustomHttpResponseException(400, "Invalid Gender");

        }
        catch
        {

            return new ValidationResult($"The value {value} is a recognized gender: Allowed Gender are {string.Join(",", Gender.AllowedGender)}");
        }
    }
}

