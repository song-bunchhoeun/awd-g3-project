using System.ComponentModel.DataAnnotations;
using DGC.eKYC.Business.DTOs.SecurityBaseRequest;

namespace DGC.eKYC.Business.Validations.ModelState;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class SuperAppUserLevelValidation : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        try
        {
            if (value is null)
            {
                throw new Exception();
            }

            var isValid = SuperAppUserLevel.IsValid(value);

            if (!isValid)
            {
                throw new Exception();
            }

            return ValidationResult.Success;
        }
        catch
        {
            return new ValidationResult(
                "The value is a recognized User Level Id; Must be Int eg: 0, 1, 2 or String L1, L2, L3",
                [validationContext.MemberName ?? string.Empty]);
        }
    }
}

