using System.ComponentModel.DataAnnotations;
using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.DocumentType;

namespace DGC.eKYC.Business.Validations.ModelState;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class DocumentTypeValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        try
        {
            
            if (Enum.IsDefined(typeof(EKycDocumentType), value ?? throw new CustomHttpResponseException(400)))
            {
                return ValidationResult.Success;
            }

            throw new CustomHttpResponseException(400, "Invalid Document Type Id");

        }
        catch
        {

            return new ValidationResult("The value is a recognized Document Type Id; eg: 1. NID, 2. DGPass, 3.Passport, 4. DrivingLicense");
        }
    }
}

