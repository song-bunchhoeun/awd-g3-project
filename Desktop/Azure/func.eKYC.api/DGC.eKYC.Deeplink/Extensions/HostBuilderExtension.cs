using DGC.eKYC.Deeplink.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

namespace DGC.eKYC.Deeplink.Extensions;

public static class HostBuilderExtension
{
    public static void AddDefault(IFunctionsWorkerApplicationBuilder workerApplication)
    {
        workerApplication.ConfigureMiddlewares();
    }

    private static IFunctionsWorkerApplicationBuilder ConfigureMiddlewares(
        this IFunctionsWorkerApplicationBuilder workerApplication)
    {
        workerApplication.UseMiddleware<ExceptionHelperMiddleware>();
        workerApplication.UseMiddleware<ApiVersionMiddleware>();
        return workerApplication;
    }
}