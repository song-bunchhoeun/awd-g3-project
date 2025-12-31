using System.Text.Json;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace DGC.eKYC.Deeplink.Extensions;

public static class ServicesExtension
{
    public static void AddDefault(this IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var defaultJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Use camelCase for JSON properties
            WriteIndented = true,                              // Pretty print the JSON
            AllowTrailingCommas = true,                        // Allow trailing commas during deserialization
        };

        var defaultJsonObjSerDeOpt = new JsonObjectSerializer(defaultJsonSerializerOptions);

        services.AddSingleton(defaultJsonObjSerDeOpt);
    }
}