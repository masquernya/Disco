namespace Disco.Web.Services;

public interface ICacheService
{
    Task<Tuple<bool, T>> TryGetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    Task RemoveAsync(string key);
}

public interface ICacheHelperService
{
    Task<T> GetWithFallback<T>(string key, TimeSpan expiration, Func<Task<T>> cb) where T : class;
    Task RemoveAsync(string key);
}

public class CacheHelperService : ICacheHelperService
{
    private ICacheService cache;
    private ILogger logger;
    public CacheHelperService(ICacheService cache, ILogger logger)
    {
        this.cache = cache;
        this.logger = logger;
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }
    
    public async Task<T> GetWithFallback<T>(string key, TimeSpan expiration, Func<Task<T>> cb) where T : class
    {
        var cached = await cache.TryGetAsync<T>(key);
        if (cached.Item1)
        {
            logger.LogInformation("Cache hit for {key}", key);
            return cached.Item2;
        }

        logger.LogInformation("Cache miss for {key}", key);
        var answer = await cb();
        await cache.SetAsync(key, answer, expiration);
        return answer;
    }
}