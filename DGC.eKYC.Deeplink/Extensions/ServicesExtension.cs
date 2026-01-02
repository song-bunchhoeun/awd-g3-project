using Azure.Core.Serialization;
using DGC.eKYC.Business.Services.Deeplink;
using DGC.eKYC.Business.Services.Jwt;
using DGC.eKYC.Dal.Contexts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Text.Json;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace DGC.eKYC.Deeplink.Extensions;

public static class ServicesExtension
{
    public static void AddDefault(this IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetService<IConfiguration>() ??
                            throw new ArgumentNullException(nameof(services), "cannot resolve IConfiguration");

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.ConfigureDatabase(configuration);
        services.ConfigureRedisCache(configuration);

        var defaultJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Use camelCase for JSON properties
            WriteIndented = true,                              // Pretty print the JSON
            AllowTrailingCommas = true,                        // Allow trailing commas during deserialization
        };

        var defaultJsonObjSerDeOpt = new JsonObjectSerializer(defaultJsonSerializerOptions);

        services.AddSingleton(defaultJsonObjSerDeOpt);
        services.AddSingleton<IJwtService, FunctionJwtService>();
        services.AddScoped<IDeeplinkService, DeeplinkService>();

    }

    private static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EKycContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultDbConnection"),
                builder =>
                {
                    builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                    builder.CommandTimeout(30);
                    builder.MaxBatchSize(100); // Optimize batch operations
                });
        });
    }

    private static void ConfigureRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("DefaultRedisConnection")
                                          ?? throw new InvalidOperationException("DefaultRedisConnection is empty")));

        services.AddFusionCache()
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithDistributedCache(_ =>
            {
                var connectionString = configuration.GetConnectionString("DefaultRedisConnection");
                var options = new RedisCacheOptions { Configuration = connectionString };

                return new RedisCache(options);
            })
            .AsHybridCache();
    }
}