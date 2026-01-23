using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Errors;
using DGC.eKYC.Deeplink.Attributes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Reflection;

namespace DGC.eKYC.Deeplink.Middlewares;

public class ApiVersionMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        await ValidateApiVersion(context, next);
        await next(context); // Executes the next middleware or the function itself
    }

    private async Task ValidateApiVersion(FunctionContext context, FunctionExecutionDelegate next)
    {
        // Try to validate API version declared on the function (via ApiVersionAttribute)
        var request = await context.GetHttpRequestDataAsync();

        if (request == null)
            return;

        // Determine if the target function or its method has an ApiVersionAttribute
        var entryPoint = context.FunctionDefinition?.EntryPoint; // e.g. "Namespace.Class.Method"
        if (string.IsNullOrEmpty(entryPoint))
            return;

        var lastDot = entryPoint.LastIndexOf('.');
        if (lastDot > 0 && lastDot < entryPoint.Length - 1)
        {
            var typeName = entryPoint.Substring(0, lastDot);
            var methodName = entryPoint.Substring(lastDot + 1);

            // Try to locate the type in loaded assemblies
            Type? targetType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                targetType = asm.GetType(typeName, throwOnError: false, ignoreCase: false);
                if (targetType != null)
                    break;
            }

            if (targetType == null)
                return;

            var method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            // Look for ApiVersionAttribute on the method first, then on the declaring type
            var apiVersionAttr = method?.GetCustomAttribute<ApiVersionAttribute>() ?? targetType.GetCustomAttribute<ApiVersionAttribute>();

            if (apiVersionAttr == null)
                throw new ArgumentNullException(nameof(ApiVersionAttribute), "missing api version attribute");

            // Try to get version from header "x-api-version" or query string "api-version"
            string? providedVersion = null;

            if (request.Headers.TryGetValues("x-api-version", out var headerValues))
            {
                providedVersion = headerValues?.FirstOrDefault();
            }

            if (providedVersion == null)
            {
                // parse query string manually
                var query = request.Url.Query; // starts with '?' or empty
                if (!string.IsNullOrEmpty(query))
                {
                    var qs = query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var kv in qs)
                    {
                        var parts = kv.Split('=', 2);
                        if (parts.Length == 2 && string.Equals(parts[0], "api-version", StringComparison.OrdinalIgnoreCase))
                        {
                            providedVersion = System.Net.WebUtility.UrlDecode(parts[1]);
                            break;
                        }
                    }
                }
            }

            if (providedVersion == null)
                throw new CustomHttpResponseException(
                    400,
                    new ErrorResponse(
                        "missing_api_response",
                        "API version required. Provide via header 'x-api-version' or query 'api-version'.",
                        []));

            // Parse provided version as ISO date yyyy-MM-dd
            var apiVersionDateValidated = DateOnly.TryParseExact(providedVersion, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None,
                out var providedDate);

            if (!apiVersionDateValidated)
                throw new CustomHttpResponseException(
                    400,
                    new ErrorResponse(
                        "bad_api_response",
                        $"api version must use ISO date yyyy-MM-dd, e.g. 2025-05-08. Provided: '{providedVersion}'.!",
                        []));

            if (providedDate != apiVersionAttr.ExpectedDate)
                throw new CustomHttpResponseException(
                    400,
                    new ErrorResponse(
                        "bad_api_response",
                        $"API version mismatch. Provided: '{providedVersion}'.",
                        []));
        }
    }
}