namespace Disco.Web.Services;

public class CacheEntry
{
    public object value { get; set; }
    public DateTime? expiry { get; set; }
}

public class MemoryCacheService : ICacheService
{
    private Dictionary<string, CacheEntry> cache = new();
    private Object mutex = new();
    
    public Task<Tuple<bool, T>> TryGetAsync<T>(string key) where T : class
    {
        lock (mutex)
        {
            if (cache.TryGetValue(key, out var result))
            {
                if (result.expiry != null && result.expiry < DateTime.UtcNow)
                {
                    cache.Remove(key);
                    return Task.FromResult(new Tuple<bool, T>(false, default(T)));
                }

                return Task.FromResult(new Tuple<bool, T>(true, result.value as T));
            }

            return Task.FromResult(new Tuple<bool, T>(false, default(T)));
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)  where T : class
    {
        lock (mutex)
        {
            cache[key] = new CacheEntry()
            {
                value = value,
                expiry = expiry == null ? null : DateTime.UtcNow + expiry,
            };
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        lock (mutex)
        {
            cache.Remove(key);
        }

        return Task.CompletedTask;
    }
}