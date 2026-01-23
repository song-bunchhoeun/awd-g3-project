namespace DGC.eKYC.Api.Extensions;

public static class LoggerExtension
{
    public static void ConfigureLogger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();
    }
}
