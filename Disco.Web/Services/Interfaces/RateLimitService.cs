namespace Disco.Web.Services;

public interface IRateLimitService
{
    Task<bool> TryIncrementResource(string resource, int maxAllowed, TimeSpan? expiry = null);
}