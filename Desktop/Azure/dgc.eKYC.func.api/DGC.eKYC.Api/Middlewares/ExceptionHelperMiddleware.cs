using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Errors;

namespace DGC.eKYC.Api.Middlewares;

public class ExceptionHelperMiddleware(
    RequestDelegate requestDelegate,
    IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly RequestDelegate _requestDelegate = requestDelegate;

    public async Task Invoke(HttpContext httpCtx)
    {
        try
        {
            await _requestDelegate.Invoke(httpCtx);
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
            httpCtx.Response.StatusCode = ex.StatusCode;
            await httpCtx.Response.WriteAsJsonAsync(ex.Value);
        }
        catch (Exception ex)
        {
            httpCtx.Response.StatusCode = 500;
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

                await httpCtx.Response.WriteAsJsonAsync(errorResponse);
            }
            else
            {
                // Generic error response for production
                var errorResponse = new ErrorResponse("internal_server_error", "An unexpected error occurred. Please try again later.", []);
                await httpCtx.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}