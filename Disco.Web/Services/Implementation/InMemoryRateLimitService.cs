namespace Disco.Web.Services;

public class InMemoryRateLimitService : IRateLimitService
{
    private Dictionary<string, List<DateTime>> rateLimit { get; } = new();
    private Object rateLimitMux { get; } = new();
    
    public Task<bool> TryIncrementResource(string resource, int maxAllowed, TimeSpan? expiry = null)
    {
        expiry ??= TimeSpan.FromMinutes(5);
        var time = DateTime.UtcNow.Subtract(expiry.Value);
        lock (rateLimitMux)
        {
            if (rateLimit.ContainsKey(resource))
            {
                rateLimit[resource] = rateLimit[resource].Where(a => a >= time).ToList();
            }
            else
            {
                rateLimit[resource] = new List<DateTime>();
            }
            if (rateLimit[resource].Count >= maxAllowed)
            {
                return Task.FromResult(false);
            }
            rateLimit[resource].Add(DateTime.UtcNow);
            return Task.FromResult(true);
        }
    }
}