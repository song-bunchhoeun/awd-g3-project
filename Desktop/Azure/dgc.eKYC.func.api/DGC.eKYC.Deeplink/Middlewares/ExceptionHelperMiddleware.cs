using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Errors;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.Json;
using Azure.Core.Serialization;

namespace DGC.eKYC.Deeplink.Middlewares;

public class ExceptionHelperMiddleware(IConfiguration configuration, JsonObjectSerializer serializerOptions) : IFunctionsWorkerMiddleware
{
    private readonly IConfiguration _configuration = configuration;

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request == null)
            return;

        try
        {
            await next.Invoke(context);
        }
        //catch (SecurityTokenExpiredException ex)
        //{
        //    httpCtx.Response.StatusCode = 401;
        //    var response = ResponseDto<object>.Success(null, ex.Message, 401);
        //    await httpCtx.Response.WriteAsJsonAsync(response);
        //}
        //catch (SecurityTokenInvalidIssuerException ex)
        //{
        //    httpCtx.Response.StatusCode = 403;
        //    var response = ResponseDto<object>.Success(null, ex.Message, 403);
        //    await httpCtx.Response.WriteAsJsonAsync(response);
        //}
        //catch (SecurityTokenInvalidAudienceException ex)
        //{
        //    httpCtx.Response.StatusCode = 403;
        //    var response = ResponseDto<object>.Success(null, ex.Message, 403);
        //    await httpCtx.Response.WriteAsJsonAsync(response);
        //}
        catch (CustomHttpResponseException ex)
        {
            var response = request.CreateResponse((HttpStatusCode)ex.StatusCode);
            await response.WriteAsJsonAsync(ex.Value, serializerOptions);
            context.GetInvocationResult().Value = response;
        }
        catch (Exception ex)
        {
            var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
            if (!string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase))
            {
                var errorDetails = new List<ErrorDetail>
                {
                    new("Message", ex.Message, []),
                    new("Source", ex.Source, []),
                    new("StackTrace", ex.StackTrace, [])
                };

                var errorResponse =
                    new ErrorResponse("internal_server_error", "An unexpected error occurred.", errorDetails);

                var response = request.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(errorResponse);
                context.GetInvocationResult().Value = response;
            }
            else
            {
                // Generic error response for production
                var errorResponse = new ErrorResponse("internal_server_error", "An unexpected error occurred. Please try again later.", []);
                var response = request.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(errorResponse);
                context.GetInvocationResult().Value = response;
            }
        }
    }
}