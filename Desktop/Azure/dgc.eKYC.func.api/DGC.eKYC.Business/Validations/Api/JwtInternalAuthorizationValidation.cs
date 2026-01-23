using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Errors;
using DGC.eKYC.Business.Services.Jwt;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DGC.eKYC.Business.Validations.Api;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class JwtInternalAuthorizationValidation : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Resolve the service
        var jwtService = context.HttpContext.RequestServices.GetService<IJwtService>()
                         ?? throw new InvalidOperationException("IJwtService not registered in DI container.");

        // 2. Extract header
        if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var header))
            throw new CustomHttpResponseException(
                400,
                new ErrorResponse(
                    "bad_request",
                    "Validation failed for the request.",
                    [new ErrorDetail(
                        "missing_authorization_header", 
                        "Authorization Header is required for this endpoint")]));

        // 3. Validate
        jwtService.ValidateToken(header);
        await next();
    }
}