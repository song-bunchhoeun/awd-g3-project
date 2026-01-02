using System.ComponentModel.DataAnnotations;
using DGC.eKYC.Business.DTOs.CustomExceptions;
using PhoneNumbers;

namespace DGC.eKYC.Business.Validations.ModelState;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class CambodianPhoneNumberValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        try
        {
            if (value is not string str)
            {
                throw new CustomHttpResponseException(400, "field value is not string");
            }

            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            var phoneNumberObject = phoneNumberUtil.Parse(str, "KH");
            if (!phoneNumberUtil.IsValidNumber(phoneNumberObject))
            {
                throw new CustomHttpResponseException(400, "phone number is not valid by google libphonenumber format");
            }

            return ValidationResult.Success;
        }
        catch
        {
            return new ValidationResult("The value is not a Cambodian Phone Number type. eg: 017123456", new[] { "PhoneNumber" });
        }
    }
}