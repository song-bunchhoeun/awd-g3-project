using System.ComponentModel.DataAnnotations;
namespace DGC.eKYC.Business.Validations.ModelState;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class UnixTimeSecondsValidationAttribute : ValidationAttribute
{
    // 2038-01-19 03:14:07 UTC (max for signed 32-bit seconds)
    private const long MaxUnixSeconds = 2_147_483_647L;

    protected override ValidationResult? IsValid(object? timestamp, ValidationContext ctx)
    {
        try
        {
            if (timestamp is not long unix)
                throw new ArgumentNullException(nameof(timestamp), "Timestamp is not the correct data type for UNIX Timestamp");

            if (unix is <= 0 or >= MaxUnixSeconds)
                throw new ArgumentNullException(nameof(timestamp), "The field {0} must be a valid Unix timestamp in seconds " +
                                                                   "(between 0 and 2 147 483 647).");
            return ValidationResult.Success;
        }
        catch (Exception ex)
        {
            return new ValidationResult(ex.Message, ["CallBackUrl"]);
        }
        
    }
}