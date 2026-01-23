using DGC.eKYC.Dal.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace DGC.eKYC.Api.Extensions;

public static class DatabaseExtension
{
    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
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

    public static void ConfigureRedisCache(this IServiceCollection services, IConfiguration configuration)
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
