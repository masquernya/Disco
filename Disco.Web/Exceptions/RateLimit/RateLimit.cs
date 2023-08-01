using System.Net;

namespace Disco.Web.Exceptions.RateLimit;

public class RateLimitReachedException : BaseWebException
{
    public RateLimitReachedException() : base(HttpStatusCode.TooManyRequests, "Rate limit exceeded for this resource")
    {
        
    }
}