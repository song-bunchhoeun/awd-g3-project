using Microsoft.Extensions.Configuration;

namespace DGC.eKYC.Deeplink.Extensions;

public static class EnvConfigExtension
{
    public static void Default(this IConfigurationBuilder configBuilder)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
            configBuilder.AddUserSecrets<Program>();

        configBuilder.AddEnvironmentVariables();
    }
}