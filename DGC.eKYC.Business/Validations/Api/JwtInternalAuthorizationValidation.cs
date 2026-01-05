using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Errors;
using DGC.eKYC.Business.Services.Jwt;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DGC.eKYC.Business.Validations.Api;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class JwtInternalAuthorizationValidation : ActionFilterAttribute
{
    private const string Scheme = "Bearer";

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
                    [new ErrorDetail("missing_authorization_header", "Authorization Header is required for this endpoint")]));

        // 3. Split "Bearer <token>"
        var parts = header.ToString().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !parts[0].Equals(Scheme, StringComparison.OrdinalIgnoreCase))
            throw new CustomHttpResponseException(
                400,
                new ErrorResponse(
                    "bad_request",
                    "Validation failed for the request.",
                    [new ErrorDetail("bad_authorization_header", "Bearer Authorization Header is required for this endpoint")]));

        var token = parts[1];

        // 4. Validate
        try
        {
            var valid = jwtService.IsTokenValid(token);
            if (!valid)
                throw new CustomHttpResponseException(
                    410,
                    new ErrorResponse(
                        "bad_request",
                        "Authorization Validation failed for the request."));

            await next();
        }
        catch (Exception ex)
        when (ex is not CustomHttpResponseException)
        {
            throw new CustomHttpResponseException(
                500,
                new ErrorResponse(
                    "internal_validation_error",
                    "Validation failed for the request."));
        }
    }
}