using DGC.eKYC.Business.DTOs.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace DGC.eKYC.Deeplink.Functions.Base;

public abstract class BaseFunction(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Generic helper: deserializes JSON body into TModel, validates DataAnnotations (including custom attributes),
    /// and on success invokes the provided handler with the validated model.
    /// Returns BadRequest on invalid JSON / missing body / validation errors.
    /// Use this from any function method to support different models per method.
    /// </summary>
    protected async Task<IActionResult> ExecuteAsync<TModel>(HttpRequest req, Func<HttpRequest, TModel, CancellationToken, Task<IActionResult>> handler, CancellationToken cancellationToken)
    {
        TModel? model;
        try
        {
            model = await req.ReadFromJsonAsync<TModel>();
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { errors = new[] { "Invalid JSON body.", ex.Message } });
        }

        if (model == null)
        {
            return new BadRequestObjectResult(new { errors = new[] { "Request body is required and must be valid JSON." } });
        }

        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model);

        if (Validator.TryValidateObject(model, ctx, results, validateAllProperties: true))
            return await handler(req, model, cancellationToken);

        // Retrieve the environment from IConfiguration
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"];

        if (string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase))
        {
            // Hide details in production
            return new BadRequestObjectResult(new ErrorResponse("bad_parameters", "needed parameters is not met for this API!", []));
        }
        else
        {
            // Show detailed validation errors in non-production environments
            var errors = results
                .Select(ms => new ErrorDetail(string.Join(',', ms.MemberNames), ms.ErrorMessage?.ToString() ?? "this parameter is needed!", []))
                .ToList();

            return new BadRequestObjectResult(new ErrorResponse("bad_request", "Validation failed for the request.", errors));
        }

    }
}