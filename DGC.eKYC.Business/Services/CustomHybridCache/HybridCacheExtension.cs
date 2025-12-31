using Microsoft.Extensions.Caching.Hybrid;

namespace DGC.eKYC.Business.Services.CustomHybridCache;

public static class HybridCacheExtensions
{
    public static async Task<T?> GetAsync<T>(this HybridCache cache, string key, CancellationToken cancellationToken)
    {
        var options = new HybridCacheEntryOptions
        {
            Flags = HybridCacheEntryFlags.DisableUnderlyingData |
                    HybridCacheEntryFlags.DisableLocalCacheWrite |
                    HybridCacheEntryFlags.DisableDistributedCacheWrite
        };

        var result = await cache.GetOrCreateAsync<T>(
            key,
            factory: null!,
            options,
            cancellationToken: cancellationToken
        );

        return result;
    }
}