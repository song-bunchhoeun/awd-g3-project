using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Errors;
using DGC.eKYC.Business.DTOs.SuperAppSecurity;
using DGC.eKYC.Business.Services.HashService;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DGC.eKYC.Business.Services.Validations.Api;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SuperAppHashValidation : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var hashCompute = context.HttpContext.RequestServices.GetRequiredService<IHashCompute>() ??
                          throw new CustomHttpResponseException(500, new ErrorResponse("di-error", "", []));
        var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>() ??
                            throw new CustomHttpResponseException(500, new ErrorResponse("di-error", "", []));

        try
        {
            if (context.ActionArguments.Values
                    .FirstOrDefault(arg => arg is SuperAppSecurityBaseInput) is SuperAppSecurityBaseInput securityInput)
            {
                var isValid = hashCompute.ValidateCheckSum(securityInput.InitData);

                if (!isValid)
                {
                    throw new CustomHttpResponseException(403,
                        new ErrorResponse("forbidden", "Hash Key is invalid", []));
                }

                hashCompute.PopulateSecurityField(securityInput, securityInput.InitData);
                hashCompute.ValidateSecurityFieldInput(securityInput);

                var initDataExpirationSecond = configuration.GetValue<int>("ApiKeyExpiration", 10);
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var createdTime = epoch.AddSeconds(securityInput.TimeStamp ?? 0d);
                var now = DateTime.UtcNow;
                var elapsed = now - createdTime;

                if (elapsed.TotalSeconds >= initDataExpirationSecond)
                {
                    throw new CustomHttpResponseException(403,
                        new ErrorResponse("forbidden", "Hash Key is expired", []));
                }
            }
            else
            {
                throw new Exception("Hash Key is missing");
            }
        }
        catch (CustomHttpResponseException)
        {
            throw;
        }
        catch
        {
            throw new CustomHttpResponseException(403, new ErrorResponse("forbidden", "Hash Key is invalid", []));
        }
    }
}