using Microsoft.Extensions.Caching.Distributed;

namespace CurrencyConverter.WebApi.Services;

public class DefaultRateLimitingService
{
    private readonly IDistributedCache _cache;
    private const int Threshold = 50;

    public DefaultRateLimitingService(
        IDistributedCache distributedCache)
    {
        _cache = distributedCache;
    }

    public async Task<bool> IsRequestAllowedAsync(string redisKey)
    {
        var cacheEntry = await _cache.GetStringAsync(redisKey);
        var count = 0;

        if (cacheEntry != null)
        {
            count = int.Parse(cacheEntry);
        }

        if (count >= Threshold)
        {
            return false;
        }

        count++;

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
        };

        await _cache.SetStringAsync(redisKey, count.ToString(), options);

        return true;
    }
}